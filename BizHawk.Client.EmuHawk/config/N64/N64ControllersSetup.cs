using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Nintendo.N64;

namespace BizHawk.Client.EmuHawk
{
	public partial class N64ControllersSetup : ConfigForm
	{
		[RequiredService]
		private N64 Core { get; set; }

		private List<N64ControllerSettingControl> ControllerSettingControls
		{
			get
			{
				return Controls
					.OfType<N64ControllerSettingControl>()
					.OrderBy(n => n.ControllerNumber)
					.ToList();
			}
		}

		public N64ControllersSetup()
		{
			InitializeComponent();
		}

		private void N64ControllersSetup_Load(object sender, EventArgs e)
		{
			var n64Settings = Core.GetSyncSettings();
			
			ControllerSettingControls
				.ForEach(c =>
				{
					c.IsConnected = n64Settings.Controllers[c.ControllerNumber - 1].IsConnected;
					c.PakType = n64Settings.Controllers[c.ControllerNumber - 1].PakType;
					c.Refresh();
				});
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			var n64Settings = Core.GetSyncSettings();
			
			ControllerSettingControls
				.ForEach(c =>
				{
					n64Settings.Controllers[c.ControllerNumber - 1].IsConnected = c.IsConnected;
					n64Settings.Controllers[c.ControllerNumber - 1].PakType = c.PakType;
				});

			Core.PutSyncSettings(n64Settings);

			DialogResult = DialogResult.OK;
			Close();
		}

		private void CancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
