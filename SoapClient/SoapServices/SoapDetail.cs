using System.Xml.Serialization;

namespace SoapServices
{
	[XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", ElementName = "Detail")]
	public class SoapDetail
	{
		[XmlElement]
		public SoapText Text { get; set; }
	}
}