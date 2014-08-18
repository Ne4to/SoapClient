namespace SoapServices
{
	public class DigestAuthParameters
	{
		public int NonceCounter { get; set; }
		public string Realm{ get; set; }
		public string Cnonce{ get; set; }
		public string Algorithm{ get; set; }
		public string Nonce{ get; set; }
		public string Opaque{ get; set; }
		public string Uri{ get; set; }

		public DigestAuthParameters()
		{
			NonceCounter = 1;
		}
	}
}