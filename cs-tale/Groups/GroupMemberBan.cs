namespace AltaClient.Groups
{
	public class GroupMemberBan
	{
		public Group Group { get; }

		public GroupMemberBan(Group group, object data)
		{
			Group = group;
		}

		public void Revoke()
		{
			//TODO:
		}
	}
}
