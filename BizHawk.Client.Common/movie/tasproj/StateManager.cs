using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.Common
{
	public class StateManager : IStateManager
	{
		private readonly TasMovie _movie;

		public StateManager(TasMovie movie)
		{
			_movie = movie;
		}

		public void Dispose()
		{
			// Nothign to do for now
		}

		public byte[] this[int frame]
		{
			get
			{
				return new byte[0];
			}
		}

		public TasStateManagerSettings Settings { get; set; }

		public Action<int> InvalidateCallback { private get; set; }

		public void Capture(bool force = false)
		{

		}

		public bool Invalidate(int frame)
		{
			return false;
		}

		public bool HasState(int frame)
		{
			return false;
		}

		public void Clear()
		{

		}

		public void Save(BinaryWriter bw)
		{
			// save a path to the file?
		}

		public void Load(BinaryReader br)
		{
			// load a path from the file?
		}

		public KeyValuePair<int, byte[]> GetStateClosestToFrame(int frame)
		{
			return new KeyValuePair<int, byte[]>(0, new byte[0]);
		}

		public bool Any()
		{
			return false;
		}

		public int Last => 0;

		public void SetState(int frame, byte[] state)
		{

		}
	}
}
