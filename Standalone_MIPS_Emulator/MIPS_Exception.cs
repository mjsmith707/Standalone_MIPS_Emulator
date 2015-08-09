/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

// Implements MIPS Machine Specific Exceptions
namespace Standalone_MIPS_Emulator {
// Might be useful http://www.eecs.harvard.edu/~dholland/os161/documentation/sys161/mips.html
public class MIPS_Exception : Exception {
	// CPU Exception Codes Enumeration
	public enum ExceptionCode {
		INT = 0,            // Interrupt
		MOD = 1,            // TLB modification exception
		TLBL = 2,           // TLB exception (load or instruction fetch)
		TLBS = 3,           // TLB exception (store)
		ADEL = 4,           // Address error exception (load or instruction fetch)
		ADES = 5,           // Address error exception (store)
		IBE =  6,           // Bus error exception (instruction fetch)
		DBUS = 7,           // Bus error exception (data reference: load or store)
		SYSCALL = 8,        // Syscall exception
		BKPT = 9,           // Breakpoint exception
		RI = 10,            // Reserved instruction exception
		CPU = 11,           // Coprocessor Unusable exception
		OVF = 12,           // Arithmetic Overflow exception
		TRAP = 13,          // Trap exception
		Reserved14 = 14,    // Reserved
		FPE = 15,           // Floating point exception
		Reserved16 = 16,    // Reserved
		Reserved17 = 17,    // Reserved
		C2E = 18,           // Reserved for precise Coprocessor 2 exceptions
		Reserved19 = 19,    // Reserved
		Reserved20 = 20,    // Reserved
		Reserved21 = 21,    // Reserved
		MDMX = 22,          // MDMX (Unused)
		WATCH = 23,         // Reference to WatchHi/WatchLo address (unused)
		MCheck = 24,        // Machine check
		Reserved25 = 25,    // Reserved
		Reserved26 = 26,    // Reserved
		Reserved27 = 27,    // Reserved
		Reserved28 = 28,    // Reserved
		Reserved29 = 29,    // Reserved
		CacheErr = 30,      // Cache error. Has dedicated vector and Cause register is not updated
		Reserved31 = 31,    // Reserved
		UNIMPLEMENTED = 32  // Placeholder exception
	};

	// Interrupt Numbers
	public enum InterruptNumber {
		INVALID = 0,
		IP0 = 8,
		IP1 = 9,
		IP2 = 10,
		IP3 = 11,
		IP4 = 12,
		IP5 = 13,
		IP6 = 14,
		IP7 = 15
	};

	// Stored Exception Code
	private ExceptionCode code;
	private InterruptNumber interrupt;

	// Parameterized Constructor
	public MIPS_Exception(ExceptionCode code) {
		this.code = code;
		this.interrupt = InterruptNumber.INVALID;
	}

	public MIPS_Exception(InterruptNumber number) {
		this.code = ExceptionCode.INT;
		this.interrupt = number;
	}

	// Public code accessor
	public ExceptionCode getCode() {
		return this.code;
	}

	// Interrupt number accessor
	public InterruptNumber getIntNumber() {
		return this.interrupt;
	}
}
}