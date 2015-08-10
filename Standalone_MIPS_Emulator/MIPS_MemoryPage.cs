/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

// Basic paged memory.
// Will need complete rewrite for virtual memory support
namespace Standalone_MIPS_Emulator {
	public class MIPS_MemoryPage {
		// 4096 Byte Page Size
		private const UInt32 pagesize = 0x00001000;
		private const UInt32 pagemask = 0x00000FFF;

		// Page members
		private UInt32 addrbase;
		private byte flags;
		private byte[] memory;

		public MIPS_MemoryPage (UInt32 addrbase, byte flags) {
			this.addrbase = addrbase;
			this.flags = flags;
			this.memory = new byte[pagesize];
		}

		public byte readByte(UInt32 address) {
			return memory[(address&pagemask)];
		}

		public void writeByte(UInt32 address, byte value) {
			memory[(address&pagemask)] = value;
		}
	}
}