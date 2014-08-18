using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SoapServices
{
	public class ResponseMessage
	{
		private static readonly XNamespace NsEnvelope = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");

		public XDocument Document { get; private set; }
		public SoapFault Fault { get; private set; }

		protected XElement BodyContentNode { get; private set; }

		public ResponseMessage(XDocument document)
		{
			Document = document;
			Read();
		}

		private void Read()
		{
			var envelopeElement = Document.Element(XName.Get("Envelope", NsEnvelope.NamespaceName));
			var bodyElement = envelopeElement.Element(XName.Get("Body", NsEnvelope.NamespaceName));

			BodyContentNode = bodyElement.FirstNode as XElement;

			if (BodyContentNode != null && BodyContentNode.Name == XName.Get("Fault", NsEnvelope.NamespaceName))
			{
				Fault = Deserialize<SoapFault>(BodyContentNode, envelopeElement);
			}
		}

		public T GetContent<T>()
		{
			if (BodyContentNode == null)
				throw new Exception("No content");

			if (Fault != null)
				throw new SoapFaultException(Fault);

			return Deserialize<T>(BodyContentNode, null);
		}

		private T Deserialize<T>(XElement node, XElement envelopeElement)
		{
			var parseNode = node;

			if (envelopeElement != null)
			{
				parseNode = new XElement(node);

				foreach (var xAttribute in envelopeElement.Attributes())
				{
					if (xAttribute.IsNamespaceDeclaration)
					{
						parseNode.SetAttributeValue(XNamespace.Xmlns + xAttribute.Name.LocalName, xAttribute.Value);
					}
				}
			}

			var str = parseNode.ToString();

			using (var stringReader = new StringReader(str))
			{
				var xmlReaderSettings = new XmlReaderSettings();
				
				using (var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
				{
					var xmlSerializer = new XmlSerializer(typeof (T));
					return (T) xmlSerializer.Deserialize(xmlReader);
				}
			}
		}
	}

	public class SoapFaultException : Exception
	{
		public SoapFault Fault { get; private set; }

		public SoapFaultException(SoapFault fault)
		{
			Fault = fault;
		}
	}
}