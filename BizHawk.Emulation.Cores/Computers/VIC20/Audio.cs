using System;

using BizHawk.Common;
using BizHawk.Common.BufferExtensions;
using BizHawk.Emulation.Common;
using BizHawk.Common.NumberExtensions;

namespace BizHawk.Emulation.Cores.Computers.VIC20
{
	// Audio Emulation goes here
	// don't worry too much about the blipbuffers for now, sound will be one of the last things you do.
	// if you are interested see GBHawk for a reference, but the most important thing is to properly disppose of them
	public class Audio : ISoundProvider
	{
		public VIC20Hawk Core { get; set; }

		private BlipBuffer _blip_L = new BlipBuffer(15000);
		private BlipBuffer _blip_R = new BlipBuffer(15000);

		public uint master_audio_clock;

		public byte ReadReg(int addr)
		{
			byte ret = 0;

			return ret;
		}

		public void WriteReg(int addr, byte value)
		{

		}

		public void tick()
		{

		}

		public void Reset()
		{
			_blip_L.SetRates(4194304, 44100);
			_blip_R.SetRates(4194304, 44100);
		}

		public void SyncState(Serializer ser)
		{

		}

		#region audio

		public bool CanProvideAsync => false;

		public void SetSyncMode(SyncSoundMode mode)
		{
			if (mode != SyncSoundMode.Sync)
			{
				throw new InvalidOperationException("Only Sync mode is supported_");
			}
		}

		public SyncSoundMode SyncMode => SyncSoundMode.Sync;

		public void GetSamplesSync(out short[] samples, out int nsamp)
		{
			_blip_L.EndFrame(master_audio_clock);
			_blip_R.EndFrame(master_audio_clock);
			
			nsamp = _blip_L.SamplesAvailable();

			// only for running without errors, remove this line once you get audio
			nsamp = 1;

			samples = new short[nsamp * 2];

			// uncomment these once you have audio to play
			//_blip_L.ReadSamplesLeft(samples, nsamp);
			//_blip_R.ReadSamplesRight(samples, nsamp);

			master_audio_clock = 0;
		}

		public void GetSamplesAsync(short[] samples)
		{
			throw new NotSupportedException("Async is not available");
		}

		public void DiscardSamples()
		{
			_blip_L.Clear();
			_blip_R.Clear();
			master_audio_clock = 0;
		}

		private void GetSamples(short[] samples)
		{

		}

		public void DisposeSound()
		{
			_blip_L.Clear();
			_blip_R.Clear();
			_blip_L.Dispose();
			_blip_R.Dispose();
			_blip_L = null;
			_blip_R = null;
		}

		#endregion
	}
}