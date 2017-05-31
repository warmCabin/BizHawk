using System.Windows.Forms;

using BizHawk.Emulation.Common;
using BizHawk.Client.Common;

namespace BizHawk.Client.EmuHawk
{
	public class ConfigForm : Form
	{
		public MainForm MainForm { get; set; }
		public Config Config { get; set; }
		public GameInfo Game { get; set; }
		public OSDManager OSD { get; set; }
	}
}
