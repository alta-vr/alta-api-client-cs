using Newtonsoft.Json;

namespace AltaClient.Core
{
	public class Config
	{
		[JsonProperty("client_id")]
		public string ClientId { get; private set; }

		[JsonProperty("client_secret")]
		public string ClientSecret { get; private set; }

		[JsonProperty("scope")]
		public string Scope { get; private set; }

		[JsonProperty("endpoint")]
		public string Endpoint { get; private set; }
	}
}
