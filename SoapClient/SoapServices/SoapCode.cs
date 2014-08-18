using System.Xml;
using System.Xml.Serialization;

namespace SoapServices
{
	[XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", ElementName = "Code")]
	public class SoapCode
	{
		[XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public XmlQualifiedName Value { get; set; }

		[XmlElement]
		public SoapCode Subcode { get; set; }
	}
}