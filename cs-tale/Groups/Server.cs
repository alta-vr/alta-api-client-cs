using AltaClient.Core;
using System.Threading.Tasks;

namespace AltaClient.Groups
{
	public class UserInfo
	{
		public int Identfier { get; }
		public string Username { get; }
	}

	public class ServerInfo
	{
		public int Identifier { get; }
		public string Name { get; }
		public UserInfo[] OnlinePlayers { get; }
		public string ServerStatus { get; }
		public int SceneIndex { get; }
		public string Region { get; }
		public string OnlinePing { get; }
		public string Description { get; }
		public int Playability { get; }
		public string Version { get; }
		public string Type { get; }
	}

	public class Server
	{
		static Logger Logger { get; } = new Logger("Server");

		public delegate void InfoChangedHandler(Server server, ServerInfo oldInfo);

		public Group Group { get; }
		public ServerInfo Info { get; }

		public bool IsOnline { get; private set; }

		public event InfoChangedHandler Updated;
		public event InfoChangedHandler StatusChanged;

		Console console;

		public Server(Group group, ServerInfo info)
		{
			Group = group;
			Info = info;

			EvaluateState();
		}

		void EvaluateState()
		{
			IsOnline = Info.OnlinePing != null && (System.DateTime.Now - System.DateTime.Parse(Info.OnlinePing)) < System.TimeSpan.FromMinutes(10);
		}

		//Provided by LiveList update
		public void OnUpdate(ServerInfo oldInfo)
		{
			EvaluateState();

			Updated?.Invoke(this, oldInfo);
		}

		public void OnStatus(ServerInfo info)
		{
			//var cache = Info.Clone();

			//Info.Merge(info);

			EvaluateState();

			//StatusChanged?.Invoke(this, cache);
		}

		public async Task<Console> GetConsole()
		{
			if (console == null)
			{
				console = new Console(this);

				//console.Closed += ConsoleClosed;
			}

			await console.WaitReady();

			return console;
		}
		
		void ConsoleDisconnect(Console console)
		{
			Logger.Error($"Console to {Info.Name} disconnected.");

			console = null;
		}
	}
}
