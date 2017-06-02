using System;
using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Nintendo.SNES;
using BizHawk.Client.EmuHawk.WinFormExtensions;

namespace BizHawk.Client.EmuHawk
{
	public partial class SNESControllerSettings : ConfigForm
	{
		[RequiredService]
		private LibsnesCore Snes { get; set; }

		private LibsnesCore.SnesSyncSettings _syncSettings;
		private bool _supressDropdownChangeEvents;

		public SNESControllerSettings()
		{
			InitializeComponent();
		}

		private void SNESControllerSettings_Load(object sender, EventArgs e)
		{
			_syncSettings = Snes.GetSyncSettings().Clone();

			LimitAnalogChangeCheckBox.Checked = _syncSettings.LimitAnalogChangeSensitivity;

			_supressDropdownChangeEvents = true;
			Port1ComboBox.PopulateFromEnum<LibsnesControllerDeck.ControllerType>(_syncSettings.LeftPort);
			Port2ComboBox.PopulateFromEnum<LibsnesControllerDeck.ControllerType>(_syncSettings.RightPort);
			_supressDropdownChangeEvents = false;
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			bool changed =
				_syncSettings.LeftPort.ToString() != Port1ComboBox.SelectedItem.ToString()
				|| _syncSettings.RightPort.ToString() != Port2ComboBox.SelectedItem.ToString()
				|| _syncSettings.LimitAnalogChangeSensitivity != LimitAnalogChangeCheckBox.Checked;

			if (changed)
			{
				_syncSettings.LeftPort = (LibsnesControllerDeck.ControllerType)Enum.Parse(typeof(LibsnesControllerDeck.ControllerType), Port1ComboBox.SelectedItem.ToString());
				_syncSettings.RightPort = (LibsnesControllerDeck.ControllerType)Enum.Parse(typeof(LibsnesControllerDeck.ControllerType), Port2ComboBox.SelectedItem.ToString());
				_syncSettings.LimitAnalogChangeSensitivity = LimitAnalogChangeCheckBox.Checked;

				MainForm.PutCoreSyncSettings(_syncSettings);
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void CancelBtn_Click(object sender, EventArgs e)
		{
			OSD.AddMessage("Controller settings aborted");
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void PortComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_supressDropdownChangeEvents)
			{
				var leftPort = (LibsnesControllerDeck.ControllerType)Enum.Parse(typeof(LibsnesControllerDeck.ControllerType), Port1ComboBox.SelectedItem.ToString());
				var rightPort = (LibsnesControllerDeck.ControllerType)Enum.Parse(typeof(LibsnesControllerDeck.ControllerType), Port2ComboBox.SelectedItem.ToString());
				ToggleMouseSection(leftPort == LibsnesControllerDeck.ControllerType.Mouse
					|| rightPort == LibsnesControllerDeck.ControllerType.Mouse);
			}
		}

		private void ToggleMouseSection(bool show)
		{
			LimitAnalogChangeCheckBox.Visible =
				MouseSpeedLabel1.Visible =
				MouseSpeedLabel2.Visible =
				MouseSpeedLabel3.Visible =
				MouseNagLabel1.Visible =
				MouseNagLabel2.Visible =
				show;
		}
	}
}
