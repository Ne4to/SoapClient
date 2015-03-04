using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoapClientBuilder
{
	public static class Namespaces
	{
		public static readonly XNamespace Xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
		public static readonly XNamespace Wsdl = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/");
		public static readonly XNamespace Soap = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/soap/");
		public static readonly XNamespace Soap12 = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/soap12/");
		public static readonly XNamespace Mime = XNamespace.Get("http://www.w3.org/2005/05/xmlmime");
		public static readonly XNamespace Xop = XNamespace.Get("http://www.w3.org/2004/08/xop/include");
		

		//public static readonly XNamespace Addressing = XNamespace.Get("http://www.w3.org/2006/05/addressing/wsdl");
	}

	public static class SimpleTypes
	{
		private static readonly Dictionary<XName, Type> _types = new Dictionary<XName, Type>();

		static SimpleTypes()
		{
			_types.Add(XName.Get("int", Namespaces.Xsd.NamespaceName), typeof(Int32));
			_types.Add(XName.Get("integer", Namespaces.Xsd.NamespaceName), typeof(Int32));
			_types.Add(XName.Get("string", Namespaces.Xsd.NamespaceName), typeof(String));
			_types.Add(XName.Get("boolean", Namespaces.Xsd.NamespaceName), typeof(Boolean));
			_types.Add(XName.Get("dateTime", Namespaces.Xsd.NamespaceName), typeof(DateTime));
			_types.Add(XName.Get("duration", Namespaces.Xsd.NamespaceName), typeof(string));
			_types.Add(XName.Get("base64Binary", Namespaces.Xsd.NamespaceName), typeof(byte[]));
			_types.Add(XName.Get("hexBinary", Namespaces.Xsd.NamespaceName), typeof(byte[]));
			_types.Add(XName.Get("anyURI", Namespaces.Xsd.NamespaceName), typeof(string));
			_types.Add(XName.Get("token", Namespaces.Xsd.NamespaceName), typeof(string));

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