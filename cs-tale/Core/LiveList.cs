using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Core
{
	public class LiveList<T>
	{
		public delegate Task<object[]> GetAll();
		public delegate void SubscribeCallback(object data);
		public delegate void Subscribe(SubscribeCallback callback);
		public delegate T Process(object data);


		public string Name { get; }

		public IReadOnlyCollection<T> Items => items.Values;

		public bool IsLive { get; private set; }

		public bool IsBlocked { get; private set; }

		public event Action<T> Create;
		public event Action<T> Update;
		public event Action<T> Delete;

		Dictionary<int, T> items = new Dictionary<int, T>();

		GetAll getAll;
		Subscribe create;
		Subscribe delete;
		Subscribe update;

		Func<T, int> getRawId;
		Func<T, int> getId;

		Process process;

		public LiveList(string name, GetAll getAll, Subscribe subscribeToCreate, Subscribe subscribeToDelete, Subscribe subscribeToUpdate, Func<T, int> getRawId, Func<T, int> getId, Process process)
		{
			Name = name;
			this.getAll = getAll;
			create = subscribeToCreate;
			delete = subscribeToDelete;
			update = subscribeToUpdate;

			this.getRawId = getRawId;
			this.getId = getId;
			this.process = process;
		}
	}
}
