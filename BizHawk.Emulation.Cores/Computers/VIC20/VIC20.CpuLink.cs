using BizHawk.Emulation.Cores.Components.M6502;

namespace BizHawk.Emulation.Cores.Computers.VIC20
{
    public partial class VIC20Hawk
	{
		public struct CpuLink : IMOS6502XLink
		{
			private readonly VIC20Hawk _vic20;

			public CpuLink(VIC20Hawk vic20)
			{
				_vic20 = vic20;
			}

			public byte DummyReadMemory(ushort address) => _vic20.ReadMemory(address);

			public void OnExecFetch(ushort address) => _vic20.ExecFetch(address);

			public byte PeekMemory(ushort address) => _vic20.CDL == null ? _vic20.PeekMemory(address) : _vic20.FetchMemory_CDL(address);

			public byte ReadMemory(ushort address) => _vic20.CDL == null ? _vic20.ReadMemory(address) : _vic20.ReadMemory_CDL(address);

			public void WriteMemory(ushort address, byte value) => _vic20.WriteMemory(address, value);
		}
	}
}
