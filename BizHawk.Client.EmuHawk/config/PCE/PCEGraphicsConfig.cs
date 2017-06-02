using System;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.PCEngine;

namespace BizHawk.Client.EmuHawk
{
	public partial class PCEGraphicsConfig : ConfigForm
	{
		[RequiredService]
		private PCEngine Pce { get; set; }

		public PCEGraphicsConfig()
		{
			InitializeComponent();
		}

		private void PCEGraphicsConfig_Load(object sender, EventArgs e)
		{
			PCEngine.PCESettings s = Pce.GetSettings();

			DispOBJ1.Checked = s.ShowOBJ1;
			DispBG1.Checked = s.ShowBG1;
			DispOBJ2.Checked = s.ShowOBJ2;
			DispBG2.Checked = s.ShowBG2;
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			PCEngine.PCESettings s = Pce.GetSettings();
			s.ShowOBJ1 = DispOBJ1.Checked;
			s.ShowBG1 = DispBG1.Checked;
			s.ShowOBJ2 = DispOBJ2.Checked;
			s.ShowBG2 = DispBG2.Checked;
			Pce.PutSettings(s);
			Close();
		}
	}
}
