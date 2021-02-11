using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Core
{
	public class HttpError : Exception
	{
		public HttpMethod Method { get; }
		public string Path { get; }
		public HttpStatusCode Code { get; }

		public HttpError (HttpMethod method, string path, HttpStatusCode code, string message, Exception inner) : base(message, inner)
		{
			Method = method;
			Path = path;
			Code = code;
		}
	}

	public class ApiConnection
	{
		static Logger Logger { get; } = new Logger("ApiConnection");

		public HttpClient ApiClient { get; private set; }
		
		public int UserId { get; private set; }

		public string ClientId { get; private set; }

		string accessToken;
		
		public async Task Login(Config config)
		{
			HttpClient client = new HttpClient();

			DiscoveryDocumentResponse discovery = await client.GetDiscoveryDocumentAsync("https://accounts.townshiptale.com");

			if (discovery.IsError)
			{
				throw new Exception(discovery.Error);
			}
			
			TokenClientOptions options = new TokenClientOptions
			{
				ClientId = config.ClientId,
				ClientSecret = config.ClientSecret
			};

			TokenResponse response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discovery.TokenEndpoint,

				ClientId = config.ClientId,
				ClientSecret = config.ClientSecret,

				Scope = config.Scope
			});

			if (response.IsError)
			{
				throw new Exception(response.Error);
			}

			accessToken = response.AccessToken;

			JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

			JwtSecurityToken token = handler.ReadJwtToken(accessToken);

			UserId = int.Parse(token.Claims.First(item => item.Type == "client_sub").Value);
			ClientId = config.ClientId;

			ApiClient = new HttpClient();

			foreach (var pair in GetHeaders())
			{
				ApiClient.DefaultRequestHeaders.Add(pair.Item1, pair.Item2);
			}

			ApiClient.BaseAddress = new Uri(config.Endpoint ?? "https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/test/api/");
			
			Logger.Success("User ID: " + UserId);
			Logger.Success("Username: " + token.Claims.First(item => item.Type == "client_username").Value);
		}

		public IEnumerable<(string, string)> GetHeaders()
		{
			yield return ("Authorization", "Bearer " + accessToken);
			yield return ("x-api-key", "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf");
			yield return ("User-Agent", ClientId);
		}

		public async Task<T> FetchAs<T>(HttpMethod method, string path, object body = null)
		{
			JToken result = await Fetch(method, path, body);

			return result.ToObject<T>();
		}

		public async Task<JArray> FetchArray(HttpMethod method, string path, object body = null)
		{
			var result = await Fetch(method, path, body);

			return (JArray)result;
		}

		public async Task<JToken> Fetch(HttpMethod method, string path, object body = null)
		{
			HttpResponseMessage message = await ApiClient.SendAsync(new HttpRequestMessage(method, path)
			{
				Content = body == null ? null : new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
			});
			
			if (message.IsSuccessStatusCode)
			{
				return JToken.Parse(await message.Content.ReadAsStringAsync());
			}

			throw new HttpError(method, path, message.StatusCode, message.ReasonPhrase, null);
		}
		
		public async Task<int> ResolveUsernameOrId(string value)
		{
			if (int.TryParse(value, out int id))
			{
				return id;
			}

			JToken result = await Fetch(HttpMethod.Post, "users/search/username", new { username = value });
			
			return ((JObject)result).GetValue("id").Value<int>();
		}
	}
}
