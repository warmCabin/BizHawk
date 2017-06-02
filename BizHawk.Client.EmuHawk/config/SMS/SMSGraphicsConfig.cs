using System;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Sega.MasterSystem;

namespace BizHawk.Client.EmuHawk
{
	public partial class SMSGraphicsConfig : ConfigForm
	{
		[RequiredService]
		private SMS Sms { get; set; }

		public SMSGraphicsConfig()
		{
			InitializeComponent();
		}

		private void SMSGraphicsConfig_Load(object sender, EventArgs e)
		{
			var s = Sms.GetSettings();
			DispOBJ.Checked = s.DispOBJ;
			DispBG.Checked = s.DispBG;
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			var s = Sms.GetSettings();
			s.DispOBJ = DispOBJ.Checked;
			s.DispBG = DispBG.Checked;
			MainForm.PutCoreSettings(s);
			Close();
		}
	}
}
