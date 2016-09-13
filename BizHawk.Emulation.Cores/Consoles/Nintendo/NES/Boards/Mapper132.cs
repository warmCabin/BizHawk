﻿using BizHawk.Common;

namespace BizHawk.Emulation.Cores.Nintendo.NES
{
	// Creatom
	// specs pulled from Nintendulator sources
	public sealed class Mapper132 : NES.NESBoardBase
	{
		private ByteBuffer reg = new ByteBuffer(4);

		//configuraton
		int prg_mask, chr_mask;
		//state
		int prg, chr;

		bool is173;

		public override bool Configure(NES.EDetectionOrigin origin)
		{
			switch (Cart.board_type)
			{
				case "MAPPER132":
				case "UNIF_UNL-22211":
					break;
				case "MAPPER173":
					is173 = true;
					break;
				default:
					return false;
			}

			prg_mask = Cart.prg_size / 32 - 1;
			chr_mask = Cart.chr_size / 8 - 1;
			SetMirrorType(Cart.pad_h, Cart.pad_v);
			return true;
		}

		public void sync()
		{

				prg=reg[2]>>2;
				prg &= prg_mask;
				chr = (reg[2] & 0x3);
				chr &= chr_mask;


		}

		public override void WritePRG(int addr, byte value)
		{
			sync();
		}

		public override void WriteEXP(int addr, byte value)
		{
			if (addr <= 0x103 && addr >= 0x100)
				reg[addr&0x03] = (byte)(value & 0x0f);

		}

		public override byte ReadEXP(int addr)
		{

			if ((addr & 0x100) != 0)
				return (byte)((NES.DB & (is173 ? 0x01 : 0xf0)) | reg[2]);
			else if ((addr & 0x1000) == 0)
				return NES.DB;
			else
				return 0xff;
		}

		public override byte ReadPRG(int addr)
		{
			return ROM[addr + (prg << 15)];
		}

		public override byte ReadPPU(int addr)
		{
			if (addr < 0x2000)
			{
				return VROM[addr + (chr << 13)];
			}
			else return base.ReadPPU(addr);
		}

		public override void SyncState(Serializer ser)
		{
			base.SyncState(ser);
			ser.Sync("chr", ref chr);
			ser.Sync("prg", ref prg);
			ser.Sync("is173", ref is173);
			ser.Sync("reg", ref reg);
		}

	}
}
