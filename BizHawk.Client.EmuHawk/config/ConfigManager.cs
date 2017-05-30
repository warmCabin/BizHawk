using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using BizHawk.Emulation.Common;

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

		public bool IsAvailable<T>(IEmulatorServiceProvider serviceProvider)
			where T : Form
		{
			return IsAvailable(typeof(T), serviceProvider);
		}

		private bool IsAvailable(Type t, IEmulatorServiceProvider serviceProvider)
		{
			if (!ServiceInjector.IsAvailable(serviceProvider, t))
			{
				return false;
			}

			var tool = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(type => type == t);

			if (tool == null) // This isn't a tool, must not be available
			{
				return false;
			}

			return true;
		}

		public T ShowDialog<T>(IEmulatorServiceProvider serviceProvider)
			where T : Form
		{

			T newDialog = Activator.CreateInstance<T>();
			newDialog.Owner = _mainForm;

			var result = ServiceInjector.UpdateServices(serviceProvider, newDialog);

			if (!result)
			{
				throw new InvalidOperationException("Current core can not provide all the required dependencies");
			}

			// TODO: inject common properties like MainForm, Config, GameInfo here
			newDialog.ShowDialog();

			return newDialog;
		}
	}
}
