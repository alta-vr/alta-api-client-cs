namespace AltaClient.Groups
{
	public class GroupRequest
	{
		public Group Group { get; }

		public GroupRequest(Group group, object data)
		{
			Group = group;
		}

		public void Revoke()
		{
			//TODO:
		}
	}
}
