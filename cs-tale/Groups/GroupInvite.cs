namespace AltaClient.Groups
{
	public class GroupInvite
	{
		public Group Group { get; }

		public GroupInvite(Group group, object data)
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
