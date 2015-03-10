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
	public class MIPS_InstructionContext
	{
		private byte opcode;
		private byte rs;
		private byte rt;
		private byte rd;
		private byte shamt;
		private byte funct;
		private UInt16 imm;
		private UInt32 jimm;
		private MIPS_Memory mainMemory;
		private MIPS_Register[] registerFile;
		private MIPS_Register PC;
		private MIPS_Register hi;
		private MIPS_Register lo;
		private MIPS_Boolean branchDelay;
		private MIPS_Register branchTarget;

		public MIPS_InstructionContext (ref MIPS_Memory memory, ref MIPS_Register[] regfile, ref MIPS_Register PC, ref MIPS_Register hi, ref MIPS_Register lo, ref MIPS_Register branchTarget, ref MIPS_Boolean branchDelay)
		{
			this.mainMemory = memory;
			this.registerFile = regfile;
			this.PC = PC;
			this.hi = hi;
			this.lo = lo;
			this.opcode = 0;
			this.rs = 0;
			this.rd = 0;
			this.rt = 0;
			this.imm = 0;
			this.jimm = 0;
			this.funct = 0;
			this.branchDelay = branchDelay;
			this.branchTarget = branchTarget;
		}

		public void setContext(byte opcode, byte rs, byte rt, byte rd, byte shamt, byte funct, UInt16 imm, UInt32 jimm) {
			this.opcode = opcode;
			this.rs = rs;
			this.rt = rt;
			this.rd = rd;
			this.shamt = shamt;
			this.funct = funct;
			this.imm = imm;
			this.jimm = jimm;
		}

		public MIPS_Register[] getRegisters() {
			return this.registerFile;
		}

		public MIPS_Memory getMemory() {
			return this.mainMemory;
		}

		public MIPS_Register getPC() {
			return this.PC;
		}

		public MIPS_Register getHI() {
			return this.hi;
		}

		public MIPS_Register getLO() {
			return this.lo;
		}

		public MIPS_Register getBranchTarget() {
			return this.branchTarget;
		}

		public void setBranchDelay(bool value) {
			this.branchDelay.setValue(value);
		}

		public byte getOpcode() {
			return this.opcode;
		}

		public byte getRS() {
			return this.rs;
		}

		public byte getRT() {
			return this.rt;
		}

		public byte getRD() {
			return this.rd;
		}

		public byte getShamt() {
			return this.shamt;
		}

		public byte getFunct() {
			return this.funct;
		}

		public UInt16 getImm() {
			return this.imm;
		}

		public UInt32 getJimm() {
			return this.jimm;
		}
	}
}