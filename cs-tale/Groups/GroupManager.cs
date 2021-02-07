using AltaClient.Core;
using System;
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
			//Groups = new LiveList<Group>("groups",
			//	() => this.Api.Fetch("GET", "groups/joined"),
			//	callback => this.Subscriptions.Subscribe("me-group-create", this.Api.userId, callback),
			//	callback => this.Subscriptions.Subscribe("me-group-delete", this.Api.userId, callback),
			//	null,
			//	data => !!data.group ? data.group.id : data.id,
			//	group => group.info.id,
 		//		data => !!data.Group ? new Group(this, data.Group, data.Member) : new Group(this, data));

			Groups.Delete += group => group.Dispose();

			//Invites = new LiveList<GroupInvite>("invites", () => this.Api.fetch("GET", "groups/invites"), callback => this.Subscriptions.subscribe("me-group-invite-create", this.Api.userId, callback), callback => this.Subscriptions.subscribe("me-group-invite-delete", this.Api.userId, callback), undefined, data => data.id, invite => invite.info.id, data => new GroupInvite(this, data));
			//Requests = new LiveList<GroupRequest>("requests", () => this.Api.fetch("GET", "groups/requests"), callback => this.Subscriptions.subscribe("me-group-request-create", this.Api.userId, callback), callback => this.Subscriptions.subscribe("me-group-request-delete", this.Api.userId, callback), undefined, data => data.id, invite => invite.info.id, data => new GroupRequest(this, data));
		}

		public async Task AcceptAllInvites(bool subscribe)
		{
			try
			{
				//await Invites.Refresh(subscribe);

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

		public async Task AutomaticConsole(Action<Console> callback)
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
