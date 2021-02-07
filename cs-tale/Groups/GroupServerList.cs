using AltaClient.Core;
using System.Threading.Tasks;

namespace AltaClient.Groups
{
	//public class GroupServerList : LiveList<Server>
	//{
	//	public Group Group { get; }
	//	public GroupManager Manager { get; }

	//	public GroupServerList(Group group) : 
	//		base($"{group.Info.Name} servers",
	//		() => Task.FromResult(group.Info.Servers),
	//		callback => group.Manager.Subscriptions.Subscribe("group-server-create", group.Info.Identifier, callback),
	//		callback => group.Manager.Subscriptions.Subscribe("group-server-update", group.Info.Identifier, callback),
	//		callback => group.Manager.Subscriptions.Subscribe("group-server-update", group.Info.Identifier, callback),
	//		data => data.Identifier,
	//		server => server.Info.Identifier,
	//        data => new Server(group, data))
	//	{
	//		Group = group;
	//		Manager = group.Manager;
	//	}
	//}
}