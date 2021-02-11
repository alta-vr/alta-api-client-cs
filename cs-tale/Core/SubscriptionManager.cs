using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace AltaClient.Core
{
	public class SubscriptionManager
	{
		static Logger Logger { get; } = new Logger("SubscriptionManager");

		public ApiConnection Api { get; }

		public event Action<string> MessageReceived;

		WebSocket websocket;

		Dictionary<uint, TaskCompletionSource<JObject>> callbacks = new Dictionary<uint, TaskCompletionSource<JObject>>();
		Dictionary<string, Action<JObject>> subscriptions = new Dictionary<string, Action<JObject>>();

		uint nextId = 0;
		string authorization;

		public SubscriptionManager(ApiConnection api)
		{
			Api = api;
		}

		public async Task Initialize()
		{
			List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
	
			foreach (var item in Api.GetHeaders())
			{
				headers.Add(new KeyValuePair<string, string>(item.Item1, item.Item2));
			}

			authorization = headers.Find(item => item.Key == "Authorization").Value;

			websocket = new WebSocket("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev", customHeaderItems: headers);

			websocket.MessageReceived += OnMessageReceived;
			websocket.Closed += Closed;

			await websocket.OpenAsync();
		}


		public Task<JObject> Subscribe(string eventName, object sub, Action<JObject> callback)
		{
			if (websocket == null)
			{
				throw new Exception("Subscription manager must have init called first");
			}

			uint id = ++nextId;
			
			string subscription = $"{eventName}-{sub}";

			if (!subscriptions.TryGetValue(subscription, out Action<JObject> existing))
			{
				subscriptions.Add(subscription, callback);
			}
			else
			{
				existing += callback;
			}

			websocket.Send(JsonConvert.SerializeObject(new
			{
				id,
				method = "POST",
				path = $"subscription/{eventName}/{sub}",
				authorization
			}));

			TaskCompletionSource<JObject> source = new TaskCompletionSource<JObject>();

			callbacks[id] = source;

			return source.Task;
		}

		void OnMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			if (args.Message != null)
			{
				JObject parsed;

				try
				{
					parsed = JObject.Parse(args.Message);
				}
				catch (Exception e)
				{
					MessageReceived?.Invoke(args.Message);
					return;
				}

				try
				{
					if (parsed.TryGetValue("id", out JToken token))
					{
						uint id = token.Value<uint>();

						var callback = callbacks[id];
						callbacks.Remove(id);

						if (parsed.GetValue("responseCode").Value<int>() == 200)
						{
							callback.SetResult(parsed);
						}
						else
						{
							callback.SetException(new Exception(args.Message));
						}
						
						return;
					}

					if (parsed.TryGetValue("event", out token))
					{
						string content = parsed.GetValue("content").Value<string>();
						string eventName = parsed.GetValue("event").Value<string>();

						parsed.SelectToken("content").Replace(new JObject(content));

						if (subscriptions.TryGetValue(eventName, out Action<JObject> callback))
						{
							callback(parsed);
						}

						if (parsed.TryGetValue("key", out JToken key))
						{
							if (subscriptions.TryGetValue($"{eventName}-{key.Value<string>()}", out callback))
							{
								callback(parsed);
							}
						}
					}
				}
				catch (Exception e)
				{
					Logger.Error(e);
					Logger.Error(parsed);
				}
			}
		}

		void Closed(object sender, EventArgs e)
		{
			//TODO: Handle Code 1001

			Logger.Error($"WebAPI Websocket closed. Code: ???. Reason: ???.");
		}
	}
}
