using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltaClient.Core
{
	public class LiveList<T>
		where T : class
	{
		static Logger Logger { get; } = new Logger("LiveList");

		public delegate Task<JArray> GetAll();
		public delegate Task Subscribe(Action<JObject> callback);
		public delegate T Process(JObject data);


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

		Func<JObject, int> getRawId;
		Func<T, int> getId;

		Process process;

		public LiveList(string name, GetAll getAll, Subscribe subscribeToCreate, Subscribe subscribeToDelete, Subscribe subscribeToUpdate, Func<JObject, int> getRawId, Func<T, int> getId, Process process)
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
		
		public T Get(int id)
		{
			if (items.TryGetValue(id, out T value))
			{
				return value;
			}

			return null;
		}

		public async Task<IEnumerable<T>> Refresh(bool isSubscribing)
		{
			if (IsLive || IsBlocked)
			{
				return Items;
			}

			if (isSubscribing)
			{
				IsLive = true;

				try
				{
					await create(ReceiveCreate);
					Logger.Info($"Subscribed to {Name} create");

					await delete(ReceiveDelete);
					Logger.Info($"Subscribed to {Name} delete");

					if (update != null)
					{
						await update(ReceiveUpdate);
						Logger.Info($"Subscribed to {Name} update");
					}
				}
				catch (Exception e)
				{
					//if (error.responseCode == 404)
					{
						Block();
					}
				}
			}

			JArray results;

			try
			{
				results = await getAll();

				if (results == null)
				{
					Logger.Info($"getAll returned null in {Name}");
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error getting items for LiveList");
				Logger.Info(e);

				results = null;

				Block();
			}

			List<int> toRemove = new List<int>();

			foreach (KeyValuePair<int, T> pair in items)
			{
				var id = pair.Key;
				var item = pair.Value;

				if (results != null && !results.Any(result => getRawId((JObject)result) == id))
				{
					toRemove.Add(id);

					Delete?.Invoke(item);
				}
			}
			
			foreach (int id in toRemove)
			{
				items.Remove(id);
			}

			if (results != null)
			{
				foreach (JToken result in results)
				{
					ReceiveCreateInternal((JObject)result);
				}
			}
        
	       return Items;
		}

		void Block()
		{
			if (!IsBlocked)
			{
				IsBlocked = true;
				Logger.Error("Not allowed to access " + Name);
			}
		}

		void ReceiveCreate(JObject data)
		{
			ReceiveCreateInternal((JObject)data.GetValue("content"));
		}
		
		void ReceiveCreateInternal(JObject data)
		{
			int id;

			try
			{
				id = getRawId(data);
			}
			catch (Exception e)
			{
				Logger.Error("Error in receive create");
				Logger.Error(e);

				throw e;
			}

			if (!items.ContainsKey(id))
			{
				T item = process(data);
				items[id] = item;

				Create?.Invoke(item);
			}
		}

		void ReceiveDelete(JObject data)
		{
			//TODO:
		}

		void ReceiveUpdate(JObject data)
		{
			//TODO:
		}
	}
}
