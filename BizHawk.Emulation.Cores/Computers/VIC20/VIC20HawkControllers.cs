using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using BizHawk.Common;
using BizHawk.Emulation.Common;

// Controller stuff
// I recommend saving this for until you get stuff booting
// see GBHawk for a typical implementation

namespace BizHawk.Emulation.Cores.Computers.VIC20
{
	/// <summary>
	/// Represents a VIC20 add on
	/// </summary>
	public interface IPort
	{
		byte Read(IController c);

		ControllerDefinition Definition { get; }

		void SyncState(Serializer ser);

		int PortNum { get; }
	}

	[DisplayName("VIC20 Controller")]
	public class StandardControls : IPort
	{
		public StandardControls(int portNum)
		{
			PortNum = portNum;
			Definition = new ControllerDefinition
			{
				Name = "VIC20 Controller",
				BoolButtons = BaseDefinition
				.Select(b => "P" + PortNum + " " + b)
				.ToList()
			};
		}

		public int PortNum { get; }

		public ControllerDefinition Definition { get; }

		public byte Read(IController c)
		{
			byte result = 0xFF;

			return result;
		}

		private static readonly string[] BaseDefinition =
		{

		};

		public void SyncState(Serializer ser)
		{
			//nothing
		}
	}
}