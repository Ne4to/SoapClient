using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.Xml.Linq;
using SoapClientBuilder;

namespace SoapClientGenerator
{
	class Program
	{
		private const int HelpHeaderWidth = 30;

		static void Main(string[] args)
		{
			try
			{
				//var wsdlDoc = XDocument.Parse(File.ReadAllText(wsdlFilename));
				//var builder = new ProxyBuilder(wsdlDoc, new BuilderParameters()
				//{
				//	CodeNamespace = ns
				//});
				//builder.Run();

				//var sourceCode = builder.GetSourceCode();

				//File.WriteAllText(@"C:\Users\Alexey\Source\Repos\SoapClient\SoapClient\WcfService1ClientApp\" + outFileName, sourceCode);


				//Debugger.Launch();
				WriteInfo();

				var appParameters = Utils.GetParameters(args);
				var action = appParameters.GetParameter("action");
				if (action == null)
				{
					Console.WriteLine("action parameter is required");
					return;
				}

				switch (action)
				{
					case "downloadWsdl":
						DownloadWsdl(appParameters);
						return;

					case "generateCode":
						break;

					default:
						Console.WriteLine("Action parameter value is not valid");
						return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
			}
		}

		private static void DownloadWsdl(List<AppParameter> appParameters)
		{
			var wsdlUri = appParameters.GetParameter("wsdlUri");
			if (wsdlUri == null)
			{
				Console.WriteLine("wsdlUri parameter is required");
				return;				
			}

			var outPath = appParameters.GetParameter("outPath");
			if (outPath == null)
			{
				Console.WriteLine("outPath parameter is required");
				return;
			}

			WsdlDocumentLoader loader = new WsdlDocumentLoader();
			var doc = loader.LoadAsync(new Uri(wsdlUri)).Result;
			doc.Save(outPath);
		}

		private static void WriteInfo()
		{
			Console.WriteLine("SOAP client source code generator");
			Console.WriteLine();

			Console.WriteLine("Syntax: SoapClientGenerator.exe /action:downloadWsdl /wsdlUri:<wsdl file uri> /outPath:<output wsdl file path>");
			Console.WriteLine("Syntax: SoapClientGenerator.exe /action:<downloadWsdl|generateCode> <metadataDocumentPath> <file> <namespace> [/svcutil:<svcutilPath>]");
			//Console.WriteLine("<metadataDocumentPath>".PadRight(HelpHeaderWidth) + " - The path to a metadata document (wsdl)");
			//Console.WriteLine("<file>".PadRight(HelpHeaderWidth) + " - Output file path");
			//Console.WriteLine("<namespace>".PadRight(HelpHeaderWidth) + " - Output file namespace");
			//Console.WriteLine("<svcutil>".PadRight(HelpHeaderWidth) + " - SvcUtil.exe path, default {0}", SvcUtilDefaultPath);
			Console.WriteLine();

			//Console.WriteLine(
//				"Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement");
	//		Console.WriteLine(
		//		"Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement /svcutil:\"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools\\SvcUtil.exe\"");
		//	Console.WriteLine();
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

	static class Utils
	{
		public static List<AppParameter> GetParameters(IEnumerable<string> args)
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

		public static string GetParameter(this List<AppParameter> list, string key)
		{
			var p = list.FirstOrDefault(t => t.Key == key);
			return p == null ? null : p.Value;
		}
	}
}
