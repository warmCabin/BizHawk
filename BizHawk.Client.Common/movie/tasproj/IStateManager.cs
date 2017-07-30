using System;
using System.Collections.Generic;
using System.IO;

namespace BizHawk.Client.Common
{
	public interface IStateManager : IDisposable
	{
		byte[] this[int frame] { get; }

		TasStateManagerSettings Settings { get; set; }

		Action<int> InvalidateCallback { set; }

		void Capture(bool force = false);

		bool HasState(int frame);

		bool Invalidate(int frame);

		void Clear();

		void Save(BinaryWriter bw);

		void Load(BinaryReader bw);

		KeyValuePair<int, byte[]> GetStateClosestToFrame(int frame);

		bool Any();

		int Last { get; }

		void SetState(int frame, byte[] state);
	}
}
