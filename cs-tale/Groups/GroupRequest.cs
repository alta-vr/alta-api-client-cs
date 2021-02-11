namespace AltaClient.Groups
{
	public class GroupRequest
	{
		public GroupManager Manager { get; }
		public GroupInfo Info { get; }

		public GroupRequest(GroupManager manager, GroupInfo info)
		{
			Manager = manager;
			Info = info;
		}

		public void Revoke()
		{
			//TODO:
		}
	}
}
