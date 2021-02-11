using AltaClient.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AltaClient.Groups
{
	public class GroupManager
	{
		static Logger Logger { get; } = new Logger("GroupManager");

		public ApiConnection Api { get; }
		public SubscriptionManager Subscriptions { get; }
		public LiveList<Group> Groups { get; }
		public LiveList<GroupInvite> Invites { get; }
		public LiveList<GroupRequest> Requests { get; }

		public GroupManager(SubscriptionManager subscriptions)
		{
			Api = subscriptions.Api;
			Subscriptions = subscriptions;
			Groups = new LiveList<Group>("groups",
				async () => await this.Api.Fetch(HttpMethod.Get, "groups/joined") as JArray,
				callback => this.Subscriptions.Subscribe("me-group-create", this.Api.UserId, callback),
				callback => this.Subscriptions.Subscribe("me-group-delete", this.Api.UserId, callback),
				null,
				data => data.SelectToken(data.TryGetValue("group", out JToken group)  ? "group.id" : "id").ToObject<int>(),
				group => group.Info.Identifier,
 				data => new Group(this, data));

			Groups.Delete += group => group.Dispose();

			Invites = new LiveList<GroupInvite>("invites", () => this.Api.FetchArray(HttpMethod.Get, "groups/invites"), callback => this.Subscriptions.Subscribe("me-group-invite-create", this.Api.UserId, callback), callback => this.Subscriptions.Subscribe("me-group-invite-delete", this.Api.UserId, callback), null, data => data.SelectToken("id").ToObject<int>(), invite => invite.Info.Identifier, data => new GroupInvite(this, data.ToObject<GroupInfo>()));
			//Requests = new LiveList<GroupRequest>("requests", () => this.Api.fetch("GET", "groups/requests"), callback => this.Subscriptions.subscribe("me-group-request-create", this.Api.userId, callback), callback => this.Subscriptions.subscribe("me-group-request-delete", this.Api.userId, callback), undefined, data => data.id, invite => invite.info.id, data => new GroupRequest(this, data));
		}

		public async Task AcceptAllInvites(bool subscribe)
		{
			try
			{
				await Invites.Refresh(subscribe);

				if (subscribe)
				{
					this.Invites.Create += item => item.Accept();
				}

				foreach (GroupInvite invite in Invites.Items)
				{
					invite.Accept();
				}

				Logger.Info("Accepted all group invites");
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
		}

		public async Task AutomaticConsole(Action<ServerConnection> callback)
		{
			Logger.Info("Enabling automatic console for all groups");

			async Task HandleGroup(Group group)
			{
				await group.AutomaticConsole(callback);
			}

			Groups.Create += async group => await HandleGroup(group);

			foreach (Group group in Groups.Items)
			{
				await HandleGroup(group);
			}
		}
	}
}
