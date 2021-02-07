using AltaClient.Core;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

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
				Console.WriteLine(e.Message);
				Console.WriteLine(e.InnerException?.Message);
			}

			Console.ReadLine();
		}

		static async Task Run()
		{ 
			Console.WriteLine("Hello World!");

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

				Console.ReadLine();

				return;
			}

			ApiConnection connection = new ApiConnection();


			Logger.Info("Logging in");
			await connection.Login(config);
		}
	}
}
