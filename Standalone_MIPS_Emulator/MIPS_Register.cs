using System;

// MIPS Register Class
// Essentially a UInt32 wrapper plus muting ability.
namespace Standalone_MIPS_Emulator
{
	public class MIPS_Register
	{
		private UInt32 register;
		private Boolean muted;

		public MIPS_Register()
		{
			this.register = 0;
			this.muted = false;
		}

		public MIPS_Register(UInt32 value) {
			this.register = value;
		}

		public MIPS_Register(UInt32 value, Boolean mute) {
			this.register = value;
			this.muted = mute;
		}

		public void setValue(UInt32 value) {
			if (muted) {
				return;
			} else {
				this.register = value;
			}
		}

		public UInt32 getValue() {
			return this.register;
		}
	}
}

