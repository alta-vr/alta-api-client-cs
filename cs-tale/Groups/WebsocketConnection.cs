using AltaClient.Core;
using Newtonsoft.Json;
using SuperSocket.ClientEngine;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebSocket4Net;

namespace AltaClient.Groups
{

	public class SendingMessage
	{
		public int Id { get; }
		public string Content { get; }

		static int nextId;

		public SendingMessage(string content)
		{
			Id = nextId++;

			Content = content;
		}
	}

	public class ReceivingMessage
	{
		public enum MessageType
		{
			SystemMessage,
			Subscription,
			CommandResult
		}

		public int id;
		public MessageType type;
		public string timeStamp;
		public string eventType;
		public object data;
		public int commandId;
	}
	
	public class WebsocketConnection
	{
		static Logger Logger { get; }

		public class ConnectRequestResponse
		{
			public class Connection
			{
				public string address;
				public string websocket_port;
			}

			public Connection connection;
			public string token;
		}

		ConnectRequestResponse response;
		WebSocket websocket;

		bool isAllowed;

		TaskCompletionSource<bool> connected = new TaskCompletionSource<bool>();
		
		public ApiConnection Api { get; }

		public Server Server { get; }
		
		public event Action<ReceivingMessage> MessageReceived;
		public event Action<Exception> ErrorReceived;
		public event Action CloseReceived;

		int nextReceiveId;

		public WebsocketConnection(Server server)
		{
			Server = server;

			Api = Server.Group.Manager.Api;
		}

		public async Task<bool> Connect()
		{
			try
			{
				dynamic details = await Api.Fetch(HttpMethod.Post, $"servers/{Server.Info.Identifier}/console", new object());

				response = details.ToObject<ConnectRequestResponse>();

				if (details.allowed)
				{
					Logger.Success($"Connecting to {Server.Info.Name}");
					
					isAllowed = true;

					await Join();
				}
				else
				{
					isAllowed = false;
				}
			}
			catch (HttpError e)
			{
				if (e.Code != HttpStatusCode.Forbidden)
				{
					Logger.Error("Unexpected error connecting to server");
					Logger.Info(e);
				}
				else
				{
					isAllowed = false;
				}
			}
				
			return true;
		}

		public Task<bool> Join()
		{
			string uri = "ws://" + response.connection.address + ":" + response.connection.websocket_port;

			websocket = new WebSocket(uri);

			websocket.Opened += WebsocketOpened;
			websocket.Error += WebsocketError;
			websocket.Closed += WebsocketClosed;
			websocket.MessageReceived += WebsocketMessage;
			websocket.DataReceived += DataReceived;
			websocket.Open();

			Console.WriteLine("Connecting to " + uri);

			return connected.Task;
		}

		private void DataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("Data received");
		}

		void WebsocketMessage(object sender, MessageReceivedEventArgs e)
		{
			Console.WriteLine("Websocket message");
			Console.WriteLine(e.Message);

			ReceivingMessage data = JsonConvert.DeserializeObject<ReceivingMessage>(e.Message);

			data.id = nextReceiveId++;
			
			MessageReceived?.Invoke(data);
		}

		void WebsocketClosed(object sender, EventArgs e)
		{
			Console.WriteLine("Websocket closed");

			CloseReceived?.Invoke();
		}

		void WebsocketError(object sender, ErrorEventArgs e)
		{
			Console.WriteLine("Webscoket error");
			Console.WriteLine(e.Exception?.Message);

			connected.TrySetResult(false);

			ErrorReceived?.Invoke(e.Exception);
		}

		void WebsocketOpened(object sender, EventArgs e)
		{
			Console.WriteLine("Successfully connected to server");

			websocket.Send(response.token);

			connected.SetResult(true);
		}

		public void Send(SendingMessage message)
		{
			Console.WriteLine("Sending : " + message.Content);
			websocket.Send(JsonConvert.SerializeObject(message));
		}

		public void Terminate()
		{
			websocket.Close();
		}
	}
}
