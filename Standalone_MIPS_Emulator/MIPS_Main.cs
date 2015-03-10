/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

namespace Standalone_MIPS_Emulator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			MIPS_CPU CPU0 = new MIPS_CPU();
			// 001000 00001 00010 00000 00000 001111
			// addi		r1   r2		F
			// 000100 00000 00000 11111 11111 111110
			// beq		r0 	r0		-2
			//UInt32 addi = 0x2022000F;
			//UInt32 noop = 0x0;
			//UInt32 beq = 0x1000FFFE;
			//CPU0.loadText(0x00000000, addi);
			//CPU0.loadText(0x00000004, beq);
			//CPU0.loadText(0x00000008, noop);
			CPU0.loadFile(0x00000000, "/Users/msmith/Desktop/main.bin");
			CPU0.start();
		}
	}
}
