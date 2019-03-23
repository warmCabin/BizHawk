using System;
using System.ComponentModel;

using Newtonsoft.Json;

using BizHawk.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.Computers.VIC20
{
	public partial class VIC20Hawk : IEmulator, IStatable, ISettable<VIC20Hawk.VIC20Settings, VIC20Hawk.VIC20SyncSettings>
	{
		public VIC20Settings GetSettings()
		{
			return _settings.Clone();
		}

		public VIC20SyncSettings GetSyncSettings()
		{
			return _syncSettings.Clone();
		}

		public bool PutSettings(VIC20Settings o)
		{
			_settings = o;
			return false;
		}

		public bool PutSyncSettings(VIC20SyncSettings o)
		{
			bool ret = VIC20SyncSettings.NeedsReboot(_syncSettings, o);
			_syncSettings = o;
			return ret;
		}

		private VIC20Settings _settings = new VIC20Settings();
		public VIC20SyncSettings _syncSettings = new VIC20SyncSettings();

		public class VIC20Settings
		{

			public VIC20Settings Clone()
			{
				return (VIC20Settings)MemberwiseClone();
			}
		}

		public class VIC20SyncSettings
		{
			[JsonIgnore]
			public string Port1 = VIC20HawkControllerDeck.DefaultControllerName;

			public enum ControllerType
			{
				Default,
			}

			[JsonIgnore]
			private ControllerType _VIC20Controller;

			[DisplayName("Controller")]
			[Description("Select Controller Type")]
			[DefaultValue(ControllerType.Default)]
			public ControllerType VIC20Controller
			{
				get { return _VIC20Controller; }
				set
				{
					if (value == ControllerType.Default) { Port1 = VIC20HawkControllerDeck.DefaultControllerName; }
					else { Port1 = VIC20HawkControllerDeck.DefaultControllerName; }

					_VIC20Controller = value;
				}
			}

			[DisplayName("Use Existing SaveRAM")]
			[Description("When true, existing SaveRAM will be loaded at boot up")]
			[DefaultValue(false)]
			public bool Use_SRAM { get; set; }

			public VIC20SyncSettings Clone()
			{
				return (VIC20SyncSettings)MemberwiseClone();
			}

			public static bool NeedsReboot(VIC20SyncSettings x, VIC20SyncSettings y)
			{
				return !DeepEquality.DeepEquals(x, y);
			}
		}
	}
}
