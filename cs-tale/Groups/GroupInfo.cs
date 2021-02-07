using System;

namespace AltaClient.Groups
{
	public class GroupRole
	{
		public int Identifier { get; }
		public string Name { get; }
		public string Color { get; }
		public string[] Permissions { get; }
	}

	public class GroupInfo
	{
		public int Identifier { get; }
		public string Name { get; }

		public string Description { get; }
		public int MemberCount { get; }
		public DateTime CreatedAt { get; }
		public string Type { get; }
		public string[] Tags { get; }
		public GroupRole[] Roles { get; }
		public int AllowedServersCount { get; }
		public Server[] Servers { get; }
	}
}