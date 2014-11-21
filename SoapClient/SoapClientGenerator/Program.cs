using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace SoapClientGenerator
{
	class Program
	{
		private const string ClientBaseClassName = "SoapServices.SoapClientBase";
		private const int HelpHeaderWidth = 30;
		private const string SvcUtilDefaultPath = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\SvcUtil.exe";

		static void Main(string[] args)
		{
			try
			{
				//Debugger.Launch();
				WriteInfo();

				if (args.Length < 3 || args.Length > 4)
				{
					Console.WriteLine("Wrong number of parameters ({0}), expected 3 or 4", args.Length);
					return;
				}

				var svcutil = SvcUtilDefaultPath;
				var wsdlUri = args[0];
				var outFilePath = args[1];
				var ns = args[2];

				var svcutilParam = GetParameters(args.Skip(3)).FirstOrDefault(p => p.Key.Equals("svcutil", StringComparison.OrdinalIgnoreCase));
				if (svcutilParam != null)
				{
					svcutil = svcutilParam.Value;
				}

				CreateProxy(svcutil, wsdlUri, outFilePath, ns);
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
			}
		}

		private static List<AppParameter> GetParameters(IEnumerable<string> args)
		{
			var result = new List<AppParameter>();

			foreach (var s in args)
			{
				if (!s.StartsWith("/"))
					throw new Exception(String.Format("Bad parameter '{0}'", s));

				var pos = s.IndexOf(':');
				if (pos == -1)
				{
					result.Add(new AppParameter(s.Substring(1)));
				}
				else
				{
					var key = s.Substring(1, pos - 1);
					var value = s.Substring(pos + 1);
					result.Add(new AppParameter(key, value));
				}
			}

			return result;
		}

		private static void WriteInfo()
		{
			Console.WriteLine("SOAP client source code generator");
			Console.WriteLine("SvcUtil.exe from .Net SDK is required");
			Console.WriteLine();

			Console.WriteLine("Syntax: SoapClientGenerator.exe <metadataDocumentPath> <file> <namespace> [/svcutil:<svcutilPath>]");
			Console.WriteLine("<metadataDocumentPath>".PadRight(HelpHeaderWidth) + " - The path to a metadata document (wsdl)");
			Console.WriteLine("<file>".PadRight(HelpHeaderWidth) + " - Output file path");
			Console.WriteLine("<namespace>".PadRight(HelpHeaderWidth) + " - Output file namespace");
			Console.WriteLine("<svcutil>".PadRight(HelpHeaderWidth) + " - SvcUtil.exe path, default {0}", SvcUtilDefaultPath);
			Console.WriteLine();

			Console.WriteLine(
				"Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement");
			Console.WriteLine(
				"Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement /svcutil:\"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools\\SvcUtil.exe\"");
			Console.WriteLine();
		}

		private static void CreateProxy(string svcutil, string wsdlUri, string outFilePath, string ns)
		{
			Console.WriteLine("SvcUtil {0}", svcutil);
			Console.WriteLine("WSDL {0}", wsdlUri);
			Console.WriteLine("Code file {0}", outFilePath);
			Console.WriteLine("Code file namespace {0}", ns);
			Console.WriteLine();

			Console.WriteLine("Processing WSDL {0}", wsdlUri);
			// 1. generate raw code
			var csFilePath = Path.GetTempFileName() + ".cs";
			GenerateRawCode(svcutil, wsdlUri, csFilePath);
			Console.WriteLine("Raw file: {0}", csFilePath);

			// 2. complile assembly
			Console.WriteLine("Compile...");
			var assembly = Compile(csFilePath);

			// 3. ParseAssembly
			Console.WriteLine("Gathering info...");
			var services = ParseAssembly(assembly);

			// 4. Generate source code
			Console.WriteLine("Code generation...");
			GeneratedCode(services, outFilePath, ns);

			Console.WriteLine("DONE!");
			Console.WriteLine();
		}

		private static void GeneratedCode(AssemblyInfo assemblyInfo, string srcFile, string ns)
		{
			var targetUnit = new CodeCompileUnit();
			var samples = new CodeNamespace(ns);
			//samples.Imports.Add(new CodeNamespaceImport("System"));

			targetUnit.Namespaces.Add(samples);

			foreach (var serviceInterface in assemblyInfo.Services)
			{
				var interfaceTypeDec = AddClientInterface(serviceInterface);
				samples.Types.Add(interfaceTypeDec);

				var implTypeDec = AddClientImplementation(serviceInterface);
				samples.Types.Add(implTypeDec);
			}

			foreach (var contractInfo in assemblyInfo.Contracts)
			{
				var codeDeclaration = contractInfo.GetCodeDeclaration();
				samples.Types.Add(codeDeclaration);
			}

			foreach (var enumInfo in assemblyInfo.Enums)
			{
				var codeDeclaration = enumInfo.GetCodeDeclaration();
				samples.Types.Add(codeDeclaration);
			}

			var provider = CodeDomProvider.CreateProvider("CSharp");
			var options = new CodeGeneratorOptions();
			options.BracingStyle = "C";
			options.BlankLinesBetweenMembers = true;
			using (var sourceWriter = new StreamWriter(srcFile))
			{
				provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
			}

			// Fix auto props declaration
			var srcContent = File.ReadAllText(srcFile);
			srcContent = srcContent.Replace("};", "}");
			File.WriteAllText(srcFile, srcContent);
		}

		private static CodeTypeDeclaration AddClientImplementation(ServiceInterface serviceInterface)
		{
			var classTypeDec = new CodeTypeDeclaration(serviceInterface.Name + "Client");
			classTypeDec.IsClass = true;
			classTypeDec.IsPartial = true;
			classTypeDec.TypeAttributes |= TypeAttributes.Public;

			classTypeDec.BaseTypes.Add(ClientBaseClassName);
			classTypeDec.BaseTypes.Add(new CodeTypeReference(serviceInterface.Name));

			foreach (var serviceMethod in serviceInterface.Methods)
			{
				var mth = new CodeMemberMethod();
				mth.Attributes = MemberAttributes.Public;
				mth.Name = serviceMethod.Name;
				mth.ReturnType = new CodeTypeReference(serviceMethod.MethodInfo.ReturnType);

				var parameterName = "request";
				var parameterInfos = serviceMethod.MethodInfo.GetParameters();
				Type sendType = parameterInfos[0].ParameterType;

				foreach (var parameterInfo in parameterInfos)
				{
					var bodyMember =
						parameterInfo.ParameterType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
							.FirstOrDefault(m => m.GetCustomAttribute<System.ServiceModel.MessageBodyMemberAttribute>() != null);

					if (bodyMember != null)
					{
						parameterName += "." + bodyMember.Name;
						var fieldInfo = bodyMember as FieldInfo;
						if (fieldInfo != null)
						{
							sendType = fieldInfo.FieldType;
						}

						var propertyInfo = bodyMember as PropertyInfo;
						if (propertyInfo != null)
						{
							sendType = propertyInfo.PropertyType;
						}
					}

					mth.Parameters.Add(new CodeParameterDeclarationExpression(parameterInfo.ParameterType, parameterInfo.Name));
				}

				var returnStatement = new CodeMethodReturnStatement();

				var invokeExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "CallAsync", new CodePrimitiveExpression(serviceMethod.Action), new CodeVariableReferenceExpression(parameterName));
				invokeExpression.Method.TypeArguments.Add(sendType);
				invokeExpression.Method.TypeArguments.Add(serviceMethod.MethodInfo.ReturnType.GenericTypeArguments[0]);
				returnStatement.Expression = invokeExpression;

				mth.Statements.Add(returnStatement);

				classTypeDec.Members.Add(mth);
			}

			return classTypeDec;
		}

		private static CodeTypeDeclaration AddClientInterface(ServiceInterface serviceInterface)
		{
			var interfaceTypeDec = new CodeTypeDeclaration(serviceInterface.Name);
			interfaceTypeDec.IsInterface = true;

			foreach (var serviceMethod in serviceInterface.Methods)
			{
				var mth = new CodeMemberMethod();
				mth.Name = serviceMethod.Name;
				mth.ReturnType = new CodeTypeReference(serviceMethod.MethodInfo.ReturnType);

				foreach (var parameterInfo in serviceMethod.MethodInfo.GetParameters())
				{
					mth.Parameters.Add(new CodeParameterDeclarationExpression(parameterInfo.ParameterType, parameterInfo.Name));
				}

				interfaceTypeDec.Members.Add(mth);
			}
			return interfaceTypeDec;
		}

		private static AssemblyInfo ParseAssembly(Assembly assembly)
		{
			var services = from t in assembly.DefinedTypes
						   let serviceContractAttr = t.GetCustomAttribute<ServiceContractAttribute>()
						   where serviceContractAttr != null
						   select new ServiceInterface(t);

			var contracts = from t in assembly.DefinedTypes
							let serviceContractAttr = t.GetCustomAttribute<ServiceContractAttribute>()
							where serviceContractAttr == null
							&& !t.ImplementedInterfaces.Contains(typeof(IClientChannel))
							&& !t.BaseType.IsSubclassOf(typeof(ClientBase<>))
							&& !(t.BaseType.Namespace == typeof(ClientBase<>).Namespace && t.BaseType.Name == typeof(ClientBase<>).Name)
							&& !t.IsEnum
							select new ContractInfo(t);

			var enums = from t in assembly.DefinedTypes
						where t.IsEnum
						select new EnumInfo(t);

			return new AssemblyInfo()
			{
				Services = new List<ServiceInterface>(services),
				Contracts = new List<ContractInfo>(contracts),
				Enums = new List<EnumInfo>(enums)
			};
		}

		private static Assembly Compile(string csFilePath)
		{
			var codeDomProvider = CodeDomProvider.CreateProvider("CS");

			var compilerParameters = new CompilerParameters()
			{
				GenerateExecutable = false,
				GenerateInMemory = true,
			};

			compilerParameters.ReferencedAssemblies.Add("System.dll");
			compilerParameters.ReferencedAssemblies.Add("System.Xml.dll");
			compilerParameters.ReferencedAssemblies.Add("System.ServiceModel.dll");
			compilerParameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");

			var compilerResults = codeDomProvider.CompileAssemblyFromFile(compilerParameters, csFilePath);
			var assembly = compilerResults.CompiledAssembly;
			return assembly;
		}

		private static void GenerateRawCode(string svcutilPath, string wsdlUri, string outFile)
		{
			if (!File.Exists(svcutilPath))
				throw new Exception("svcutil is not found");

			var processStartInfo = new ProcessStartInfo(svcutilPath);
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			var tempFileName = Path.GetTempFileName();
			processStartInfo.Arguments = String.Format("\"{0}\" /noconfig /nologo /t:code /mc /out:\"{1}\"", wsdlUri, tempFileName);

			Process.Start(processStartInfo).WaitForExit();

			File.Copy(tempFileName + ".cs", outFile, true);
		}
	}

	class AppParameter
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public AppParameter(string key, string value = null)
		{
			Key = key;
			Value = value;
		}
	}
}
