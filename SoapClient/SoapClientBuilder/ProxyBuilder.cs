using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoapClientBuilder
{
	// TODO XmlElementAttribute.DataType
	// TODO XmlElementAttribute.IsNullable
	// TODO XmlElementAttribute.Order
	// TODO XML Documentation
	public class ProxyBuilder
	{
		private const string ClientBaseClassName = "SoapServices.SoapClientBase";

		private readonly XDocument _wsdlDoc;

		private readonly CodeCompileUnit _codeUnit;
		private readonly CodeNamespace _codeNamespace;

		private Dictionary<XName, XElement> _messageElements;
		private readonly Dictionary<XName, CodeTypeReference> _generatedTypes = new Dictionary<XName, CodeTypeReference>();
		private readonly Dictionary<XName, XElement> _schemaElements;
		private readonly Dictionary<XName, XElement> _schemaComplexTypes;
		private readonly Dictionary<XName, XElement> _schemaSimpleTypes;
		private readonly Dictionary<XName, XElement> _schemaAttributes;

		public ProxyBuilder(XDocument wsdlDoc)
		{
			if (wsdlDoc == null) throw new ArgumentNullException("wsdlDoc");
			_wsdlDoc = wsdlDoc;

			_codeUnit = new CodeCompileUnit();
			_codeNamespace = new CodeNamespace("Aaaa"); // TODO
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
					var inputTypeReference = Process(inputElement);

					var outputElement = operation.Element(Namespaces.Wsdl + "output");
					var outputTypeReference = Process(outputElement);

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
			mth.Name = operationName;
			var typeName = String.Format("System.Threading.Tasks.Task<{0}>", outputTypeReference.BaseType);
			mth.ReturnType = new CodeTypeReference() { BaseType = typeName };

			mth.Parameters.Add(new CodeParameterDeclarationExpression(inputTypeReference, "request"));

			interfaceTypeDec.Members.Add(mth);
		}

		private static void AddImplementationOperation(string operationName, CodeTypeReference outputTypeReference, CodeTypeDeclaration implTypeDec, CodeTypeReference inputTypeReference, string soapAction)
		{
			var mth = new CodeMemberMethod();
			mth.Attributes = MemberAttributes.Public;
			mth.Name = operationName;
			var typeName = String.Format("System.Threading.Tasks.Task<{0}>", outputTypeReference.BaseType);
			mth.ReturnType = new CodeTypeReference() { BaseType = typeName };

			mth.Parameters.Add(new CodeParameterDeclarationExpression(inputTypeReference, "request"));

			var returnStatement = new CodeMethodReturnStatement();

			var invokeExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
				"CallAsync",
				new CodePrimitiveExpression(soapAction),
				new CodeVariableReferenceExpression("request"));
			invokeExpression.Method.TypeArguments.Add(inputTypeReference.BaseType);
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

		private CodeTypeReference Process(XElement element)
		{
			CodeTypeReference result = null;
			var inputMessageType = GetTypeName(element, "message");

			var messageElement = _messageElements[inputMessageType];
			var parts = messageElement.Elements(Namespaces.Wsdl + "part");
			foreach (var partElement in parts)
			{
				var elementType = GetTypeName(partElement, "element");
				result = GetCodeTypeReference(elementType);
			}

			return result;
		}

		private CodeTypeReference GetCodeTypeReference(XName typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException("typeName");

			var type = SimpleTypes.Get(typeName);
			if (type != null)
				return new CodeTypeReference(type);

			CodeTypeReference codeTypeReference;
			if (_generatedTypes.TryGetValue(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaElement(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaComplexType(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaSimpleType(typeName, out codeTypeReference))
				return codeTypeReference;

			if (TryGetFromSchemaAttribute(typeName, out codeTypeReference))
				return codeTypeReference;

			var exceptionMessage = String.Format("Type '{0}' is not supported", typeName);
			throw new NotSupportedException(exceptionMessage);
		}

		private bool TryGetFromSchemaElement(XName typeName, out CodeTypeReference typeReference)
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
				CreateComplexTypeDeclaration(complexTypeElement, localName, typeName.NamespaceName);
				return true;
			}

			var tn = GetTypeName(typeElement, "type");
			var x = _schemaComplexTypes[tn];

			localName = GetCodeTypeName(tn.LocalName);
			typeReference = new CodeTypeReference(new CodeTypeParameter(localName));
			_generatedTypes.Add(typeName, typeReference);

			CreateComplexTypeDeclaration(x, localName, tn.NamespaceName);

			return true;
		}

		private bool TryGetFromSchemaComplexType(XName typeName, out CodeTypeReference typeReference)
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

			CreateComplexTypeDeclaration(typeElement, localName, typeName.NamespaceName);

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
				typeReference = GetCodeTypeReference(XName.Get("string", Namespaces.Xsd.NamespaceName));
				return true;
			}

			var listElement = typeElement.Element(Namespaces.Xsd + "list");
			if (listElement != null)
			{
				var itemType = GetTypeName(listElement, "itemType");
				var itemTypeCodeRef = GetCodeTypeReference(itemType);
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

			var baseType = GetTypeName(restrictionElement, "base");
			if (baseType != null)
			{
				typeReference = GetCodeTypeReference(baseType);
				return true;
			}

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
			typeReference = GetCodeTypeReference(baseType);
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

		private CodeTypeDeclaration CreateComplexTypeDeclaration(XElement complexTypeElement, string typeName, string typeNamespace)
		{
			var targetClass = new CodeTypeDeclaration(typeName)
			{
				IsClass = true,
				TypeAttributes = TypeAttributes.Public
			};
			_codeNamespace.Types.Add(targetClass);

			var attrType = new CodeTypeReference(typeof(XmlRootAttribute));
			targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(attrType,
				new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(typeName)),
				new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(typeNamespace))));

			foreach (var attributeElement in complexTypeElement.Elements(Namespaces.Xsd + "attribute"))
			{
				string name;
				CodeTypeReference codeTypeReference;
				var args = new List<CodeAttributeArgument>();

				var refTypeName = GetTypeName(attributeElement, "ref");
				if (refTypeName != null)
				{
					name = refTypeName.LocalName;
					codeTypeReference = GetCodeTypeReference(refTypeName);

					//[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/2005/05/xmlmime")]
					args.Add(new CodeAttributeArgument("Form", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(XmlSchemaForm)), "Qualified")));
					args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(refTypeName.Namespace.NamespaceName)));
				}
				else
				{
					name = FixFieldName(attributeElement.Attribute("name").Value);
					var typeName2 = GetTypeName(attributeElement, "type");
					codeTypeReference = GetCodeTypeReference(typeName2);
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
			}

			var sequenceElement = complexTypeElement.Element(Namespaces.Xsd + "sequence");
			if (sequenceElement != null)
			{
				foreach (var xElement in sequenceElement.Elements(Namespaces.Xsd + "element"))
				{
					string name;
					XName typeName2;

					var refTypeName = GetTypeName(xElement, "ref");
					if (refTypeName != null)
					{
						name = refTypeName.LocalName;
						typeName2 = refTypeName;
					}
					else
					{
						name = FixFieldName(xElement.Attribute("name").Value);
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
					
					CodeTypeReference codeTypeReference = GetCodeTypeReference(typeName2);

					var fieldCode = new CodeMemberField();
					fieldCode.Attributes = MemberAttributes.Public;
					fieldCode.Name = name;
					fieldCode.Type = isCollection ? new CodeTypeReference(String.Format("{0}[]", codeTypeReference.BaseType)) : codeTypeReference;

					var args = new List<CodeAttributeArgument>();
					args.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(name)));
					//args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns.NamespaceName)));

					attrType = new CodeTypeReference(typeof(XmlElementAttribute));
					fieldCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));

					targetClass.Members.Add(fieldCode);
				}
			}

			return targetClass;
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
			var documentationElement = xElement.Element(Namespaces.Wsdl + "documentation");
			if (documentationElement == null)
				return null;

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

		private static string GetSoapAction(XElement bindingOperationElement)
		{
			var operationElement = bindingOperationElement.Element(Namespaces.Soap + "operation");
			if (operationElement == null)
				return null;

			var soapActionAttribute = operationElement.Attribute("soapAction");
			if (soapActionAttribute == null)
				return null;

			return soapActionAttribute.Value;
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
}