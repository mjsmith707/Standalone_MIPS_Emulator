/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

// Implements the particularlities of Coprocessor0's registers
namespace Standalone_MIPS_Emulator
{
	public class MIPS_CPC0Register
	{
		private enum REGBitRW {
			LOCKED = 0,
			READ = 1 , 
			READWRITE = 2
		};

		private REGBitRW[] bitfields;
		private UInt32 register;

		public MIPS_CPC0Register() {
			this.bitfields = new REGBitRW[32];
		}

		public MIPS_CPC0Register(UInt32 value, UInt32 mask1, UInt32 mask2) {
			this.register = value;
			this.bitfields = new REGBitRW[32];
			this.setRWMask(mask1, mask2);
		}

		// Sets Register Bit Read/Write locks
		// E.x.
		// Bit:	  3210
		// Mask1: 0111
		// Mask2: 0011
		// LOCK bit3, READ bit 2, READWRITE bit 1, READWRITE bit 0
		public void setRWMask(UInt32 mask1, UInt32 mask2) {
			byte value1 = 0;
			byte value2 = 0;
			const UInt32 bitmask = 0x1;

			for (Int32 i=0; i<31; i++) {
				value1 = (byte)((mask1&(bitmask << i)) >> i);
				value2 = (byte)((mask2&(bitmask << i)) >> i);
				if ((value1 == 0) && (value2 == 0)) {
					bitfields[i] = REGBitRW.LOCKED;
				}
				else if ((value1 == 1) && (value2 == 0)) {
					bitfields[i] = REGBitRW.READ;
				}
				else if ((value1 == 1) && (value2 == 1)) {
					bitfields[i] = REGBitRW.READWRITE;
				}
				else {
					throw new ApplicationException("Invalid RWX Mask for CPC0 register.");
				}
			}
		}

		// Reading LOCKED field always returns 0
		// but if we wrote setValue and our initializer values correctly
		// we should be just fine.
		public UInt32 getValue() {
			return this.register;
		}

		// Need to honor the RW Mask array.
		// Writing to READ field is ignored
		// Writing to LOCKED field is UNDEFINED by architecture
		// but we'll just ignore it too.

		public void setValue(UInt32 value) {
			byte bit1 = 0;
			byte bit2 = 0;
			UInt32 newvalue = 0;
			const UInt32 mask = 0x1;

			for (Int32 i=0; i<31; i++) {
				bit1 = (byte)((this.register&(mask << i)) >> i);
				bit2 = (byte)((value&(mask << i)) >> i);
				if (bitfields[i] == REGBitRW.LOCKED) {
					// Ignore
					newvalue |= (bit1&mask);
				}
				else if (bitfields[i] == REGBitRW.READ) {
					// Ignore
					newvalue |= (bit1&mask);
				}
				else if (bitfields[i] == REGBitRW.READWRITE) {
					// Write Always
					newvalue |= (bit2&mask);
				}
				else {
					throw new ApplicationException("Invalid RWX Mask for CPC0 register during writing.");
				}
			}

			this.register = newvalue;
		}
	}
}

