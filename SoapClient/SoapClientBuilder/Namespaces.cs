using System.Xml.Linq;

namespace SoapClientBuilder
{
	public static class Namespaces
	{
		public static readonly XNamespace Xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
		public static readonly XNamespace Wsdl = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/");
		public static readonly XNamespace Soap = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/soap/");
		public static readonly XNamespace Soap12 = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/soap12/");
		
		//public static readonly XNamespace Mime = XNamespace.Get("http://www.w3.org/2005/05/xmlmime");
		//public static readonly XNamespace Xop = XNamespace.Get("http://www.w3.org/2004/08/xop/include");
		//public static readonly XNamespace Addressing = XNamespace.Get("http://www.w3.org/2006/05/addressing/wsdl");
	}
}