using AltaClient.Core;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AltaClient.Groups;

namespace CsTaleTest
{
	class Program
	{
		static Logger Logger { get; } = new Logger("Program");

		static void Main(string[] args)
		{
			try
			{
				Run().GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				Logger.Error(e.Message);
				Logger.Error(e.InnerException?.Message);
			}

			System.Console.ReadLine();
		}

		static async Task Run()
		{ 
			Config config;

			try
			{
				Logger.Info("Reading config");

				string configValue = File.ReadAllText("./config.json");

				config = JsonConvert.DeserializeObject<Config>(configValue);
			}
			catch (Exception e)
			{
				Logger.Fatal("Error parsing config.json");
				Logger.Fatal(e);

				System.Console.ReadLine();

				return;
			}

			ApiConnection connection = new ApiConnection();
			
			await connection.Login(config);

			SubscriptionManager subscriptions = new SubscriptionManager(connection);

			await subscriptions.Initialize();

			GroupManager groupManager = new GroupManager(subscriptions);

			await groupManager.AcceptAllInvites(true);

			foreach (Group group in await groupManager.Groups.Refresh(true))
			{
				Logger.Info(group.Info.Name);
			}
		}
	}
}
