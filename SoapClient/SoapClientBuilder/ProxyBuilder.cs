using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoapClientBuilder
{
	// TODO XmlElementAttribute.DataType
	// TODO XmlElementAttribute.IsNullable
	// TODO XML Documentation
	public class ProxyBuilder
	{
		private const string ClientBaseClassName = "SoapServices.SoapClientBase";

		private readonly XDocument _wsdlDoc;
		private readonly BuilderParameters _parameters;

		private readonly CodeCompileUnit _codeUnit;
		private readonly CodeNamespace _codeNamespace;

		private readonly Dictionary<XName, XElement> _messageElements;
		private readonly Dictionary<XName, CodeTypeReference> _generatedTypes = new Dictionary<XName, CodeTypeReference>();
		private readonly Dictionary<XName, XElement> _schemaElements;
		private readonly Dictionary<XName, XElement> _schemaComplexTypes;
		private readonly Dictionary<XName, XElement> _schemaSimpleTypes;
		private readonly Dictionary<XName, XElement> _schemaAttributes;

		public ProxyBuilder(XDocument wsdlDoc, BuilderParameters parameters)
		{
			if (wsdlDoc == null) throw new ArgumentNullException("wsdlDoc");
			if (parameters == null) throw new ArgumentNullException("parameters");
			_wsdlDoc = wsdlDoc;
			_parameters = parameters;

			_codeUnit = new CodeCompileUnit();
			_codeNamespace = new CodeNamespace(parameters.CodeNamespace);
			//samples.Imports.Add(new CodeNamespaceImport("System"));
			_codeUnit.Namespaces.Add(_codeNamespace);

			_schemaElements = GetSchemaItems("element");
			_schemaComplexTypes = GetSchemaItems("complexType");
			_schemaSimpleTypes = GetSchemaItems("simpleType");
			_schemaAttributes = GetSchemaItems("attribute");

			_messageElements = GetMessageElements();
		}

		public void Run()
		{
			var portTypeElements = _wsdlDoc.Root.Elements(Namespaces.Wsdl + "portType");
			foreach (var portTypeElement in portTypeElements)
			{
				var portTypeName = portTypeElement.Attribute("name").Value;

				XElement bindingElement = null;
				foreach (var be in _wsdlDoc.Root.Elements(Namespaces.Wsdl + "binding"))
				{
					var typeName = GetTypeName(be, "type");
					if (typeName.LocalName == portTypeName)
					{
						bindingElement = be;
						break;
					}
				}

				if (bindingElement == null)
					continue;

				var soapBindingElement = bindingElement.Element(Namespaces.Soap + "binding");
				if (soapBindingElement == null)
				{
					soapBindingElement = bindingElement.Element(Namespaces.Soap12 + "binding");
				}

				if (soapBindingElement == null)
					continue;

				if (soapBindingElement.Attribute("transport").Value != "http://schemas.xmlsoap.org/soap/http")
					continue;

				//<soap:binding transport="http://schemas.xmlsoap.org/soap/http" xmlns:soap="http://schemas.xmlsoap.org/wsdl/soap/" />
				//<http:binding verb="GET" xmlns:http="http://schemas.xmlsoap.org/wsdl/http/" />
				var interfaceTypeDec = AddServiceInterface(portTypeName);
				var implTypeDec = AddServiceImplementation(interfaceTypeDec);

				var operations = portTypeElement.Elements(Namespaces.Wsdl + "operation");
				foreach (var operation in operations)
				{
					var operationName = operation.Attribute("name").Value;

					var opEl = bindingElement.Elements(Namespaces.Wsdl + "operation").Single(o => o.Attribute("name").Value == operationName);
					string soapAction = null;
					var soapOp = opEl.Element(Namespaces.Soap + "operation");
					if (soapOp != null)
					{
						var soapActionAttr = soapOp.Attribute("soapAction");
						soapAction = soapActionAttr == null ? null : soapActionAttr.Value;
					}

					var inputElement = operation.Element(Namespaces.Wsdl + "input");
					var inputTypeReference = Process(inputElement, true);

					var outputElement = operation.Element(Namespaces.Wsdl + "output");
					var outputTypeReference = Process(outputElement, false);

					AddInterfaceOperation(operationName, outputTypeReference, interfaceTypeDec, inputTypeReference);
					AddImplementationOperation(operationName, outputTypeReference, implTypeDec, inputTypeReference, soapAction);
				}
			}
		}

		private Dictionary<XName, XElement> GetSchemaItems(string itemName)
		{
			var typesElement = _wsdlDoc.Root.Element(Namespaces.Wsdl + "types");

			var q = from schemaElement in typesElement.Elements(Namespaces.Xsd + "schema")
					let schemaNamespace = schemaElement.Attribute("targetNamespace").Value
					from typeElement in schemaElement.Elements(Namespaces.Xsd + itemName)
					let typeName = GetTypeName(typeElement, "name", schemaNamespace)
					select new { typeName, typeElement };

			return q.ToDictionary(kvp => kvp.typeName, kvp => kvp.typeElement);
		}

		private Dictionary<XName, XElement> GetMessageElements()
		{
			var targetNamespace = _wsdlDoc.Root.Attribute("targetNamespace").Value;
			var messageElements = _wsdlDoc.Root.Elements(Namespaces.Wsdl + "message").ToArray();
			return messageElements.ToDictionary(me => GetTypeName(me, "name", targetNamespace), me => me);
		}

		private void AddInterfaceOperation(string operationName, CodeTypeReference outputTypeReference,
			CodeTypeDeclaration interfaceTypeDec, CodeTypeReference inputTypeReference)
		{
			var mth = new CodeMemberMethod();
			mth.Name = operationName + "Async";
			var typeName = String.Format("System.Threading.Tasks.Task<{0}>", outputTypeReference.BaseType);
			mth.ReturnType = new CodeTypeReference() { BaseType = typeName };

			mth.Parameters.Add(new CodeParameterDeclarationExpression(inputTypeReference.BaseType + "Request", "request"));

			interfaceTypeDec.Members.Add(mth);
		}

		private static void AddImplementationOperation(string operationName, CodeTypeReference outputTypeReference, CodeTypeDeclaration implTypeDec, CodeTypeReference inputTypeReference, string soapAction)
		{
			var mth = new CodeMemberMethod();
			mth.Attributes = MemberAttributes.Public;
			mth.Name = operationName + "Async";
			var typeName = String.Format("System.Threading.Tasks.Task<{0}>", outputTypeReference.BaseType);
			mth.ReturnType = new CodeTypeReference() { BaseType = typeName };

			mth.Parameters.Add(new CodeParameterDeclarationExpression(inputTypeReference.BaseType + "Request", "request"));

			var returnStatement = new CodeMethodReturnStatement();

			var invokeExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
				"CallAsync",
				new CodePrimitiveExpression(soapAction),
				new CodeVariableReferenceExpression("request"));
			invokeExpression.Method.TypeArguments.Add(inputTypeReference.BaseType + "Request");
			invokeExpression.Method.TypeArguments.Add(outputTypeReference.BaseType);
			returnStatement.Expression = invokeExpression;

			mth.Statements.Add(returnStatement);

			implTypeDec.Members.Add(mth);
		}

		private CodeTypeDeclaration AddServiceInterface(string portTypeName)
		{
			var interfaceTypeDec = new CodeTypeDeclaration(portTypeName);
			interfaceTypeDec.IsInterface = true;
			_codeNamespace.Types.Add(interfaceTypeDec);
			return interfaceTypeDec;
		}

		private CodeTypeDeclaration AddServiceImplementation(CodeTypeDeclaration interfaceTypeDec)
		{
			var className = (interfaceTypeDec.Name.StartsWith("I", StringComparison.Ordinal) ? interfaceTypeDec.Name.Substring(1) : interfaceTypeDec.Name) + "Client";
			var classTypeDec = new CodeTypeDeclaration(className);
			classTypeDec.IsClass = true;
			classTypeDec.IsPartial = true;
			classTypeDec.TypeAttributes |= TypeAttributes.Public;

			classTypeDec.BaseTypes.Add(ClientBaseClassName);
			classTypeDec.BaseTypes.Add(new CodeTypeReference(interfaceTypeDec.Name));
			_codeNamespace.Types.Add(classTypeDec);

			return classTypeDec;
		}

		private CodeTypeReference Process(XElement element, bool isRequest)
		{
			CodeTypeReference result = null;
			var inputMessageType = GetTypeName(element, "message");

			var messageElement = _messageElements[inputMessageType];
			var parts = messageElement.Elements(Namespaces.Wsdl + "part");
			foreach (var partElement in parts)
			{
				var elementType = GetTypeName(partElement, "element");
				result = GetCodeTypeReference(elementType, true, isRequest);
			}

			return result;
		}

		private CodeTypeReference GetCodeTypeReference(XName typeName, bool isRoot, bool isRequest)
		{
			if (typeName == null)
				throw new ArgumentNullException("typeName");

			var type = SimpleTypes.Get(typeName);
			if (type != null)
				return new CodeTypeReference(type);

			CodeTypeReference codeTypeReference;
			if (_generatedTypes.TryGetValue(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaElement(typeName, isRoot, isRequest, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaComplexType(typeName, isRoot, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaSimpleType(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaAttribute(typeName, out codeTypeReference))
				return codeTypeReference;

			var exceptionMessage = String.Format("Type '{0}' is not supported", typeName);
			throw new NotSupportedException(exceptionMessage);
		}

		private bool TryGetFromSchemaElement(XName typeName, bool isRoot, bool isRequest, out CodeTypeReference typeReference)
		{
			XElement typeElement;
			if (!_schemaElements.TryGetValue(typeName, out typeElement))
			{
				typeReference = null;
				return false;
			}

			var complexTypeElement = typeElement.Element(Namespaces.Xsd + "complexType");
			string localName;
			if (complexTypeElement != null)
			{
				localName = GetCodeTypeName(typeName.LocalName);
				typeReference = new CodeTypeReference(new CodeTypeParameter(localName));
				_generatedTypes.Add(typeName, typeReference);
				CreateComplexTypeDeclaration(complexTypeElement, localName, typeName.NamespaceName, isRoot, isRequest);
				return true;
			}

			var tn = GetTypeName(typeElement, "type");
			var x = _schemaComplexTypes[tn];

			localName = GetCodeTypeName(tn.LocalName);
			typeReference = new CodeTypeReference(new CodeTypeParameter(localName));
			_generatedTypes.Add(typeName, typeReference);

			CreateComplexTypeDeclaration(x, localName, tn.NamespaceName, isRoot, isRequest);

			return true;
		}

		private bool TryGetFromSchemaComplexType(XName typeName, bool isRoot, out CodeTypeReference typeReference)
		{
			XElement typeElement;
			if (!_schemaComplexTypes.TryGetValue(typeName, out typeElement))
			{
				typeReference = null;
				return false;
			}

			var localName = GetCodeTypeName(typeName.LocalName);
			typeReference = new CodeTypeReference(new CodeTypeParameter(localName));
			_generatedTypes.Add(typeName, typeReference);

			CreateComplexTypeDeclaration(typeElement, localName, typeName.NamespaceName, isRoot, false);

			return true;
		}

		private bool TryGetFromSchemaSimpleType(XName typeName, out CodeTypeReference typeReference)
		{
			XElement typeElement;
			if (!_schemaSimpleTypes.TryGetValue(typeName, out typeElement))
			{
				typeReference = null;
				return false;
			}

			var unionElement = typeElement.Element(Namespaces.Xsd + "union");
			if (unionElement != null)
			{
				typeReference = GetCodeTypeReference(XName.Get("string", Namespaces.Xsd.NamespaceName), false, false);
				return true;
			}

			var listElement = typeElement.Element(Namespaces.Xsd + "list");
			if (listElement != null)
			{
				var itemType = GetTypeName(listElement, "itemType");
				var itemTypeCodeRef = GetCodeTypeReference(itemType, false, false);
				typeReference = new CodeTypeReference(String.Format("{0}[]", itemTypeCodeRef.BaseType));
				_generatedTypes.Add(typeName, typeReference);

				return true;
			}

			var restrictionElement = typeElement.Element(Namespaces.Xsd + "restriction");
			if (restrictionElement == null)
			{
				typeReference = null;
				return false;
			}

			var enumerationElements = restrictionElement.Elements(Namespaces.Xsd + "enumeration").ToArray();
			if (enumerationElements.Any())
			{
				var targetEnum = new CodeTypeDeclaration(typeName.LocalName);
				targetEnum.IsEnum = true;
				targetEnum.TypeAttributes = TypeAttributes.Public;

				targetEnum.CustomAttributes.Add(new CodeAttributeDeclaration(
					new CodeTypeReference(typeof(XmlTypeAttribute)),
					new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(typeName.Namespace.NamespaceName))));
				_codeNamespace.Types.Add(targetEnum);

				foreach (var xElement in restrictionElement.Elements(Namespaces.Xsd + "enumeration"))
				{
					var name = FixFieldName(xElement.Attribute("value").Value);

					var f = new CodeMemberField(name, name);

					// TODO enum values
					//var xmlEnumAttr = memberInfo.GetCustomAttribute<XmlEnumAttribute>();
					//if (xmlEnumAttr != null)
					//{
					//	var attrType = new CodeTypeReference(typeof(XmlEnumAttribute));
					//	f.CustomAttributes.Add(new CodeAttributeDeclaration(attrType,
					//		new CodeAttributeArgument(new CodePrimitiveExpression(xmlEnumAttr.Name))));
					//}

					targetEnum.Members.Add(f);
				}

				typeReference = new CodeTypeReference(new CodeTypeParameter(targetEnum.Name));
				_generatedTypes.Add(typeName, typeReference);

				return true;
			}

			var baseType = GetTypeName(restrictionElement, "base");
			if (baseType != null)
			{
				typeReference = GetCodeTypeReference(baseType, false, false);
				return true;
			}

			typeReference = null;
			return false;
		}

		private bool TryGetFromSchemaAttribute(XName typeName, out CodeTypeReference typeReference)
		{
			XElement typeElement;
			if (!_schemaAttributes.TryGetValue(typeName, out typeElement))
			{
				typeReference = null;
				return false;
			}

			var restrictionElement = typeElement.Element(Namespaces.Xsd + "simpleType").Element(Namespaces.Xsd + "restriction");
			var baseType = GetTypeName(restrictionElement, "base");
			typeReference = GetCodeTypeReference(baseType, false, false);
			return true;
		}

		private readonly List<string> _usedNames = new List<string>();
		private string GetCodeTypeName(string localName)
		{
			// TODO class name == field name
			string newName;
			if (_usedNames.Contains(localName))
			{
				newName = localName + "1";
			}
			else
			{
				newName = localName;
			}

			_usedNames.Add(newName);

			return newName;
		}

		private CodeTypeDeclaration CreateComplexTypeDeclaration(XElement complexTypeElement, string typeName, string typeNamespace, bool isRoot, bool isRequest)
		{
			var targetClass = new CodeTypeDeclaration(isRequest ? (typeName + "Request") : typeName)
			{
				IsClass = true,
				TypeAttributes = TypeAttributes.Public
			};
			_codeNamespace.Types.Add(targetClass);

			var attrArgs = new List<CodeAttributeArgument>();
			//attrArgs.Add(new CodeAttributeArgument("TypeName", new CodePrimitiveExpression(typeName)));
			if (!String.IsNullOrEmpty(typeNamespace))
				attrArgs.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(typeNamespace)));

			var attrType = new CodeTypeReference(typeof(XmlTypeAttribute));
			targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, attrArgs.ToArray()));


			if (isRoot)
			{
				attrArgs = new List<CodeAttributeArgument>();
				if (!String.IsNullOrEmpty(typeNamespace))
					attrArgs.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(typeNamespace)));

				attrType = new CodeTypeReference(typeof(XmlRootAttribute));
				targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, attrArgs.ToArray()));
			}

			AddComment(complexTypeElement, targetClass);

			var complexContentEl = complexTypeElement.Element(Namespaces.Xsd + "complexContent");
			if (complexContentEl != null)
			{
				var extensionEl = complexContentEl.Element(Namespaces.Xsd + "extension");
				var baseTypeName = GetTypeName(extensionEl, "base");

				var baseType = GetCodeTypeReference(baseTypeName, false, false);

				// TODO add     [System.Xml.Serialization.XmlIncludeAttribute(typeof(VideoEncoderConfiguration))]
				//((CodeTypeMember) baseType).CustomAttributes.Add(null);

				targetClass.BaseTypes.Add(baseType);

				AddSequence(extensionEl, targetClass);
			}

			foreach (var attributeElement in complexTypeElement.Elements(Namespaces.Xsd + "attribute"))
			{
				string name;
				CodeTypeReference codeTypeReference;
				var args = new List<CodeAttributeArgument>();

				var refTypeName = GetTypeName(attributeElement, "ref");
				if (refTypeName != null)
				{
					name = refTypeName.LocalName;
					codeTypeReference = GetCodeTypeReference(refTypeName, false, false);

					//[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/2005/05/xmlmime")]
					args.Add(new CodeAttributeArgument("Form", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(XmlSchemaForm)), "Qualified")));
					args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(refTypeName.Namespace.NamespaceName)));
				}
				else
				{
					name = FixFieldName(attributeElement.Attribute("name").Value);
					var typeName2 = GetTypeName(attributeElement, "type");
					codeTypeReference = GetCodeTypeReference(typeName2, false, false);
				}

				var fieldCode = new CodeMemberField();
				fieldCode.Attributes = MemberAttributes.Public;
				fieldCode.Name = name;
				fieldCode.Type = codeTypeReference;


				//args.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(name)));
				//args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns.NamespaceName)));

				attrType = new CodeTypeReference(typeof(XmlAttributeAttribute));
				fieldCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));

				targetClass.Members.Add(fieldCode);

				AddComment(attributeElement, fieldCode);
			}

			AddSequence(complexTypeElement, targetClass);

			return targetClass;
		}

		private void AddSequence(XElement complexTypeElement, CodeTypeDeclaration targetClass)
		{
			var sequenceElement = complexTypeElement.Element(Namespaces.Xsd + "sequence");
			if (sequenceElement == null)
				return;

			var xElements = sequenceElement.Elements(Namespaces.Xsd + "element").ToArray();
			for (int index = 0; index < xElements.Length; index++)
			{
				var xElement = xElements[index];
				string fieldName;
				string xmlElementName;
				XName typeName2;

				var refTypeName = GetTypeName(xElement, "ref");
				if (refTypeName != null)
				{
					xmlElementName = refTypeName.LocalName;
					fieldName = refTypeName.LocalName;
					typeName2 = refTypeName;
				}
				else
				{
					xmlElementName = xElement.Attribute("name").Value;
					fieldName = FixFieldName(xElement.Attribute("name").Value);
					typeName2 = GetTypeName(xElement, "type");
				}

				if (typeName2 == null)
				{
					// TODO
					continue;
				}

				var minOccursAttr = xElement.Attribute("minOccurs");
				var maxOccursAttr = xElement.Attribute("maxOccurs");
				bool isCollection = minOccursAttr != null
									&& minOccursAttr.Value == "0"
									&& maxOccursAttr != null
									&& maxOccursAttr.Value == "unbounded";

				CodeTypeReference codeTypeReference = GetCodeTypeReference(typeName2, false, false);

				var fieldCode = new CodeMemberField();
				fieldCode.Attributes = MemberAttributes.Public;
				fieldCode.Name = fieldName;
				fieldCode.Type = isCollection
					? new CodeTypeReference(String.Format("{0}[]", codeTypeReference.BaseType))
					: codeTypeReference;

				var args = new List<CodeAttributeArgument>();

				if (fieldName != xmlElementName)
					args.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(xmlElementName)));

				args.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(index)));
				//args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns.NamespaceName)));

				var attrType = new CodeTypeReference(typeof(XmlElementAttribute));
				fieldCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));

				targetClass.Members.Add(fieldCode);

				AddComment(xElement, fieldCode);
			}
		}

		private static void AddComment(XElement xElement, CodeTypeMember memberElement)
		{
			var comment = GetComment(xElement);
			if (comment != null)
			{
				memberElement.Comments.Add(new CodeCommentStatement("<summary>", true));
				memberElement.Comments.Add(new CodeCommentStatement(comment, true));
				memberElement.Comments.Add(new CodeCommentStatement("</summary>", true));
			}
		}

		private string FixFieldName(string name)
		{
			var builder = new StringBuilder(name);

			builder.Replace(".", String.Empty);
			builder.Replace(" ", String.Empty);
			builder.Replace("-", String.Empty);

			return builder.ToString();
		}

		private static string GetComment(XElement xElement)
		{
			var annotationEl = xElement.Element(Namespaces.Xsd + "annotation");
			if (annotationEl == null)
				return null;

			var documentationElement = annotationEl.Element(Namespaces.Xsd + "documentation");
			if (documentationElement == null)
				return null;

			return Regex.Replace(documentationElement.Value, "\n", "\r\n");

			return documentationElement.Value;
		}

		private static XElement GetPortTypeOperationElement(XElement portTypeElement, string operationName)
		{
			var q = from opEl in portTypeElement.Elements(Namespaces.Wsdl + "operation")
					let nameAttr = opEl.Attribute("name")
					where nameAttr != null && nameAttr.Value == operationName
					select opEl;

			return q.Single();
		}

		private static XName GetTypeName(XElement element, string attributeName, string targetNamespace)
		{
			var xAttribute = element.Attribute(attributeName);
			if (xAttribute == null)
				throw new Exception();

			var index = xAttribute.Value.IndexOf(':');
			if (index == -1)
			{
				return XName.Get(xAttribute.Value, targetNamespace);
			}

			throw new NotSupportedException();
		}

		private static XName GetTypeName(XElement element, string attributeName)
		{
			var xAttribute = element.Attribute(attributeName);
			if (xAttribute == null)
				return null;

			var index = xAttribute.Value.IndexOf(':');
			if (index == -1)
			{
				throw new NotImplementedException();
			}

			var nsName = xAttribute.Value.Substring(0, index);
			var localName = xAttribute.Value.Substring(index + 1);

			return XName.Get(localName, element.GetNamespaceOfPrefix(nsName).NamespaceName);
		}

		public string GetSourceCode()
		{
			var provider = CodeDomProvider.CreateProvider("CSharp");
			var options = new CodeGeneratorOptions();
			options.BracingStyle = "C";
			options.BlankLinesBetweenMembers = true;

			var stringBuilder = new StringBuilder();

			using (var sourceWriter = new StringWriter(stringBuilder))
			{
				provider.GenerateCodeFromCompileUnit(_codeUnit, sourceWriter, options);
			}

			return stringBuilder.ToString();
		}
	}

	public class BuilderParameters
	{
		public string CodeNamespace { get; set; }
	}
}
