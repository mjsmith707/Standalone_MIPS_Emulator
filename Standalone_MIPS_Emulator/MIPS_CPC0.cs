/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

// MIPS Control Coprocessor 0
// Not fully implemented
namespace Standalone_MIPS_Emulator
{
	public class MIPS_CPC0 : MIPS_Coprocessor
	{
		private MIPS_CPC0Register[,] registerFile;

		public MIPS_CPC0 ()
		{
			// Initialize Registers
			// Too easy to just have normal registers
			// we have to deal with the sel field...
			// and tons of other gotchas
			// [registernum,selfield] rwmask1, rwmask2
			// MIPS Arch Vol 3
			//        00000000000000000000000000000000
			// mask1: 10000000000000000000000000111111
			// mask2: 00000000000000000000000000111111
			// reset: 00000000000000000000000000000000

			registerFile = new MIPS_CPC0Register[32,32];

			// Index Register
			// 48 Entry TLB (6 bits) so Index bits are masked as RW
			// reset: 00000000000000000000000000000000
			// mask1: 10000000000000000000000000111111
			// mask2: 00000000000000000000000000111111
			registerFile[0,0] = new MIPS_CPC0Register(0x0, 0x8000003F, 0x3F);

			// Random Register
			// Same as Index except masked R
			// FIXME: Zero TLB Entries on reset?
			// reset: 00000000000000000000000000000000
			// mask1: 10000000000000000000000000111111
			// mask2: 00000000000000000000000000000000
			registerFile[1,0] = new MIPS_CPC0Register(0x0, 0x8000003F, 0x0);

			// EntryLo0 Register, EntryLo1 Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111111111111111111
			// mask2: 01111111111111111111111111111111
			registerFile[2,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFFF, 0x7FFFFFFF);
			registerFile[3,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFFF, 0x7FFFFFFF);

			// Context Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111111111111110000
			// mask2: 11111111100000000000000000000000
			registerFile[4,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFF0, 0xFF80000);

			// PageMask Register
			// reset: 00000000000000000000000000000000
			// mask1: 00011111111111111111000000000000
			// mask2: 00011111111111111111000000000000
			registerFile[5,0] = new MIPS_CPC0Register(0x0, 0x1FFFF000, 0x1FFFF000);

			// Wired Register
			// reset: 00000000000000000000000000000000
			// mask1: 00000000000000000000000000111111
			// mask2: 00000000000000000000000000111111
			registerFile[6,0] = new MIPS_CPC0Register(0x0, 0x0000003F, 0x0000003F);

			// Reserved Register
			// reset: 00000000000000000000000000000000
			// mask1: 00000000000000000000000000000000
			// mask2: 00000000000000000000000000000000
			registerFile[7,0] = new MIPS_CPC0Register(0x0, 0x0, 0x0);

			// BadVAddr Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111111111111111111
			// mask2: 00000000000000000000000000000000
			registerFile[8,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFFF, 0x0);

			// Count Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111111111111111111
			// mask2: 11111111111111111111111111111111
			registerFile[9,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFFF, 0xFFFFFFFF);

			// Reserved Registers
			// reset: 00000000000000000000000000000000
			// mask1: 00000000000000000000000000000000
			// mask2: 00000000000000000000000000000000
			registerFile[9,6] = new MIPS_CPC0Register(0x0, 0x0, 0x0);
			registerFile[9,7] = new MIPS_CPC0Register(0x0, 0x0, 0x0);

			// EntryHi Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111110000011111111
			// mask2: 11111111111111111110000011111111
			registerFile[10,0] = new MIPS_CPC0Register(0x0, 0xFFFFE0FF, 0xFFFFE0FF);

			// Compare Register
			// reset: 00000000000000000000000000000000
			// mask1: 11111111111111111111111111111111
			// mask2: 11111111111111111111111111111111
			registerFile[11,0] = new MIPS_CPC0Register(0x0, 0xFFFFFFFF, 0xFFFFFFFF);

			// Reserved Registers
			// reset: 00000000000000000000000000000000
			// mask1: 00000000000000000000000000000000
			// mask2: 00000000000000000000000000000000
			registerFile[11,6] = new MIPS_CPC0Register(0x0, 0x0, 0x0);
			registerFile[11,7] = new MIPS_CPC0Register(0x0, 0x0, 0x0);

			// Status Register
			// reset: 00000000000000000000000000000000
			// mask1: 
			// mask2: 
			registerFile[12,0] = new MIPS_CPC0Register();
			registerFile[13,0] = new MIPS_CPC0Register();
			registerFile[14,0] = new MIPS_CPC0Register();
			registerFile[15,0] = new MIPS_CPC0Register();
			registerFile[16,0] = new MIPS_CPC0Register();
			registerFile[16,1] = new MIPS_CPC0Register();
			registerFile[16,2] = new MIPS_CPC0Register();
			registerFile[16,3] = new MIPS_CPC0Register();
			registerFile[16,6] = new MIPS_CPC0Register();
			registerFile[16,7] = new MIPS_CPC0Register();
			registerFile[17,0] = new MIPS_CPC0Register();
			registerFile[18,0] = new MIPS_CPC0Register();
			registerFile[19,0] = new MIPS_CPC0Register();
			registerFile[20,0] = new MIPS_CPC0Register();
			registerFile[21,0] = new MIPS_CPC0Register();
			registerFile[22,0] = new MIPS_CPC0Register();
			registerFile[23,0] = new MIPS_CPC0Register();
			registerFile[24,0] = new MIPS_CPC0Register();
			registerFile[25,0] = new MIPS_CPC0Register();
			registerFile[26,0] = new MIPS_CPC0Register();
			registerFile[27,0] = new MIPS_CPC0Register();
			registerFile[27,1] = new MIPS_CPC0Register();
			registerFile[27,2] = new MIPS_CPC0Register();
			registerFile[27,3] = new MIPS_CPC0Register();
			registerFile[28,0] = new MIPS_CPC0Register();
			registerFile[28,1] = new MIPS_CPC0Register();
			registerFile[29,0] = new MIPS_CPC0Register();
			registerFile[29,1] = new MIPS_CPC0Register();
			registerFile[30,0] = new MIPS_CPC0Register();
			registerFile[31,0] = new MIPS_CPC0Register();
		}

		public override UInt32 getRegister(byte register, byte sel)
		{
			return this.registerFile[register,sel].getValue();
		}

		public override void setRegister(byte register, byte sel, UInt32 value)
		{
			this.registerFile[register,sel].setValue(value);
		}
	}
}

