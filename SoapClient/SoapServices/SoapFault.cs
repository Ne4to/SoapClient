using System.Xml.Serialization;

namespace SoapServices
{
	[XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", ElementName = "Fault")]
	public class SoapFault
	{
		[XmlElement]
		public SoapCode Code { get; set; }

		[XmlElement]
		public SoapReason Reason { get; set; }

		[XmlElement]
		public SoapDetail Detail { get; set; }
	}
}