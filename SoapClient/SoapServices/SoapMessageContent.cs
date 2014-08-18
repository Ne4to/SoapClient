using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace SoapServices
{
	public class SoapMessageContent : IHttpContent
	{
		private static readonly XNamespace NsEnvelope = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");

		private readonly Lazy<HttpStringContent> _lazyContent;
		private readonly List<IMessageHeader> _soapHeaders;

		HttpStringContent InnerContent { get { return _lazyContent.Value; } }
		
		public List<IMessageHeader> SoapHeaders { get { return _soapHeaders; } }
		public string Action { get; set; }
		public object BodyContent { get; set; }
		
		public SoapMessageContent()
		{
			_lazyContent = new Lazy<HttpStringContent>(CreateInnerContent);
			_soapHeaders = new List<IMessageHeader>();
		}

		public SoapMessageContent(SoapMessageContent original)
			: this()
		{
			if (original == null) throw new ArgumentNullException("original");

			Action = original.Action;
			BodyContent = original.BodyContent;

			foreach (var messageHeader in original.SoapHeaders)
			{
				SoapHeaders.Add(messageHeader);
			}
		}


		private HttpStringContent CreateInnerContent()
		{
			if (Action == null)
				throw new Exception("Action is null");

			if (BodyContent == null)
				throw new Exception("BodyContent is null");

			var envelopeElement = new XElement(XName.Get("Envelope", NsEnvelope.NamespaceName));
			envelopeElement.SetAttributeValue(XNamespace.Xmlns + "s", NsEnvelope);

			var headerElement = new XElement(XName.Get("Header", NsEnvelope.NamespaceName));
			envelopeElement.Add(headerElement);

			foreach (var soapHeader in SoapHeaders)
			{
				headerElement.Add(soapHeader.GetXml());
			}

			var bodyElement = new XElement(XName.Get("Body", NsEnvelope.NamespaceName));
			envelopeElement.Add(bodyElement);



			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
			};

			var stringBuilder = new StringBuilder();
			using (var writer = XmlWriter.Create(stringBuilder, settings))
			{
				var xmlSerializer = new XmlSerializer(BodyContent.GetType());
				xmlSerializer.Serialize(writer, BodyContent);
			}

			var requestElement = XElement.Parse(stringBuilder.ToString());
			bodyElement.Add(requestElement);

			var doc = new XDocument(envelopeElement);

			string envelopeStr = doc.ToString();
			return new HttpStringContent(envelopeStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/soap+xml" + Action);
		}

		#region IHttpContent implementstion

		public void Dispose()
		{
			InnerContent.Dispose();
		}

		public IAsyncOperationWithProgress<ulong, ulong> BufferAllAsync()
		{
			return InnerContent.BufferAllAsync();
		}

		public IAsyncOperationWithProgress<IBuffer, ulong> ReadAsBufferAsync()
		{
			return InnerContent.ReadAsBufferAsync();
		}

		public IAsyncOperationWithProgress<IInputStream, ulong> ReadAsInputStreamAsync()
		{
			return InnerContent.ReadAsInputStreamAsync();
		}

		public IAsyncOperationWithProgress<string, ulong> ReadAsStringAsync()
		{
			return InnerContent.ReadAsStringAsync();
		}

		public bool TryComputeLength(out ulong length)
		{
			return InnerContent.TryComputeLength(out length);
		}

		public IAsyncOperationWithProgress<ulong, ulong> WriteToStreamAsync(IOutputStream outputStream)
		{
			return InnerContent.WriteToStreamAsync(outputStream);
		}

		public HttpContentHeaderCollection Headers { get { return InnerContent.Headers; } }

		#endregion

	}
}