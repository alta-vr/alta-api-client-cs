using AltaClient.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Groups
{
	public class ServerConnection
	{
		static Logger Logger { get; } = new Logger("ServerConnection");

		public Server Server { get; }

		public event Action<ServerConnection> Closed;
		public event Action<ServerConnection, ReceivingMessage> SystemMessageReceived;
		public event Action<ServerConnection, Exception> ErrorReceived;


		Task initializing;

		bool? isAllowed;

		Dictionary<string, Action<object>> subscriptions = new Dictionary<string, Action<object>>();
		Dictionary<int, TaskCompletionSource<object>> commandCallbacks = new Dictionary<int, TaskCompletionSource<object>>();

		WebsocketConnection connection;

		public ServerConnection(Server server)
		{
			Server = server;

			connection = new WebsocketConnection(server);

			connection.MessageReceived += HandleMessage;
			connection.ErrorReceived += HandleError;
			connection.CloseReceived += HandleClose;
		}

		public Task Reattempt()
		{
			if (isAllowed == false)
			{
				isAllowed = null;
			}

			return WaitReady();
		}
		
		public Task WaitReady()
		{
			if (initializing == null)
			{
				initializing = connection.Connect();
			}

			return initializing;
        }

		void HandleMessage(ReceivingMessage data)
		{
			switch (data.type)
			{
				case ReceivingMessage.MessageType.SystemMessage:
					SystemMessageReceived?.Invoke(this, data);
					break;

				case ReceivingMessage.MessageType.Subscription:
					if (subscriptions.TryGetValue(data.eventType, out Action<object> callback))
					{
						callback.Invoke(data.data);
					}
					break;

				case ReceivingMessage.MessageType.CommandResult:
					if (commandCallbacks.TryGetValue(data.commandId, out TaskCompletionSource<object> completion))
					{
						completion.SetResult(data.data);
						commandCallbacks.Remove(data.commandId);
					}
					break;

				default:
					Logger.Warn("Unhandled message:");
					Logger.Info(data);
					break;
			}
		}

		void HandleError(Exception data)
		{
			Logger.Error("Connection threw an error");
			Logger.Info(data);

			ErrorReceived?.Invoke(this, data);
		}

		void HandleClose()
		{
			Logger.Info("Connection closed");

			Closed?.Invoke(this);
		}

		public Task<object> Send(string command)
		{
			SendingMessage message = new SendingMessage(command);

			connection.Send(message);

			TaskCompletionSource<object> result = new TaskCompletionSource<object>();

			commandCallbacks.Add(message.Id, result);

			return result.Task;
		}

		public async Task<object> Subscribe(string eventName, Action<object> callback)
		{
			Logger.Info("Subscribing to " + eventName);
			
			if (subscriptions.TryGetValue(eventName, out Action<object> existing))
			{
				existing += callback;
			}
			else
			{
				subscriptions.Add(eventName, callback);
			}

			dynamic result = await Send("websocket subscribe " + eventName);

			if (result.Exception != null)
			{
				Logger.Error($"Failed to subscribe to {eventName}");
				Logger.Info(result.Exception);
			}
			else
			{
				Logger.Info($"Subscribed to {eventName} : {result.ResultString}");
			}

			return result;
		}

		public async Task<object> Unsubscribe(string eventName, Action<object> callback)
		{
			Logger.Info("Unsubscribing from " + eventName);

			if (subscriptions.TryGetValue(eventName, out Action<object> existing))
			{
				existing -= callback;

				if (existing == null)
				{
					subscriptions.Remove(eventName);
				}
			}

			dynamic result = await Send("websocket unsubscribe " + eventName);

			if (result.Exception != null)
			{
				Logger.Error($"Failed to unsubscribe from {eventName}");
				Logger.Info(result.Exception);
			}
			else
			{
				Logger.Info($"Unsubscribed from {eventName} : { result.ResultString}");
			}

			return result;
		}

		public void Disconnect()
		{
			connection.Terminate();
		}
	}
}
