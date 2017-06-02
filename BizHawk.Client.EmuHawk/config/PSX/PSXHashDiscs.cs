using System;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.DiscSystem;
using BizHawk.Emulation.Cores.Sony.PSX;

namespace BizHawk.Client.EmuHawk
{
	public partial class PSXHashDiscs : ConfigForm
	{
		[RequiredService]
		private Octoshock Psx { get; set; }

		public PSXHashDiscs()
		{
			InitializeComponent();
		}

		private void BtnHash_Click(object sender, EventArgs e)
		{
			txtHashes.Text = "";
			btnHash.Enabled = false;
			try
			{
				foreach (var disc in Psx.Discs)
				{
					DiscHasher hasher = new DiscHasher(disc);
					uint hash = hasher.Calculate_PSX_RedumpHash();
					txtHashes.Text += $"{hash:X8} {disc.Name}\r\n";
				}
			}
			finally
			{
				btnHash.Enabled = true;
			}
		}
	}
}
