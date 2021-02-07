using AltaClient.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Groups
{
	public delegate void GroupEventHandler(Group group);

	public class Group
	{
		static Logger Logger { get; } = new Logger("Group");

		public GroupManager Manager { get; }
		public GroupInfo Info { get; private set; }

		public GroupMember Member { get; private set; }

		//public GroupMemberList<GroupMemberInvite> Invites { get; }
		//public GroupMemberList<GroupMemberRequest> Requests { get; }
		//public GroupMemberList<GroupMemberBan> Bans { get; }
		//public GroupMemberList<GroupMember> Members { get; }

		//public GroupServerList Servers { get; }

		public bool IsConsoleAutomatic { get; private set; }

		public event GroupEventHandler Updated;

		public Group(GroupManager manager, GroupInfo info, GroupMember member = null)
		{
			Manager = manager;
			Info = info;
			Member = member;

			int id = info.Identifier;

			//Members = CreateList("members", "member", true, data => new GroupMember(this, data));
			//Invites = CreateList("invites", "invite", false, data => new GroupMemberInvite(this, data));
			//Requests = CreateList("requests", "request", false, data => new GroupMemberRequest(this, data));
			//Bans = CreateList("bans", "ban", false, data => new GroupMemberBan(this, data));

			//Servers = new GroupServerList(this);
		}

		//GroupMemberList<T> CreateList<T>(string route, string name, bool hasUpdate, Func<object, T> create)
		//{
		//	int id = Info.Identifier;

		//	string createSub = $"group-{name}-create";
		//	string deleteSub = $"group-{name}-delete";
		//	string updateSub = $"group-{name}-update";

		//	Action<T> update = null;

		//	if (hasUpdate)
		//	{
		//		update = callback => Manager.subscriptions.subscribe(updateSub, id, callback);
		//	}
			
		//	GroupMemberList<T> list = new GroupMemberList($"{Info.Name} {name}",
		//	() => Manager.Api.Fetch("GET", $"groups/{id}/{route}"),
		//	callback => Manager.subscriptions.subscribe(createSub, id, callback),
		//	callback => Manager.subscriptions.subscribe(deleteSub, id, callback),
		//	update,
		//	create);

		//	return list;
		//}

		public void Dispose()
		{
			Logger.Info($"Left {Info.Identifier} - {Info.Name}");
		}

		public async Task Leave()
		{
			//return Manager.Api.fetch("DELETE", $"groups/{Info.Identifier}/members");
		}

		public async Task Invite(uint userId)
		{
			//return Manager.Api.fetch("POST", $"groups/{Info.Identifier}/invites/{userId}");
		}

		public async Task ReceiveNewInfo(object data)
		{
			//Manager.Groups.ReceiveUpdate(data);

			Updated?.Invoke(this);
		}

		public async Task AutomaticConsole(Action<Console> callback)
		{
			if (IsConsoleAutomatic)
			{
				Logger.Error("Can't enable automatic console twice");
				return;
			}

			Logger.Info($"Enabling automatic console for {Info.Name}");

			IsConsoleAutomatic = true;

			//await Servers.Refresh(true);
			//await Servers.RefreshStatus(true);

			HashSet<Console> connections = new HashSet<Console>();

			async void HandleStatus(Server server, ServerInfo oldInfo)
			{
				Logger.Info($"Received status from {server.Info.Name} - {server.IsOnline} - {server.Info.OnlinePing} - {server.Info.OnlinePlayers.Length}");

				if (server.IsOnline)
				{
					try
					{
						Console result = await server.GetConsole();

						if (connections.Add(result))
						{
							//result.Closed += (connection, data) => connections.Remove(connections);

							callback(result);
						}
						else
						{
							Logger.Info($"Console for {server.Info.Name} already exists.");
						}
					}
					catch
					{
						//Permission denied (if not, see console)
					}
				}
			}

			//Servers.Create += server =>
			//{
			//	server.StatusChanged += HandleStatus;
			//};

			//foreach (Server server in Servers.Items)
			//{
			//	HandleStatus(server, null);
			//}
		}
	}
}