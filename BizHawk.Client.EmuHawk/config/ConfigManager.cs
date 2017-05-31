using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Client.Common;

namespace BizHawk.Client.EmuHawk
{
	// Loads and Injects Dependencies into Config dialogs
	// Currently the notion of a config dialog is anything that inherits Form
	// Currently only supports Model dialogs, Modeless requires extra logic for focusing
	// Currently only supports parameterless constructors, dependenies should be declared and injected
	public class ConfigManager
	{
		private readonly MainForm _mainForm;

		public ConfigManager(MainForm mainForm)
		{
			_mainForm = mainForm;
		}

		public bool IsAvailable<T>()
			where T : ConfigForm
		{
			Type t = typeof(T);
			if (!ServiceInjector.IsAvailable(_mainForm.Emulator.ServiceProvider, t))
			{
				return false;
			}

			var tool = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.FirstOrDefault(type => type == t);

			if (tool == null) // This isn't a tool, must not be available
			{
				return false;
			}

			return true;
		}

		public DialogResult ShowDialog<T>()
			where T : ConfigForm
		{
			T newDialog = Activator.CreateInstance<T>();

			var result = ServiceInjector.UpdateServices(_mainForm.Emulator.ServiceProvider, newDialog);

			if (!result)
			{
				throw new InvalidOperationException("Current core can not provide all the required dependencies");
			}

			newDialog.Owner = _mainForm;
			newDialog.MainForm = _mainForm;
			newDialog.Config = _mainForm.Config;
			newDialog.Game = _mainForm.Game;
			newDialog.OSD = _mainForm.OSD;
			var dialogResult = newDialog.ShowDialog();

			return dialogResult;
		}
	}
}
