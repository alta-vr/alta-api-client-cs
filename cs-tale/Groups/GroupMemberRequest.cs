namespace AltaClient.Groups
{
	public class GroupMemberRequest
	{
		public Group Group { get; }

		public GroupMemberRequest(Group group, object data)
		{
			Group = group;
		}

		public void Accept()
		{
			//TODO:
		}

		public void Reject()
		{
			//TODO:
		}
	}
}
