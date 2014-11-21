using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Web.Http;

namespace SoapServices
{
	public abstract class SoapClientBase
	{
		public static readonly XNamespace DefaultEnvelopeNamespace = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");
		private const string DefaultContentType = "application/soap+xml";

		private readonly Lazy<HttpClient> _lazyClient;

		public Uri EndpointAddress { get; set; }
		public XNamespace EnvelopeNamespace { get; set; }
		public string ContentType { get; set; }

		protected SoapClientBase()
		{
			_lazyClient = new Lazy<HttpClient>(CreateHttpClient);
			EnvelopeNamespace = DefaultEnvelopeNamespace;
			ContentType = DefaultContentType;
		}

		public Func<HttpClient> CustomClientInitFunc { get; set; }

		protected virtual HttpClient CreateHttpClient()
		{
			if (EndpointAddress == null)
				throw new Exception("EndpointAddress is not set");

			if (CustomClientInitFunc != null)
				return CustomClientInitFunc();

			return new HttpClient();
		}

		protected HttpClient Client { get { return _lazyClient.Value; } }

		public async Task<TResponse> CallAsync<TRequest, TResponse>(string action, TRequest request)
		{
			IHttpContent httpContent = GetHttpContent(action, request);
			var response = await Client.PostAsync(EndpointAddress, httpContent);
			var responseContent = await response.Content.ReadAsStringAsync();
			return GetResponse<TResponse>(responseContent);
		}

		private TResponse GetResponse<TResponse>(string responseContent)
		{
			var doc = XDocument.Parse(responseContent);
			var responseMessage = new ResponseMessage(doc, EnvelopeNamespace);
			return responseMessage.GetContent<TResponse>();
		}

		private IHttpContent GetHttpContent<TRequest>(string action, TRequest request)
		{
			return new SoapMessageContent
			{
				Action = action,
				BodyContent = request,
				EnvelopeNamespace = EnvelopeNamespace,
				ContentType = ContentType,				
			};
		}
	}
}