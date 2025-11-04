using System;
using System.Collections.Generic;
using System.Linq;

namespace PlazmaGames.Core.Network
{
	internal sealed class NetworkRequestEmitter
	{
		Dictionary<int, (List<(PacketReader, int)>, List<Action<PacketReader, int>>)> _events = new Dictionary<int, (List<(PacketReader, int)>, List<Action<PacketReader, int>>)>();

		public void Emit(int type, PacketReader val, int fromID)
		{
			if (!_events.ContainsKey(type)) return;
			_events[type].Item1.Add((val, fromID));
		}

		public void Subscribe(int type, Action<PacketReader, int> callback)
		{
			if (!_events.ContainsKey(type)) _events[type] = (new List<(PacketReader, int)>(), new List<Action<PacketReader, int>>());
			_events[type].Item2.Add(callback);
		}

		public void Unsubscribe(int type, Action<PacketReader, int> callback)
		{
			if (!_events.ContainsKey(type)) return;
			_events[type].Item2.Remove(callback);
		}

		public void CheckForRequest()
		{
			foreach ((List<(PacketReader, int)>, List<Action<PacketReader, int>>) data in _events.Values.ToList())
			{
				for (int i = data.Item1.Count - 1; i >= 0; i--)
				{
					foreach (Action<PacketReader, int> act in data.Item2) act.Invoke(new PacketReader(data.Item1[i].Item1.GetPacket()), data.Item1[i].Item2);
					data.Item1.RemoveAt(i);
				}
			}
		}
	}
}
