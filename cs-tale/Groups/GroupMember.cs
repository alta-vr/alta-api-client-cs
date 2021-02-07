namespace AltaClient.Groups
{
	public class GroupMember
	{
		public Group Group { get; }

		public GroupMember(Group group, object data)
		{
			Group = group;
		}
	}
}
