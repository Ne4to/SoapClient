using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace SoapServices
{
	public class SoapDigestHttpFilter : IHttpFilter
	{
		private bool _addHeader;
		public IHttpFilter InnerFilter { get; private set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public TimeSpan? TimeDiff { get; set; }

		protected Random Rand { get; private set; }

		public SoapDigestHttpFilter(IHttpFilter innerFilter)
		{
			if (innerFilter == null) throw new ArgumentNullException("innerFilter");

			InnerFilter = innerFilter;
			Rand = new Random();
		}

		public void Dispose()
		{
			InnerFilter.Dispose();
			GC.SuppressFinalize(this);
		}

		public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
		{
			return AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
			{
				var sendRequest = request;

				if (_addHeader)
				{
					sendRequest = CopyRequest(request);					
				}

				HttpResponseMessage response = await InnerFilter.SendRequestAsync(sendRequest).AsTask(cancellationToken, progress);

				if (!_addHeader && !response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var msg = new ResponseMessage(XDocument.Parse(responseContent));
					if (msg.Fault != null)
					{
						var requestCopy = CopyRequest(request);

						response = await InnerFilter.SendRequestAsync(requestCopy).AsTask(cancellationToken, progress);
						if (response.IsSuccessStatusCode)
							_addHeader = true;
					}
				}

				return response;
			});
		}

		private HttpRequestMessage CopyRequest(HttpRequestMessage request)
		{
			var requestCopy = request.Clone();
			var content = new SoapMessageContent(requestCopy.Content as SoapMessageContent);
			content.SoapHeaders.Add(new WsSecurityHeader(UserName, Password, WsSecurityHeader.PasswordTypes.PasswordDigest,
				TimeDiff));
			requestCopy.Content = content;
			return requestCopy;
		}
	}
}