using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoapClientGenerator
{
	internal class ContractInfo
	{
		public TypeInfo Info { get; set; }

		//static readonly Type[] skipAttrs =
		//{
		//	typeof(GeneratedCodeAttribute),
		//	typeof(SerializableAttribute), // WinRT does not support			
		//	typeof(DesignerCategoryAttribute)
		//};

		public ContractInfo(TypeInfo info)
		{
			Info = info;
		}

		public CodeTypeDeclaration GetCodeDeclaration()
		{
			var targetClass = new CodeTypeDeclaration(Info.Name);
			targetClass.IsClass = true;
			targetClass.TypeAttributes = TypeAttributes.Public;

			if (Info.BaseType != typeof(object))
			{
				targetClass.BaseTypes.Add(Info.BaseType);
			}

			foreach (var customAttribute in Info.GetCustomAttributes(typeof(XmlIncludeAttribute), false).Cast<XmlIncludeAttribute>())
			{
				var attrType = new CodeTypeReference(typeof(XmlIncludeAttribute));
				targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(attrType,
					new CodeAttributeArgument(new CodeTypeOfExpression(customAttribute.Type))));
			}

			var xmlTypeAttr = Info.GetCustomAttribute<XmlTypeAttribute>();
			var messageContractAttr = Info.GetCustomAttribute<MessageContractAttribute>();

			bool addXmlRoot = false;
			string xmlRootElementName = null;
			string xmlRootNamespace = null;

			if (xmlTypeAttr != null)
			{
				addXmlRoot = true;
				xmlRootNamespace = xmlTypeAttr.Namespace;
			}
			else
			{
				if (messageContractAttr != null)
				{
					addXmlRoot = true;
					xmlRootElementName = messageContractAttr.WrapperName;
					xmlRootNamespace = messageContractAttr.WrapperNamespace;
				}
			}

			var bodyMember = Info.DeclaredFields.FirstOrDefault(f => f.IsPublic && f.GetCustomAttribute<MessageBodyMemberAttribute>() != null);
			if (bodyMember != null)
			{
				xmlRootElementName = bodyMember.Name;
				var bodyAttr = bodyMember.GetCustomAttribute<MessageBodyMemberAttribute>();
				xmlRootNamespace = bodyAttr.Namespace;
			}
			
			if (addXmlRoot)
			{
				var attrType = new CodeTypeReference(typeof(XmlRootAttribute));
				targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(attrType,
					new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(xmlRootElementName)),
					new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(xmlRootNamespace))));
			}

			AddFields(targetClass);
			AddProperties(targetClass);

			return targetClass;
		}

		private void AddProperties(CodeTypeDeclaration targetClass)
		{
			foreach (var prop in Info.DeclaredProperties.Where(p => p.GetCustomAttribute<XmlAnyAttributeAttribute>() == null && p.GetCustomAttribute<XmlIgnoreAttribute>() == null))
			{
				// TODO check
				if (prop.PropertyType == typeof(System.Xml.XmlNode[]))
					continue;

				var propCode = new CodeMemberField
				{
					Attributes = MemberAttributes.Public | MemberAttributes.Final,
					Name = prop.Name == "fixed" ? "@fixed" : prop.Name,
					Type = new CodeTypeReference(GetPropertyType(prop)),
				};

				propCode.Name += " { get; set; }";

				var xmlElementAttrs = prop.GetCustomAttributes<XmlElementAttribute>();
				foreach (var xmlElementAttr in xmlElementAttrs)
				{
					var args = new List<CodeAttributeArgument>();

					if (!String.IsNullOrEmpty(xmlElementAttr.ElementName))
					{
						args.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(xmlElementAttr.ElementName)));
					}

					string ns = GetNamespace(xmlElementAttr, null);
					if (ns != null)
					{
						args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
					}

					string dataType = GetDataType(xmlElementAttr);
					if (dataType != null)
					{
						args.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(dataType)));
					}

					int order = GetOrder(xmlElementAttr, null);
					args.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(order)));

					var attrType = new CodeTypeReference(typeof(XmlElementAttribute));
					propCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));
				}

				var xmlAnyElementAttr = prop.GetCustomAttribute<XmlAnyElementAttribute>();
				if (xmlAnyElementAttr != null)
				{
					var args = new List<CodeAttributeArgument>();

					if (xmlAnyElementAttr.Namespace != null)
					{
						args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(xmlAnyElementAttr.Namespace)));
					}

					args.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(xmlAnyElementAttr.Order)));

					var attrType = new CodeTypeReference(typeof(XmlAnyElementAttribute));
					propCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));
				}

				var xmlAttributeAttr = prop.GetCustomAttribute<XmlAttributeAttribute>();
				if (xmlAttributeAttr != null)
				{
					var args = new List<CodeAttributeArgument>();

					if (xmlAttributeAttr.Namespace != null)
					{
						args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(xmlAttributeAttr.Namespace)));
					}

					if (!String.IsNullOrEmpty(xmlAttributeAttr.DataType))
					{
						args.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(xmlAttributeAttr.DataType)));
					}

					if (xmlAttributeAttr.Form != XmlSchemaForm.None)
					{
						args.Add(new CodeAttributeArgument("Form", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(XmlSchemaForm)), xmlAttributeAttr.Form.ToString())));
					}

					var attrType = new CodeTypeReference(typeof(XmlAttributeAttribute));
					propCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));
				}

				var xmlTextAttr = prop.GetCustomAttribute<XmlTextAttribute>();
				if (xmlTextAttr != null)
				{
					var args = new List<CodeAttributeArgument>();

					var attrType = new CodeTypeReference(typeof(XmlTextAttribute));
					propCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));
				}

				targetClass.Members.Add(propCode);
			}
		}

		private static Type GetPropertyType(PropertyInfo prop)
		{
			if (prop.PropertyType == typeof(XmlElement))
				return typeof(XElement);

			if (prop.PropertyType == typeof(XmlElement[]))
				return typeof(XElement[]);

			return prop.PropertyType;
		}

		private void AddFields(CodeTypeDeclaration targetClass)
		{
			foreach (var field in Info.DeclaredFields.Where(f => f.IsPublic))
			{
				var messageBodyMemberAttr = field.GetCustomAttribute<MessageBodyMemberAttribute>();
				if (messageBodyMemberAttr == null)
					continue;

				var fieldCode = new CodeMemberField();
				fieldCode.Attributes = MemberAttributes.Public;
				fieldCode.Name = field.Name;
				targetClass.Members.Add(fieldCode);

				if (field.GetCustomAttribute<XmlAnyElementAttribute>() == null)
				{
					fieldCode.Type = new CodeTypeReference(field.FieldType);
				}
				else
				{
					fieldCode.Type = new CodeTypeReference(typeof(XElement[]));
				}

				var xmlElementAttr = field.GetCustomAttribute<XmlElementAttribute>();

				string elementName = null;
				bool isNullable = true;
				if (xmlElementAttr != null && !String.IsNullOrEmpty(xmlElementAttr.ElementName))
				{
					elementName = xmlElementAttr.ElementName;					
				}

				var xmlArrayItemAttribute = field.GetCustomAttribute<XmlArrayItemAttribute>();
				if (xmlArrayItemAttribute != null)
				{
					elementName = xmlArrayItemAttribute.ElementName;					
					isNullable = xmlArrayItemAttribute.IsNullable;
				}

				var args = new List<CodeAttributeArgument>();
				args.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(elementName)));
				args.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression(isNullable)));

				string ns = GetNamespace(xmlElementAttr, messageBodyMemberAttr);
				if (ns != null)
				{
					args.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
				}

				string dataType = GetDataType(xmlElementAttr);
				if (dataType != null)
				{
					args.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(dataType)));
				}

				int order = GetOrder(xmlElementAttr, messageBodyMemberAttr);
				args.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(order)));

				var attrType = new CodeTypeReference(typeof(XmlElementAttribute));
				fieldCode.CustomAttributes.Add(new CodeAttributeDeclaration(attrType, args.ToArray()));
			}
		}

		private string GetDataType(XmlElementAttribute xmlElementAttr)
		{
			if (xmlElementAttr != null && !String.IsNullOrEmpty(xmlElementAttr.DataType))
				return xmlElementAttr.DataType;

			return null;
		}

		private int GetOrder(XmlElementAttribute xmlElementAttr, MessageBodyMemberAttribute messageBodyMemberAttr)
		{
			if (xmlElementAttr != null && xmlElementAttr.Order > 0)
				return xmlElementAttr.Order;

			if (messageBodyMemberAttr != null && messageBodyMemberAttr.Order > 0)
				return messageBodyMemberAttr.Order;

			return 0;
		}

		private string GetNamespace(XmlElementAttribute xmlElementAttr, MessageBodyMemberAttribute messageBodyMemberAttr)
		{
			if (xmlElementAttr != null && xmlElementAttr.Namespace != null)
				return xmlElementAttr.Namespace;

			if (messageBodyMemberAttr != null && messageBodyMemberAttr.Namespace != null)
				return messageBodyMemberAttr.Namespace;

			return null;
		}
	}
}