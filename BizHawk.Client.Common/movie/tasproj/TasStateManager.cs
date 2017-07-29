using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Common.IEmulatorExtensions;

namespace BizHawk.Client.Common
{
	/// <summary>
	/// Captures savestates and manages the logic of adding, retrieving, 
	/// invalidating/clearing of states.  Also does memory management and limiting of states
	/// </summary>
	public class TasStateManager : IDisposable
	{
		public TasStateManager(TasMovie movie)
		{
			_movie = movie;

			Settings = new TasStateManagerSettings(Global.Config.DefaultTasProjSettings);

			_accessed = new List<StateManagerState>();

			if (_movie.StartsFromSavestate)
			{
				SetState(0, _movie.BinarySavestate);
			}

			MountWriteAccess();
		}

		#region API

		public void Dispose()
		{
			// States and BranchStates don't need cleaning because they would only contain an ndbdatabase entry which was demolished by the below
			NdbDatabase?.Dispose();
		}

		/// <summary>
		/// Retrieves the savestate for the given frame,
		/// If this frame does not have a state currently, will return an empty array
		/// </summary>
		/// <returns>A savestate for the given frame or an empty array if there isn't one</returns>
		public byte[] this[int frame]
		{
			get
			{
				if (frame == 0)
				{
					return InitialState;
				}

				if (_states.ContainsKey(frame))
				{
					StateAccessed(frame);
					return _states[frame].State;
				}

				return new byte[0];
			}
		}

		public TasStateManagerSettings Settings
		{
			get
			{
				return _settings;
			}

			set
			{
				_settings = value;
				if (Any())
				{
					LimitStateCount();
				}
			}
		}

		public Action<int> InvalidateCallback { private get; set; }

		/// <summary>
		/// Requests that the current emulator state be captured 
		/// Unless force is true, the state may or may not be captured depending on the logic employed by "greenzone" management
		/// </summary>
		public void Capture(bool force = false)
		{
			bool shouldCapture;

			int frame = Global.Emulator.Frame;
			if (_movie.StartsFromSavestate && frame == 0) // Never capture frame 0 on savestate anchored movies since we have it anyway
			{
				shouldCapture = false;
			}
			else if (force)
			{
				shouldCapture = force;
			}
			else if (frame == 0) // For now, long term, TasMovie should have a .StartState property, and a tasproj file for the start state in non-savestate anchored movies
			{
				shouldCapture = true;
			}
			else if (_movie.Markers.IsMarker(frame + 1))
			{
				shouldCapture = true; // Markers shoudl always get priority
			}
			else
			{
				shouldCapture = frame % StateFrequency == 0;
			}

			if (shouldCapture)
			{
				SetState(frame, (byte[])Core.SaveStateBinary().Clone(), skipRemoval: false);
			}
		}

		public bool HasState(int frame)
		{
			if (_movie.StartsFromSavestate && frame == 0)
			{
				return true;
			}

			return _states.ContainsKey(frame);
		}

		/// <summary>
		/// Clears out all savestates after the given frame number
		/// </summary>
		public bool Invalidate(int frame)
		{
			bool anyInvalidated = false;

			if (Any())
			{
				if (!_movie.StartsFromSavestate && frame == 0) // Never invalidate frame 0 on a non-savestate-anchored movie
				{
					frame = 1;
				}

				var statesToRemove = _states.Where(s => s.Key >= frame).ToList();

				anyInvalidated = statesToRemove.Any();

				foreach (var state in statesToRemove)
				{
					RemoveState(state.Key);
				} 

				CallInvalidateCallback(frame);
			}

			return anyInvalidated;
		}

		/// <summary>
		/// Clears all state information (except for the initial state which is by design immutable)
		/// </summary>
		public void Clear()
		{
			if (_states.Any())
			{
				StateManagerState power = _states.Values.First(s => s.Frame == 0);
				StateAccessed(power.Frame);

				_states.Clear();
				_accessed.Clear();

				SetState(0, power.State);
				Used = (ulong)power.State.Length;

				ClearDiskStates();
			}
		}

		public void Save(BinaryWriter bw)
		{
			List<int> noSave = ExcludeStates();

			bw.Write(_states.Count - noSave.Count);
			for (int i = 0; i < _states.Count; i++)
			{
				if (noSave.Contains(i))
				{
					continue;
				}

				StateAccessed(_states.ElementAt(i).Key);
				KeyValuePair<int, StateManagerState> kvp = _states.ElementAt(i);
				bw.Write(kvp.Key);
				bw.Write(kvp.Value.Length);
				bw.Write(kvp.Value.State);
				////_movie.ReportProgress(100d / States.Count * i);
			}
		}

		public void Load(BinaryReader br)
		{
			_states.Clear();
			try
			{
				int nstates = br.ReadInt32();
				for (int i = 0; i < nstates; i++)
				{
					int frame = br.ReadInt32();
					int len = br.ReadInt32();
					byte[] data = br.ReadBytes(len);

					// whether we should allow state removal check here is an interesting question
					// nothing was edited yet, so it might make sense to show the project untouched first
					SetState(frame, data);
					////States.Add(frame, data);
					////Used += len;
				}
			}
			catch (EndOfStreamException)
			{
			}
		}

		public KeyValuePair<int, byte[]> GetStateClosestToFrame(int frame)
		{
			var s = _states.LastOrDefault(state => state.Key < frame);

			if (s.Key == 0)
			{
				return new KeyValuePair<int, byte[]>(0, InitialState);
			}

			return new KeyValuePair<int, byte[]>(s.Key, this[s.Key]);
		}

		public bool Any()
		{
			if (_movie.StartsFromSavestate)
			{
				return _states.Count > 0;
			}

			return _states.Count > 1;
		}

		public int Last
		{
			get
			{
				if (Any())
				{
					return _states.Last().Key;
				}

				return 0;
			}
		}

		// Used when loading a branch, that branch's state must be loaded and subsequently affect the greenzone
		// TODO: this logic is probably wrong after removing branch states, I think the correct logic would involve invalidating states relevant to the old branch
		public void SetState(int frame, byte[] state, bool skipRemoval = true)
		{
			if (!skipRemoval) // skipRemoval: false only when capturing new states
			{
				MaybeRemoveStates(); // Remove before adding so this state won't be removed.
			}

			Used += (ulong)state.Length;
			if (_states.ContainsKey(frame))
			{
				_states[frame].State = state;
			}
			else
			{
				_states.Add(frame, new StateManagerState(this, state, frame));
			}

			StateAccessed(frame);

			int i = _states.IndexOfKey(frame);
			if (i > 0 && AllLag(_states.Keys[i - 1], _states.Keys[i]))
			{
				_lowPriorityStates.Add(_states[frame]);
			}
		}

		internal NDBDatabase NdbDatabase { get; private set; } // TODO: internal so StateManagerState can access it, find a way to pass something in intead and lock this down.  Nothing else should use this

		#endregion

		private TasStateManagerSettings _settings;

		// Deletes/moves states to follow the state storage size limits.
		// Used after changing the settings.
		private void LimitStateCount()
		{
			while (Used + DiskUsed > Settings.CapTotal)
			{
				int frame = StateToRemove();
				RemoveState(frame);
			}

			int index = -1;
			while (DiskUsed > (ulong)Settings.DiskCapacitymb * 1024uL * 1024uL)
			{
				do
				{
					index++;
				}
				while (!_accessed[index].IsOnDisk);

				_accessed[index].MoveToRAM();
			}

			if (Used > Settings.Cap)
			{
				MaybeRemoveStates();
			}
		}

		private byte[] InitialState
		{
			get
			{
				if (_movie.StartsFromSavestate)
				{
					return _movie.BinarySavestate;
				}

				return _states[0].State;
			}
		}

		// TODO: pass this in, and find a solution to a stale reference (this is instantiated BEFORE a new core instance is made, making this one stale if it is simply set in the constructor
		private IStatable Core => Global.Emulator.AsStatable();

		private void CallInvalidateCallback(int index)
		{
			InvalidateCallback?.Invoke(index);
		}

		private readonly List<StateManagerState> _lowPriorityStates = new List<StateManagerState>();
		
		private Guid _guid = Guid.NewGuid();
		private SortedList<int, StateManagerState> _states = new SortedList<int, StateManagerState>();

		private string StatePath
		{
			get
			{
				var basePath = PathManager.MakeAbsolutePath(Global.Config.PathEntries["Global", "TAStudio states"].Path, null);
				return Path.Combine(basePath, _guid.ToString());
			}
		}

		private bool _isMountedForWrite;
		private readonly TasMovie _movie;
		private ulong _expectedStateSize;

		private readonly int _minFrequency = VersionInfo.DeveloperBuild ? 2 : 1;
		private const int MaxFrequency = 16;

		private int StateFrequency
		{
			get
			{
				int freq = (int)(_expectedStateSize / 65536);

				if (freq < _minFrequency)
				{
					return _minFrequency;
				}

				if (freq > MaxFrequency)
				{
					return MaxFrequency;
				}

				return freq;
			}
		}

		private int MaxStates => (int)(Settings.Cap / _expectedStateSize) + (int)((ulong)Settings.DiskCapacitymb * 1024 * 1024 / _expectedStateSize);

		private int StateGap => 1 << Settings.StateGap;

		/// <summary>
		/// Mounts this instance for write access. Prior to that it's read-only
		/// </summary>
		private void MountWriteAccess()
		{
			if (_isMountedForWrite)
			{
				return;
			}

			_isMountedForWrite = true;

			int limit = 0;

			_expectedStateSize = (ulong)Core.SaveStateBinary().Length;

			if (_expectedStateSize > 0)
			{
				limit = MaxStates;
			}

			_states = new SortedList<int, StateManagerState>(limit);

			if (_expectedStateSize > int.MaxValue)
			{
				throw new InvalidOperationException();
			}

			NdbDatabase = new NDBDatabase(StatePath, Settings.DiskCapacitymb * 1024 * 1024, (int)_expectedStateSize);
		}

		private readonly List<StateManagerState> _accessed;

		private void MaybeRemoveStates()
		{
			// Loop, because removing a state that has a duplicate won't save any space
			 while (Used + _expectedStateSize > Settings.Cap || DiskUsed > (ulong)Settings.DiskCapacitymb * 1024 * 1024)
			{
				int shouldRemove = StateToRemove();
				RemoveState(shouldRemove);
			}

			if (Used > Settings.Cap)
			{
				int lastMemState = -1;
				do
				{
					lastMemState++;
				}
				while (_states[_accessed[lastMemState].Frame] == null);
				MoveStateToDisk(_accessed[lastMemState].Frame);
			}
		}

		/// <summary>
		/// returns the frame of the state
		/// </summary>
		private int StateToRemove()
		{
			// shouldRemove is frame
			int shouldRemove = -1;

			int i = 0;
			int markerSkips = MaxStates / 2;

			// lowPrioritySates (e.g. states with only lag frames between them)
			do
			{
				if (_lowPriorityStates.Count > i)
				{
					if (_states.ContainsKey(_lowPriorityStates[i].Frame))
					{
						shouldRemove = _lowPriorityStates[i].Frame;
					}
				}
				else
				{
					break;
				}

				// Keep marker states
				markerSkips--;
				if (markerSkips < 0)
				{
					shouldRemove = -1;
				}

				i++;
			}
			while ((StateIsMarker(shouldRemove) && markerSkips > -1) || shouldRemove == 0);

			// by last accessed
			markerSkips = MaxStates / 2;
			if (shouldRemove < 1)
			{
				i = 0;
				do
				{
					if (_accessed.Count > i)
					{
						if (_states.ContainsKey(_accessed[i].Frame))
						{
							shouldRemove = _accessed[i].Frame;
						}
					}
					else
					{
						break;
					}

					// Keep marker states
					markerSkips--;
					if (markerSkips < 0)
					{
						shouldRemove = -1;
					}

					i++;
				}
				while ((StateIsMarker(shouldRemove) && markerSkips > -1) || shouldRemove == 0);
			}

			if (shouldRemove < 1) // only found marker states above
			{
				StateManagerState s = _states.Values[1];
				shouldRemove = s.Frame;
			}

			return shouldRemove;
		}

		private bool StateIsMarker(int frame)
		{
			if (frame == -1) // TODO: never pass in -1, just don't call this
			{
				return false;
			}

			return _movie.Markers.IsMarker(_states[frame].Frame + 1);
		}

		private bool AllLag(int from, int upTo)
		{
			if (upTo >= Global.Emulator.Frame)
			{
				upTo = Global.Emulator.Frame - 1;
				if (!Global.Emulator.AsInputPollable().IsLagFrame)
				{
					return false;
				}
			}

			for (int i = from; i < upTo; i++)
			{
				if (_movie[i].Lagged == false)
				{
					return false;
				}
			}

			return true;
		}

		private void MoveStateToDisk(int index)
		{
			Used -= (ulong)_states[index].Length;
			_states[index].MoveToDisk();
		}

		private void MoveStateToMemory(int index)
		{
			_states[index].MoveToRAM();
			Used += (ulong)_states[index].Length;
		}

		private void RemoveState(int frame)
		{
			_accessed.Remove(_states[frame]);

			StateManagerState state;
			state = _states[frame];
			if (_states[frame].IsOnDisk)
			{
				_states[frame].Dispose();
			}
			else
			{
				Used -= (ulong)_states[frame].Length;
			}

			_states.RemoveAt(_states.IndexOfKey(frame));

			_lowPriorityStates.Remove(state);
		}

		private void StateAccessed(int frame)
		{
			if (frame == 0 && _movie.StartsFromSavestate)
			{
				return;
			}

			StateManagerState state = _states[frame];
			bool removed = _accessed.Remove(state);
			_accessed.Add(state);

			if (_states[frame].IsOnDisk)
			{
				if (!_states[_accessed[0].Frame].IsOnDisk)
				{
					MoveStateToDisk(_accessed[0].Frame);
				}

				MoveStateToMemory(frame);
			}

			if (!removed && _accessed.Count > MaxStates)
			{
				_accessed.RemoveAt(0);
			}
		}

		private void ClearDiskStates()
		{
			NdbDatabase?.Clear();
		}

		private List<int> ExcludeStates()
		{
			List<int> ret = new List<int>();
			ulong saveUsed = Used + DiskUsed;

			// respect state gap no matter how small the resulting size will be
			// still leave marker states
			for (int i = 1; i < _states.Count; i++)
			{
				if (_movie.Markers.IsMarker(_states.ElementAt(i).Key + 1)
					|| _states.ElementAt(i).Key % StateGap == 0)
				{
					continue;
				}

				ret.Add(i);

				if (_states.ElementAt(i).Value.IsOnDisk)
				{
					saveUsed -= _expectedStateSize;
				}
				else
				{
					saveUsed -= (ulong)_states.ElementAt(i).Value.Length;
				}
			}

			// if the size is still too big, exclude states form the beginning
			// still leave marker states
			int index = 0;
			while (saveUsed > (ulong)Settings.DiskSaveCapacitymb * 1024 * 1024)
			{
				do
				{
					index++;
					if (index >= _states.Count)
					{
						break;
					}
				}
				while (_movie.Markers.IsMarker(_states.ElementAt(index).Key + 1));

				if (index >= _states.Count)
				{
					break;
				}

				ret.Add(index);

				if (_states.ElementAt(index).Value.IsOnDisk)
				{
					saveUsed -= _expectedStateSize;
				}
				else
				{
					saveUsed -= (ulong)_states.ElementAt(index).Value.Length;
				}
			}

			// if there are enough markers to still be over the limit, remove marker frames
			index = 0;
			while (saveUsed > (ulong)Settings.DiskSaveCapacitymb * 1024 * 1024)
			{
				index++;
				if (!ret.Contains(index))
				{
					ret.Add(index);
				}

				if (_states.ElementAt(index).Value.IsOnDisk)
				{
					saveUsed -= _expectedStateSize;
				}
				else
				{
					saveUsed -= (ulong)_states.ElementAt(index).Value.Length;
				}
			}

			return ret;
		}

		// Map:
		// 4 bytes - total savestate count
		// [Foreach state]
		// 4 bytes - frame
		// 4 bytes - length of savestate
		// 0 - n savestate
		private ulong _used;
		private ulong Used
		{
			get
			{
				return _used;
			}

			set
			{
				// TODO: Shouldn't we throw an exception? Debug.Fail only runs in debug mode?
				if (value > 0xf000000000000000)
				{
					System.Diagnostics.Debug.Fail("ulong Used underfow!");
				}
				else
				{
					_used = value;
				}
			}
		}

		private ulong DiskUsed
		{
			get
			{
				if (NdbDatabase == null)
				{
					return 0;
				}

				return (ulong)NdbDatabase.Consumed;
			}
		}
	}
}
