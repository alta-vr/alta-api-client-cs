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
		public int Identifier { get; private set; }
		public string Name { get; private set; }
		public UserInfo[] OnlinePlayers { get; private set; }
		public string ServerStatus { get; private set; }
		public int SceneIndex { get; private set; }
		public string Region { get; private set; }
		public string OnlinePing { get; private set; }
		public string Description { get; private set; }
		public int Playability { get; private set; }
		public string Version { get; private set; }
		public string Type { get; private set; }
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

		ServerConnection console;

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

		public async Task<ServerConnection> GetConsole()
		{
			if (console == null)
			{
				console = new ServerConnection(this);

				//console.Closed += ConsoleClosed;
			}

			await console.WaitReady();

			return console;
		}
		
		void ConsoleDisconnect(ServerConnection console)
		{
			Logger.Error($"Console to {Info.Name} disconnected.");

			console = null;
		}
	}
}
