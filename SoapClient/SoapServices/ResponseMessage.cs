using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SoapServices
{
	public class ResponseMessage
	{
		public XDocument Document { get; private set; }
		public XNamespace EnvelopeNamespace { get; set; }
		public SoapFault Fault { get; private set; }

		protected XElement BodyContentNode { get; private set; }

		public ResponseMessage(XDocument document)
			: this(document, SoapClientBase.Soap12EnvelopeNamespace)
		{
		}

		public ResponseMessage(XDocument document, XNamespace envelopeNamespace)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (envelopeNamespace == null) throw new ArgumentNullException("envelopeNamespace");

			Document = document;
			EnvelopeNamespace = envelopeNamespace;
			Read();
		}

		private void Read()
		{
			var envelopeElement = Document.Element(XName.Get("Envelope", EnvelopeNamespace.NamespaceName));
			var bodyElement = envelopeElement.Element(XName.Get("Body", EnvelopeNamespace.NamespaceName));

			BodyContentNode = bodyElement.FirstNode as XElement;

			if (BodyContentNode != null && BodyContentNode.Name == XName.Get("Fault", EnvelopeNamespace.NamespaceName))
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