using System.Xml.Serialization;

namespace SoapServices
{
	[XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", ElementName = "Text")]
	public class SoapText
	{
		[XmlAttribute("xml:lang")]
		public string Language { get; set; }

		[XmlText]
		public string Message { get; set; }
	}
}