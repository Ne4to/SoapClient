using System.Xml.Serialization;

namespace SoapServices
{
	[XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", ElementName = "Reason")]
	public class SoapReason
	{
		[XmlElement]
		public SoapText Text { get; set; }
	}
}