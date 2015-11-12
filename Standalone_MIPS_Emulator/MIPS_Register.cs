/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

// MIPS GPR Register Class
// Essentially a uint wrapper plus muting ability.
namespace Standalone_MIPS_Emulator {
	public class MIPS_Register {
		private uint register;
		private Boolean muted;

		// Default Constructor
		public MIPS_Register() {
			this.register = 0;
			this.muted = false;
		}

		// Value Initializing Constructor
		public MIPS_Register(uint value) {
			this.register = value;
		}

		// Explicit mutable constructor
		public MIPS_Register(uint value, Boolean mute) {
			this.register = value;
			this.muted = mute;
		}

		// Mutable register setter
		public void setValue(uint value) {
			if (muted) {
				return;
			}
			else {
				this.register = value;
			}
		}

		public uint getValue() {
			return this.register;
		}
	}
}