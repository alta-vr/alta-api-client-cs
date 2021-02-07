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

			ApiClient = new HttpClient();
			ApiClient.SetBearerToken(response.AccessToken);
			ApiClient.DefaultRequestHeaders.Add("x-api-key", "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf");
			ApiClient.DefaultRequestHeaders.Add("User-Agent", config.ClientId);

			ApiClient.BaseAddress = new Uri(config.Endpoint ?? "https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/test/api/");

			JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

			JwtSecurityToken token = handler.ReadJwtToken(response.AccessToken);

			UserId = int.Parse(token.Claims.First(item => item.Type == "client_sub").Value);

			Logger.Success("User ID: " + UserId);
			Logger.Success("Username: " + token.Claims.First(item => item.Type == "client_username").Value);
		}


		public async Task<JObject> Fetch(HttpMethod method, string path, object body = null)
		{
			HttpResponseMessage message = await ApiClient.SendAsync(new HttpRequestMessage(method, path)
			{
				Content = body == null ? null : new StringContent(JsonConvert.SerializeObject(body))
			});
			
			if (message.IsSuccessStatusCode)
			{
				return JObject.Parse(await message.Content.ReadAsStringAsync());
			}

			throw new HttpError(method, path, message.StatusCode, message.ReasonPhrase, null);
		}

		public async Task<int> ResolveUsernameOrId(string value)
		{
			if (int.TryParse(value, out int id))
			{
				return id;
			}

			JObject result = await Fetch(HttpMethod.Post, "users/search/username", new { username = value });

			return result.GetValue("id").Value<int>();
		}
	}
}
