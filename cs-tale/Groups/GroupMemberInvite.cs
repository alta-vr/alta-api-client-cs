namespace AltaClient.Groups
{
	public class GroupMemberInvite
	{
		public Group Group { get; }

		public GroupMemberInvite(Group group, object data)
		{
			Group = group;
		}

		public void Revoke()
		{
			//TODO:
		}
	}
}