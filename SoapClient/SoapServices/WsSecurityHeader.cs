using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace SoapServices
{
	/// <summary>
	/// WS-Security 1.1 SOAP Header
	/// </summary>
	public class WsSecurityHeader : IMessageHeader
	{
		/// <summary>
		/// Possible password types
		/// </summary>
		public enum PasswordTypes
		{
			PasswordText,
			PasswordDigest
		};

		private static readonly XNamespace NsWsse = XNamespace.Get("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
		private static readonly XNamespace NsWsu = XNamespace.Get("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

		public string Username { get; set; }
		public string Password { get; set; }
		public TimeSpan? TimeDiff { get; set; }
		public PasswordTypes PasswordType { get; set; }

		public WsSecurityHeader(string username, string password)
			: this(username, password, PasswordTypes.PasswordDigest, null)
		{
		}

		public WsSecurityHeader(string username, string password, PasswordTypes passwordType, TimeSpan? timeDiff)
		{
			Username = username;
			Password = password;
			PasswordType = passwordType;
			TimeDiff = timeDiff;
		}

		public XElement GetXml()
		{
			var securityElement = new XElement(XName.Get("Security", NsWsse.NamespaceName));
			securityElement.SetAttributeValue(XNamespace.Xmlns + "wsse", NsWsse);
			securityElement.SetAttributeValue(XNamespace.Xmlns + "wsu", NsWsu);

			if (!String.IsNullOrEmpty(Username))
			{
				var usernameTokenElement = new XElement(NsWsse + "UsernameToken");
				usernameTokenElement.Add(new XElement(NsWsse + "Username", Username));

				if (PasswordTypes.PasswordText == PasswordType)
				{
					WriteClearTextPassword(usernameTokenElement);
				}
				else
				{
					WritePasswordDigest(usernameTokenElement);
				}

				securityElement.Add(usernameTokenElement);
			}

			return securityElement;
		}

		/// <summary>
		/// Retrieves current time as UTC in W3DTF format.
		/// </summary>
		/// <returns>Current time as string</returns>
		protected string GetTimestamp()
		{
			var time = TimeDiff.HasValue ? DateTime.UtcNow - TimeDiff.Value : DateTime.UtcNow;
			return time.ToString("yyyy-MM-ddTHH:mm:ssZ"); //W3DTF format;
		}

		/// <summary>
		/// Generates nonce.
		/// </summary>
		/// <returns>Nonce value as Base64 encoded string</returns>
		protected string GetNonce()
		{
			var nonce = CryptographicBuffer.GenerateRandom(16);
			return CryptographicBuffer.EncodeToBase64String(nonce);
		}

		/// <summary>
		/// Generates password hash using SHA-1 algorithm
		/// </summary>
		/// <param name="nonce">nonce</param>
		/// <param name="timestamp">Current UTC time</param>
		/// <returns>Password hash as Base64 encoded stirng</returns>
		protected string GetPasswordHash(string nonce, string timestamp)
		{
			var algorithm = HashAlgorithmProvider.OpenAlgorithm("SHA1");
			var data = new List<byte>();

			data.AddRange(Convert.FromBase64String(nonce));
			data.AddRange(Encoding.UTF8.GetBytes(timestamp + Password));

			var buffData = CryptographicBuffer.CreateFromByteArray(data.ToArray());
			var buffHash = algorithm.HashData(buffData);

			return CryptographicBuffer.EncodeToBase64String(buffHash);
		}

		private void WritePasswordDigest(XElement tokenElement)
		{
			var nonce = GetNonce();
			var timestamp = GetTimestamp();
			var passHash = GetPasswordHash(nonce, timestamp);

			var typeAttr = new XAttribute("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest");
			tokenElement.Add(new XElement(NsWsse + "Password", typeAttr, passHash));
			tokenElement.Add(new XElement(NsWsse + "Nonce", nonce));
			tokenElement.Add(new XElement(NsWsu + "Created", timestamp));
		}

		private void WriteClearTextPassword(XElement tokenElement)
		{
			var typeAttr = new XAttribute("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText");

			tokenElement.Add(new XElement(NsWsse + "Password", typeAttr, Password));
		}
	}
}