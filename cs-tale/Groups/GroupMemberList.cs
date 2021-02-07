using AltaClient.Core;
using System.Linq;

namespace AltaClient.Groups
{
	//public class GroupMemberList<T> : LiveList<T>
	//{
	//	public GroupMemberList(string name, GetAll getAll, Subscribe subscribeToCreate, Subscribe subscribeToDelete, Subscribe subscribeToUpdate, Process process) : base(name, getAll, subscribeToCreate, subscribeToDelete, subscribeToUpdate, data => data.user_id, item => item.userId, process)
	//	{

	//	}

	//	public T Find(string nameOrId)
	//	{
	//		if (int.TryParse(nameOrId, out int id))
	//		{
	//			return Get(id);
	//		}

	//		nameOrId = nameOrId.ToLower();

	//		return Items.FirstOrDefault(test => test.username.toLowerCase() == nameOrId);
	//	}
	//}
}