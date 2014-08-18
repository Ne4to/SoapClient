using System;
using Windows.Web.Http;

namespace SoapServices
{
	public static class HttpRequestMessageExtensions
	{
		public static HttpRequestMessage Clone(this HttpRequestMessage request)
		{
			var copy = new HttpRequestMessage(request.Method, request.RequestUri);
			copy.Content = request.Content;

			foreach (var header in request.Headers)
			{

				bool result = copy.Headers.TryAppendWithoutValidation(header.Key, header.Value);
				if (!result)
				{
					throw new Exception("Unable to copy headers.");
				}
			}

			foreach (var property in request.Properties)
			{
				copy.Properties.Add(property.Key, property.Value);
			}

			return copy;
		}
	}
}