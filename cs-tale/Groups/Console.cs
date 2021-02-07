using System.Threading.Tasks;

namespace AltaClient.Groups
{
	public class Console
	{
		public Server Server { get; }

		public Console(Server server)
		{
			Server = server;
		}

		public async Task WaitReady()
		{
			//TODO
		}
	}
}
