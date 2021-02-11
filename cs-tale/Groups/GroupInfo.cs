using Newtonsoft.Json;
using System;

namespace AltaClient.Groups
{
	public class GroupRole
	{
		public int Identifier { get; private set; }
		public string Name { get; private set; }
		public string Color { get; private set; }
		public string[] Permissions { get; private set; }
	}

	public class GroupInfo
	{
		[JsonProperty("id")]
		public int Identifier { get; private set; }

		[JsonProperty("name")]
		public string Name { get; private set; }

		[JsonProperty("description")]
		public string Description { get; private set; }

		[JsonProperty("member_count")]
		public int MemberCount { get; private set; }

		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; private set; }

		[JsonProperty("type")]
		public string Type { get; private set; }

		[JsonProperty("tags")]
		public string[] Tags { get; private set; }

		[JsonProperty("roles")]
		public GroupRole[] Roles { get; private set; }

		[JsonProperty("allowed_server_count")]
		public int AllowedServersCount { get; private set; }

		[JsonProperty("servers")]
		public ServerInfo[] Servers { get; private set; }
	}
}