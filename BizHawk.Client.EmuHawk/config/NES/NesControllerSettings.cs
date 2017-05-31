using System;
using System.Linq;
using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Nintendo.NES;

namespace BizHawk.Client.EmuHawk
{
	public partial class NesControllerSettings : ConfigForm
	{
		[RequiredService]
		private NES Core { get; set; }

		private NES.NESSyncSettings _syncSettings;

		public NesControllerSettings()
		{
			InitializeComponent();
		}

		private void NesControllerSettings_Load(object sender, EventArgs e)
		{
			_syncSettings = Core.GetSyncSettings();

			// TODO: use combobox extension and add descriptions to enum values
			comboBoxFamicom.Items.AddRange(NESControlSettings.GetFamicomExpansionValues().ToArray());
			comboBoxNESL.Items.AddRange(NESControlSettings.GetNesPortValues().ToArray());
			comboBoxNESR.Items.AddRange(NESControlSettings.GetNesPortValues().ToArray());

			comboBoxFamicom.SelectedItem = _syncSettings.Controls.FamicomExpPort;
			comboBoxNESL.SelectedItem = _syncSettings.Controls.NesLeftPort;
			comboBoxNESR.SelectedItem = _syncSettings.Controls.NesRightPort;
			checkBoxFamicom.Checked = _syncSettings.Controls.Famicom;
		}

		private void CheckBoxFamicom_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxFamicom.Enabled = checkBoxFamicom.Checked;
			comboBoxNESL.Enabled = !checkBoxFamicom.Checked;
			comboBoxNESR.Enabled = !checkBoxFamicom.Checked;
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			var ctrls = new NESControlSettings
			{
				Famicom = checkBoxFamicom.Checked,
				FamicomExpPort = (string)comboBoxFamicom.SelectedItem,
				NesLeftPort = (string)comboBoxNESL.SelectedItem,
				NesRightPort = (string)comboBoxNESR.SelectedItem
			};

			bool changed = NESControlSettings.NeedsReboot(ctrls, _syncSettings.Controls);

			_syncSettings.Controls = ctrls;

			if (changed)
			{
				MainForm.PutCoreSyncSettings(_syncSettings);
			}

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
