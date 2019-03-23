using System;

using BizHawk.Common.BufferExtensions;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Components.M6502;

using System.Runtime.InteropServices;

namespace BizHawk.Emulation.Cores.Computers.VIC20
{
	[Core(
		"VIC20Hawk",
		"",
		isPorted: false,
		isReleased: true)]
	[ServiceNotApplicable(typeof(IDriveLight))]
	public partial class VIC20Hawk : IEmulator, ISaveRam, IDebuggable, IStatable, IInputPollable, IRegionable, 
	ISettable<VIC20Hawk.VIC20Settings, VIC20Hawk.VIC20SyncSettings>
	{
		// declaractions
		// put top level core variables here
		// including things like RAM and BIOS
		// they will be used in the hex editor and others

		// the following declaraion is only an example
		// see memoryDomains.cs to see how it is used to define a Memory Domain that you can see in Hex editor
		// ex:
		public byte[] RAM = new byte[0x8000];


		public byte[] _bios;
		public readonly byte[] _rom;	
		
		// sometimes roms will have a header
		// the following is only an example in order to demonstrate how to extract the header
		public readonly byte[] header = new byte[0x50];

		public byte[] cart_RAM;
		public bool has_bat;

		private int _frame = 0;

		public MapperBase mapper;

		private readonly ITraceable _tracer;

		public MOS6502X<CpuLink> cpu;
		public PPU ppu;
		public Audio audio;
		public SerialPort serialport;

		private static byte[] GBA_override = { 0xFF, 0x00, 0xCD, 0x03, 0x35, 0xAA, 0x31, 0x90, 0x94, 0x00, 0x00, 0x00, 0x00 };

		[CoreConstructor("VIC20")]
		public VIC20Hawk(CoreComm comm, GameInfo game, byte[] rom, /*string gameDbFn,*/ object settings, object syncSettings)
		{
			var ser = new BasicServiceProvider(this);

			cpu = new MOS6502X<CpuLink>(new CpuLink(this))
			{
				BCD_Enabled = true
			};

			audio = new Audio();
			ppu = new PPU();
			serialport = new SerialPort();

			CoreComm = comm;

			_settings = (VIC20Settings)settings ?? new VIC20Settings();
			_syncSettings = (VIC20SyncSettings)syncSettings ?? new VIC20SyncSettings();
			_controllerDeck = new VIC20HawkControllerDeck(_syncSettings.Port1);

			// BIOS stuff can be tricky. Sometimes you'll have more then one vailable BIOS or different BIOSes for different regions
			// for now I suggest just picking one going
			byte[] Bios = null;
			//Bios = comm.CoreFileProvider.GetFirmware("VIC20", "Bios", true, "BIOS Not Found, Cannot Load");			
			_bios = Bios;

			// the following few lines are jsut examples of working with a header and hashes
			Buffer.BlockCopy(rom, 0x100, header, 0, 0x50);
			string hash_md5 = null;
			hash_md5 = "md5:" + rom.HashMD5(0, rom.Length);
			Console.WriteLine(hash_md5);

			// in this case our working ROm has the header removed (might not be the case for your system)
			_rom = rom;
			Setup_Mapper();

			_frameHz = 60;

			// usually you want to have a reflected core available to the various components since they share some information
			audio.Core = this;
			ppu.Core = this;
			serialport.Core = this;

			// the following is just interface setup, dont worry to much about it
			ser.Register<IVideoProvider>(this);
			ser.Register<ISoundProvider>(audio);
			ServiceProvider = ser;

			_settings = (VIC20Settings)settings ?? new VIC20Settings();
			_syncSettings = (VIC20SyncSettings)syncSettings ?? new VIC20SyncSettings();

			_tracer = new TraceBuffer { Header = cpu.TraceHeader };
			ser.Register<ITraceable>(_tracer);

			SetupMemoryDomains();
			HardReset();
		}

		public DisplayType Region => DisplayType.NTSC;

		private readonly VIC20HawkControllerDeck _controllerDeck;

		public void HardReset()
		{
			Register_Reset();
			ppu.Reset();
			audio.Reset();
			serialport.Reset();

			_vidbuffer = new int[VirtualWidth * VirtualHeight];
		}

		private void ExecFetch(ushort addr)
		{
			MemoryCallbacks.CallExecutes(addr, "System Bus");
		}

		// most systems have cartridges or other storage media that map memory in more then one way.
		// Use this ethod to set that stuff up when first starting the core
		private void Setup_Mapper()
		{
			mapper = new MapperDefault();
		}
	}
}
