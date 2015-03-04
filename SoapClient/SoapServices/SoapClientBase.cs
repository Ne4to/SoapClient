using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Web.Http;

namespace SoapServices
{
	public abstract class SoapClientBase
	{
		public static readonly XNamespace Soap11EnvelopeNamespace = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
		private const string Soap11ContentType = "text/xml";

		public static readonly XNamespace Soap12EnvelopeNamespace = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");
		private const string Soap12ContentType = "application/soap+xml";

		private readonly Lazy<HttpClient> _lazyClient;

		public Uri EndpointAddress { get; set; }
		public SoapVersion SoapVersion { get; set; }
		
		protected SoapClientBase()
		{
			_lazyClient = new Lazy<HttpClient>(CreateHttpClient);
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

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, EndpointAddress);
			requestMessage.Content = httpContent;
			
			if (SoapVersion == SoapVersion.Soap11 && !String.IsNullOrEmpty(action))
				requestMessage.Headers.Add("SOAPAction", "\"" + action + "\"");

			var response = await Client.SendRequestAsync(requestMessage);

			//var response = await Client.PostAsync(EndpointAddress, httpContent);
			var responseContent = await response.Content.ReadAsStringAsync();
			return GetResponse<TResponse>(responseContent);
		}

		private TResponse GetResponse<TResponse>(string responseContent)
		{
			var doc = XDocument.Parse(responseContent);
			var responseMessage = new ResponseMessage(doc, Soap11EnvelopeNamespace);
			return responseMessage.GetContent<TResponse>();
		}

		private IHttpContent GetHttpContent<TRequest>(string action, TRequest request)
		{
			return new SoapMessageContent
			{
				Action = action,
				BodyContent = request,
				EnvelopeNamespace = Soap11EnvelopeNamespace,
				ContentType = Soap11ContentType,				
			};
		}
	}
}