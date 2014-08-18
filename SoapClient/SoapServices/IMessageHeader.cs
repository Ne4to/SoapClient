using System.Xml.Linq;

namespace SoapServices
{
	public interface IMessageHeader
	{
		XElement GetXml();
	}
}