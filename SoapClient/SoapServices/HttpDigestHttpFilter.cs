using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace SoapServices
{
	public class HttpDigestHttpFilter : IHttpFilter
	{
		readonly Dictionary<Uri, DigestAuthParameters> _parameters = new Dictionary<Uri, DigestAuthParameters>();

		public IHttpFilter InnerFilter { get; private set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		protected Random Rand { get; private set; }

		public HttpDigestHttpFilter(IHttpFilter innerFilter)
		{
			if (innerFilter == null) throw new ArgumentNullException("innerFilter");

			InnerFilter = innerFilter;
			Rand = new Random();
		}

		public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
		{
			return AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
			{
				DigestAuthParameters p;
				if (_parameters.TryGetValue(request.RequestUri, out p))
				{
					var authorizationHeader = GetAuthorizationHeader(p);
					request.Headers.Authorization = authorizationHeader;
				}

				HttpResponseMessage response = await InnerFilter.SendRequestAsync(request).AsTask(cancellationToken, progress);

				if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.Any())
				{
					var authHeader = response.Headers.WwwAuthenticate[0];
					bool isDigest = authHeader.Scheme == "Digest";
					if (isDigest)
					{
						if (p != null)
						{
							_parameters.Remove(request.RequestUri);
						}

						var authParameters = InitDigestAuthParameters(authHeader, response);
						_parameters.Add(request.RequestUri, authParameters);
						var authorizationHeader = GetAuthorizationHeader(authParameters);

						var requestCopy = request.Clone();
						requestCopy.Headers.Authorization = authorizationHeader;
						response = await InnerFilter.SendRequestAsync(requestCopy).AsTask(cancellationToken, progress);

						var headerValue = response.Headers.WwwAuthenticate.FirstOrDefault(h => h.Scheme == "Digest");
						if (headerValue != null)
						{
							var nonceParam = headerValue.Parameters.FirstOrDefault(hp => hp.Name == "nonce");
							authParameters.Nonce = nonceParam.Value;
						}
					}
				}

				return response;
			});
		}

		private DigestAuthParameters InitDigestAuthParameters(HttpChallengeHeaderValue authHeader,
			HttpResponseMessage response)
		{
			var authParameters = new DigestAuthParameters();

			foreach (var headerValue in authHeader.Parameters)
			{
				switch (headerValue.Name)
				{
					case "realm":
						authParameters.Realm = headerValue.Value;
						break;

					case "nonce":
						authParameters.Nonce = headerValue.Value;
						break;

					case "algorithm":
						authParameters.Algorithm = headerValue.Value;
						break;

					case "opaque":
						authParameters.Opaque = headerValue.Value;
						break;
				}
			}

			if (String.IsNullOrEmpty(authParameters.Algorithm))
				authParameters.Algorithm = "MD5";

			authParameters.Uri = response.RequestMessage.RequestUri.AbsolutePath;
			authParameters.Cnonce = GetNonce();

			return authParameters;
		}

		private HttpCredentialsHeaderValue GetAuthorizationHeader(DigestAuthParameters authParameters)
		{
			string nonceCounterString = string.Format("{0:x08}", authParameters.NonceCounter++);

			var authorizationHeader = new HttpCredentialsHeaderValue("Digest");
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("username", UserName));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("realm", authParameters.Realm));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("qop", "auth"));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("uri", authParameters.Uri));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("nonce", authParameters.Nonce));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("nc", nonceCounterString));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("cnonce", authParameters.Cnonce));
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("algorithm", authParameters.Algorithm));
			if (!String.IsNullOrEmpty(authParameters.Opaque))
				authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("opaque", authParameters.Opaque));

			var a1 = string.Format("{0}:{1}:{2}", UserName, authParameters.Realm, Password);
			string a2 = string.Format("POST:{0}", authParameters.Uri);

			string ha1 = GetMD5HashBinHex(a1);
			string ha2 = GetMD5HashBinHex(a2);

			string a = string.Format("{0}:{1}:{4}:{2}:auth:{3}", ha1, authParameters.Nonce, authParameters.Cnonce, ha2,
				nonceCounterString);
			string responseVal = GetMD5HashBinHex(a);
			authorizationHeader.Parameters.Add(new HttpNameValueHeaderValue("response", responseVal));
			return authorizationHeader;
		}

		private string GetNonce()
		{
			byte[] nonce = new byte[16];
			Rand.NextBytes(nonce);
			return GetHex(nonce);
		}

		private static string GetHex(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "");
		}

		private static string GetMD5HashBinHex(string val)
		{
			var hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
			IBuffer buff = CryptographicBuffer.ConvertStringToBinary(val, BinaryStringEncoding.Utf8);
			var hashed = hashAlgorithm.HashData(buff);
			var res = CryptographicBuffer.EncodeToHexString(hashed);
			return res;
		}

		public void Dispose()
		{
			InnerFilter.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}