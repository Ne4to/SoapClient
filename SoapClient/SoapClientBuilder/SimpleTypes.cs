using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoapClientBuilder
{
	public static class SimpleTypes
	{
		private static readonly Dictionary<XName, Type> _types = new Dictionary<XName, Type>();

		static SimpleTypes()
		{
			// http://www.w3schools.com/schema/schema_dtypes_numeric.asp
			_types.Add(XName.Get("byte", Namespaces.Xsd.NamespaceName), typeof(SByte));
			_types.Add(XName.Get("unsignedByte", Namespaces.Xsd.NamespaceName), typeof(Byte));

			_types.Add(XName.Get("short", Namespaces.Xsd.NamespaceName), typeof(Int16));
			_types.Add(XName.Get("unsignedShort", Namespaces.Xsd.NamespaceName), typeof(UInt16));

			_types.Add(XName.Get("int", Namespaces.Xsd.NamespaceName), typeof(Int32));
			_types.Add(XName.Get("integer", Namespaces.Xsd.NamespaceName), typeof(Int32));
			_types.Add(XName.Get("unsignedInt", Namespaces.Xsd.NamespaceName), typeof(UInt32));

			_types.Add(XName.Get("long", Namespaces.Xsd.NamespaceName), typeof(Int64));
			_types.Add(XName.Get("unsignedLong", Namespaces.Xsd.NamespaceName), typeof(UInt64));

			_types.Add(XName.Get("float", Namespaces.Xsd.NamespaceName), typeof(Single));
			_types.Add(XName.Get("double", Namespaces.Xsd.NamespaceName), typeof(Double));
			_types.Add(XName.Get("decimal", Namespaces.Xsd.NamespaceName), typeof(Decimal));
		
			_types.Add(XName.Get("string", Namespaces.Xsd.NamespaceName), typeof(String));
			_types.Add(XName.Get("boolean", Namespaces.Xsd.NamespaceName), typeof(Boolean));
			_types.Add(XName.Get("dateTime", Namespaces.Xsd.NamespaceName), typeof(DateTime));
			_types.Add(XName.Get("duration", Namespaces.Xsd.NamespaceName), typeof(string));
			_types.Add(XName.Get("base64Binary", Namespaces.Xsd.NamespaceName), typeof(byte[]));
			_types.Add(XName.Get("hexBinary", Namespaces.Xsd.NamespaceName), typeof(byte[]));
			_types.Add(XName.Get("anyURI", Namespaces.Xsd.NamespaceName), typeof(string));
			_types.Add(XName.Get("token", Namespaces.Xsd.NamespaceName), typeof(string));
			_types.Add(XName.Get("QName", Namespaces.Xsd.NamespaceName), typeof(System.Xml.XmlQualifiedName));

			//_types.Add(XName.Get("contentType", Namespaces.Mime.NamespaceName), typeof(string));
			//_types.Add(XName.Get("mustUnderstand", @"http://schemas.xmlsoap.org/soap/envelope/"), typeof(Boolean));
			//_types.Add(XName.Get("anyURI", Namespaces.Xsd.NamespaceName), typeof(XElement)); // TODO
		}

		public static Type Get(XName name)
		{
			if (name == null)
				return null;

			Type result;
			if (_types.TryGetValue(name, out result))
				return result;

			return null;
		}
	}
}