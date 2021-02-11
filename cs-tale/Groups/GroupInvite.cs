namespace AltaClient.Groups
{
	public class GroupInvite
	{
		public GroupManager Manager { get; }
		public GroupInfo Info { get; }

		public GroupInvite(GroupManager manager, GroupInfo info)
		{
			Manager = manager;
			Info = info;
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
