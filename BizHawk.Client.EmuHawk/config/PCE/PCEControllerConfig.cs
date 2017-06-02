using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.PCEngine;

namespace BizHawk.Client.EmuHawk
{
	public partial class PCEControllerConfig : ConfigForm
	{
		[RequiredService]
		private PCEngine Pce { get; set; }

		public PCEControllerConfig()
		{
			InitializeComponent();
		}

		private void PCEControllerConfig_Load(object sender, EventArgs e)
		{
			var pceSettings = Pce.GetSyncSettings();
			for (int i = 0; i < 5; i++)
			{
				Controls.Add(new Label
				{
					Text = "Controller " + (i + 1),
					Location = new Point(15, 15 + (i * 25))
				});
				Controls.Add(new CheckBox
				{
					Text = "Connected",
					Name = "Controller" + i,
					Location = new Point(135, 15 + (i * 25)),
					Checked = pceSettings.Controllers[i].IsConnected
				});
			}
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			var pceSettings = Pce.GetSyncSettings();

			Controls
				.OfType<CheckBox>()
				.OrderBy(c => c.Name)
				.ToList()
				.ForEach(c =>
				{
					var index = int.Parse(c.Name.Replace("Controller", ""));
					pceSettings.Controllers[index].IsConnected = c.Checked;
				});
			MainForm.PutCoreSyncSettings(pceSettings);
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
