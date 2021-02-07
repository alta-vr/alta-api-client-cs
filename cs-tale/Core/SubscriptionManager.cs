using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Core
{
	public class SubscriptionManager
	{
		public ApiConnection Api { get; }

		public SubscriptionManager(ApiConnection api)
		{
			Api = api;
		}

		public async Task Initialize()
		{
			//TODO:
		}

		public async Task Subscribe(string eventName, object sub, Action<object> callback)
		{
			//TODO:
		}
	}
}
