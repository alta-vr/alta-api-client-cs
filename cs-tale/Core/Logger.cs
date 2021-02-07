using System;
using System.Collections.Generic;
using System.Text;

namespace AltaClient.Core
{
	public class Logger
	{
		//TODO: Hook into some official logging system

		public string Name { get; }

		public Logger(string name)
		{
			Name = name;
		}

		public string Format(object message)
		{
			if (message is string)
			{
				return $"{DateTime.Now.ToString()} - [{Name}] {message}";
			}

			if (message == null)
			{
				return $"{DateTime.Now.ToString()} - [{Name}] Null";
			}

			//TODO: Some JSON? idk

			return $"{DateTime.Now.ToString()} - [{Name}] Null";
		}

		public void Trace(object message)
		{
			Console.WriteLine(message);
		}

		public void Info(object message)
		{
			Console.WriteLine(message);
		}

		public void Warn(object message)
		{
			Console.WriteLine(message);
		}

		public void Error(object message)
		{
			Console.WriteLine(message);
		}

		public void Fatal(object message)
		{
			Console.WriteLine(message);
		}

		public void Success(object message)
		{
			Console.WriteLine(message);
		}
	}
}
