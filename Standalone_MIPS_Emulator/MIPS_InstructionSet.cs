/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Runtime.CompilerServices;

namespace Standalone_MIPS_Emulator
{
	public abstract class MIPS_Instruction
	{
		// Execution routine
		public abstract void execute(ref MIPS_InstructionContext context);

		// Sign Extend 32bits
		public int signExtend32To32(uint value) {
			return (int)value;
		}

		public int signExtend16To32(ushort value) {
			short val = (short)value;
			int val32 = val;
			return val32;
		}

		public int signExtend16To32(short value) {
			return (int)value;
		}

		// Sign Extend 16bits
		public short signExtend16To16(ushort value) {
			return (short)value;
		}

		// Zero Extend 32bits
		public uint zeroExtend16To32(ushort value) {
			return (uint)value;
		}

		// Zero Extend 16bits does nothing??
		public ushort zeroExtend16To16(ushort value) {
			return (ushort)value;
		}

		// Convenience Wrapper for Unsigned + Signed Arithmetic
		// C# handles this pretty poorly
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint unsigned32Signed32AddSub(uint unsignedVal, int signedVal) {
			uint result = (signedVal < 0) ? unsignedVal - (uint)(signedVal * (-1)) : unsignedVal + (uint)(signedVal);
			return result;
		}

		// Calculate Branch Transfer Address (PC-Relative)
		public uint calculateBTA(uint branchaddr, ushort imm) {
			short branchoffset = signExtend16To16(imm);
			branchoffset <<= 2;
			branchaddr += (UInt32)branchoffset;
			return branchaddr;
		}

		// Calculate Effective Target Address (PC-Region)
		public uint calculateETA(uint branchaddr, uint jimm) {
			jimm <<= 2;
			uint targetaddr = (branchaddr & 0xF0000000);
			targetaddr |= jimm;
			return targetaddr;
		}
	}

	// Shift Word Left Logical
	public class MIPS_SLL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint result = context.getRegisters()[context.getRT()].getValue();
			result <<= context.getShamt();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Logical
	public class MIPS_SRL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint result = context.getRegisters()[context.getRT()].getValue();
			result >>= context.getShamt();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Arithmetic
	public class MIPS_SRA : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint rt = context.getRegisters()[context.getRT()].getValue();
			int result = (int)rt >> context.getShamt();
			context.getRegisters()[context.getRD()].setValue((uint)result);
		}
	}

	// Shift Word Left Logical Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SLLV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint result = context.getRegisters()[context.getRT()].getValue();
			result <<= (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Logical Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SRLV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint result = context.getRegisters()[context.getRT()].getValue();
			result >>= (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Arithmetic Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SRAV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint rt = context.getRegisters()[context.getRT()].getValue();
			int result = (int)rt >> (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue((uint)result);
		}
	}

	// Jump Register
	public class MIPS_JR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getBranchTarget().setValue(context.getRegisters()[context.getRS()].getValue());
			context.setBranchDelay(true);
		}
	}

	// Jump and Link Register
	public class MIPS_JALR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[31].setValue(context.getRegisters()[context.getRD()].getValue());
			context.getBranchTarget().setValue(context.getRegisters()[context.getRS()].getValue());
			context.setBranchDelay(true);
		}
	}

	// Move Conditional on Zero
	public class MIPS_MOVZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRT()].getValue() == 0) {
				context.getRegisters()[context.getRD()].setValue(context.getRegisters()[context.getRS()].getValue());
			}
		}
	}

	// Move Conditional on Not Zero
	public class MIPS_MOVN : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRT()].getValue() != 0) {
				context.getRegisters()[context.getRD()].setValue(context.getRegisters()[context.getRS()].getValue());
			}
		}
	}

	public class MIPS_SYSCALL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.SYSCALL);
		}
	}

	// Breakpoint
	public class MIPS_BREAK : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.BKPT);
		}
	}

	public class MIPS_SYNC : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Move From HI Register
	public class MIPS_MFHI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(context.getHI().getValue());
		}
	}

	// Move to HI Register
	public class MIPS_MTHI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getHI().setValue(context.getRegisters()[context.getRS()].getValue());
		}
	}

	// Move From LO Register
	public class MIPS_MFLO : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(context.getLO().getValue());
		}
	}

	// Move to LO Register
	public class MIPS_MTLO : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getLO().setValue(context.getRegisters()[context.getRS()].getValue());
		}
	}

	// Multiply Word
	public class MIPS_MULT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				Int64 result = context.getRegisters()[context.getRS()].getValue();
				result *= (int)context.getRegisters()[context.getRT()].getValue();
				context.getLO().setValue((uint)(result & 0x00000000FFFFFFFF));
				context.getHI().setValue((uint)(result >> 32));
			}
		}
	}

	// Multiply Word Unsigned
	public class MIPS_MULTU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				UInt64 result = context.getRegisters()[context.getRS()].getValue();
				result *= context.getRegisters()[context.getRT()].getValue();
				context.getLO().setValue((uint)(result & 0x00000000FFFFFFFF));
				context.getHI().setValue((uint)(result >> 32));
			}
		}
	}

	// Divide Word
	public class MIPS_DIV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				int rs = (int)context.getRegisters()[context.getRS()].getValue();
				int rt = (int)context.getRegisters()[context.getRT()].getValue();

				context.getLO().setValue((uint)(rs/rt));
				context.getHI().setValue((uint)(rs%rt));
			}
		}
	}

	// Divide Unsigned Word
	public class MIPS_DIVU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				uint rs = context.getRegisters()[context.getRS()].getValue();
				uint rt = context.getRegisters()[context.getRT()].getValue();

				context.getLO().setValue((rs/rt));
				context.getHI().setValue((rs%rt));
			}
		}
	}

	// Signed Addition
	public class MIPS_ADD : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				int rsval = base.signExtend32To32(context.getRegisters()[context.getRS()].getValue());
				int rtval = base.signExtend32To32(context.getRegisters()[context.getRT()].getValue());

				checked {
					rsval += rtval;
				}

				context.getRegisters()[context.getRD()].setValue((uint)rsval);
			}
			catch (System.OverflowException) {
				// Trigger exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.OVF);
			}
		}
	}

	// Unsigned Addition
	public class MIPS_ADDU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				context.getRegisters()[context.getRD()].setValue(
				    context.getRegisters()[context.getRS()].getValue()
				    +
				    context.getRegisters()[context.getRT()].getValue());
			}
		}
	}

	// Subtract Word
	public class MIPS_SUB : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				context.getRegisters()[context.getRD()].setValue((uint)checked(
				            base.signExtend32To32(context.getRegisters()[context.getRS()].getValue())
				            -
				            base.signExtend32To32(context.getRegisters()[context.getRT()].getValue())));
			}
			catch (System.OverflowException) {
				// Trigger Exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.OVF);
			}
		}
	}

	// Subtract Word Unsigned
	public class MIPS_SUBU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				context.getRegisters()[context.getRD()].setValue(
				    context.getRegisters()[context.getRS()].getValue()
				    -
				    context.getRegisters()[context.getRT()].getValue());
			}
		}
	}

	// Bitwise AND
	public class MIPS_AND : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    &
			    context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Or
	public class MIPS_OR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    |
			    context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Exclusive OR
	public class MIPS_XOR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    ^
			    context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Not Or
	public class MIPS_NOR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint result = ~(context.getRegisters()[context.getRS()].getValue() | context.getRegisters()[context.getRT()].getValue());
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Set on Less Than
	public class MIPS_SLT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < (int)context.getRegisters()[context.getRT()].getValue()) {
				context.getRegisters()[context.getRD()].setValue(1);
			} 
			else {
				context.getRegisters()[context.getRD()].setValue(0);
			}
		}
	}

	// Set on Less Than Unsigned
	public class MIPS_SLTU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() < context.getRegisters()[context.getRT()].getValue()) {
				context.getRegisters()[context.getRD()].setValue(1);
			} 
			else {
				context.getRegisters()[context.getRD()].setValue(0);
			}
		}
	}

	// Trap if Greater or Equal
	public class MIPS_TGE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() >= (int)context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Trap if Greater or Equal Unsigned
	public class MIPS_TGEU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() >= context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Trap if Less Than
	public class MIPS_TLT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < (int)context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Trap if Less Than Unsigned
	public class MIPS_TLTU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() < context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Trap if Equal
	public class MIPS_TEQ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() == (int)context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Trap if Not Equal
	public class MIPS_TNE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() != (int)context.getRegisters()[context.getRT()].getValue()) {
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.TRAP);
			}
		}
	}

	// Jump
	public class MIPS_J : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getBranchTarget().setValue(base.calculateETA(context.getPC().getValue(), context.getJimm()));
			context.setBranchDelay(true);
		}
	}

	// Jump and Link
	public class MIPS_JAL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
			context.getBranchTarget().setValue(base.calculateETA(context.getPC().getValue(), context.getJimm()));
			context.setBranchDelay(true);
		}
	}

	// Branch on Equal
	public class MIPS_BEQ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() == context.getRegisters()[context.getRT()].getValue()) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Not Equal
	public class MIPS_BNE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() != context.getRegisters()[context.getRT()].getValue()) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Less Than or Equal to Zero
	public class MIPS_BLEZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() <= 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Greater Than Zero
	public class MIPS_BGTZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() > 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Signed Addition Immediate
	public class MIPS_ADDI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
					int rsval = base.signExtend32To32(context.getRegisters()[context.getRS()].getValue());
					int immval = base.signExtend16To32(context.getImm());
					// C# Will overflow on the (uint)rsval cast. You have got to be kidding me
					checked {
						rsval += immval;
					}
					context.getRegisters()[context.getRT()].setValue((uint)rsval);
			}
			catch (System.OverflowException) {
				// Trigger Exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.OVF);
			}
		}
	}

	// Add Immediate Unsigned Word
	public class MIPS_ADDIU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			unchecked {
				int result = base.signExtend32To32(context.getRegisters()[context.getRS()].getValue());
				// unsigned + signed hackaround for C#
				int imm = base.signExtend16To32(context.getImm());
				result += imm;
				context.getRegisters()[context.getRT()].setValue((uint)result);
			}
		}
	}

	// Set on Less Than Immediate
	public class MIPS_SLTI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < base.signExtend16To16(context.getImm())) {
				context.getRegisters()[context.getRT()].setValue(0x1);
			}
			else {
				context.getRegisters()[context.getRT()].setValue(0x0);
			}
		}
	}

	// Set on Less Than Immediate Unsigned
	public class MIPS_SLTIU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() < (ushort)base.signExtend16To16(context.getImm())) {
				context.getRegisters()[context.getRT()].setValue(0x1);
			}
			else {
				context.getRegisters()[context.getRT()].setValue(0x0);
			}
		}
	}

	// And Immediate
	public class MIPS_ANDI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    &
			    base.zeroExtend16To32(context.getImm()));
		}
	}

	// Or Immediate
	public class MIPS_ORI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    |
			    base.zeroExtend16To32(context.getImm()));
		}
	}

	// Exclusive OR Immediate
	public class MIPS_XORI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue(
			    context.getRegisters()[context.getRS()].getValue()
			    ^
			    base.zeroExtend16To32(context.getImm()));
		}
	}

	// Load Upper Immediate
	public class MIPS_LUI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue((uint)((uint)context.getImm() << 16));
		}
	}

	// Move from Coprocessor 0
	public class MIPS_MFC0 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			// FIXME: Requires sel field to be zero.... but who cares? Not True?
			// FIXME: Throws Coprocessor Unusuable, Reserved Instruction
			const byte selmask = 0x7;
			byte sel = (byte)(base.zeroExtend16To16(context.getImm())&selmask);
			byte cpcregister = (byte)context.getRD();
			uint value = context.getCoprocessors()[0].getRegister(cpcregister, sel);
			context.getRegisters()[context.getRT()].setValue(value);
		}
	}

	// Move to Coprocessor 0
	public class MIPS_MTC0 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			// FIXME: Requires sel field to be zero.... but who cares? Not True?
			// FIXME: Throws Coprocessor Unusuable, Reserved Instruction
			const byte selmask = 0x7;
			byte sel = (byte)(base.zeroExtend16To16(context.getImm())&selmask);

			uint value = context.getRegisters()[context.getRT()].getValue();
			context.getCoprocessors()[0].setRegister(context.getRD(), sel, value);

			// Check for Cause register software interrupts IP0/IP1
			// CPU.serviceint() will test whether interrupt is valid
			if ((context.getRD() == 13) && (sel == 0)) {
				if (((value & 0x100) >> 8) == 0x1) {
					throw new MIPS_Exception(MIPS_Exception.InterruptNumber.IP0);
				} else if (((value & 0x200) >> 9) == 0x1) {
					throw new MIPS_Exception(MIPS_Exception.InterruptNumber.IP1);
				}
			}
		}
	}

	public class MIPS_COP1 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_COP2 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Branch on Equal Likely
	public class MIPS_BEQL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() == context.getRegisters()[context.getRT()].getValue()) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	// Branch on Not Equal Likely
	public class MIPS_BNEL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() != context.getRegisters()[context.getRT()].getValue()) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	// Branch on Less Than or Equal to Zero Likely
	public class MIPS_BLEZL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() <= 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	// Branch on Greater Than Zero Likely
	public class MIPS_BGTZL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() > 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	// Load Byte
	public class MIPS_LB : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadByte((uint)vAddr));
		}
	}

	// Load Halfword
	public class MIPS_LH : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadHalf((uint)vAddr));
		}
	}

	public class MIPS_LWL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			short lefthalf = (short)context.getMemory().ReadHalf((uint)vAddr-1);
			int word = 0x0;
			word = lefthalf;
			word <<= 16;
			uint finalword = (uint)word;
			finalword &= 0xFFFF0000;	// Probably unnecessary if C# shifts in zeroes
			context.getRegisters()[context.getRT()].setValue(finalword);
		}
	}

	// Load Word
	public class MIPS_LW : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadWord((uint)vAddr));
		}
	}

	// Load Byte Unsigned
	public class MIPS_LBU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			ushort offset = zeroExtend16To16(context.getImm());
			uint address = context.getRegisters()[context.getRD()].getValue();
			address += (uint)offset;
			uint value = context.getMemory().ReadByte(address);
			context.getRegisters()[context.getRT()].setValue(value);
		}
	}

	// Load Halfword Unsigned
	public class MIPS_LHU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			ushort offset = zeroExtend16To16(context.getImm());
			uint address = context.getRegisters()[context.getRD()].getValue();
			address += (uint)offset;
			uint value = context.getMemory().ReadHalf(address);
			context.getRegisters()[context.getRT()].setValue(value);
		}
	}

	// Load Word Right
	public class MIPS_LWR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			short righthalf = (short)context.getMemory().ReadHalf((uint)vAddr-1);
			int word = righthalf;
			context.getRegisters()[context.getRT()].setValue((uint)word);
		}
	}

	// Store Byte
	public class MIPS_SB : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreByte((uint)vAddr, (byte)context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Store Halfword
	public class MIPS_SH : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreHalf((uint)vAddr, (ushort)context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Store Word Left
	public class MIPS_SWL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			uint rt = context.getRegisters()[context.getRT()].getValue();
			ushort lefthalf = (ushort)(rt & 0xFFFF0000);
			Byte left1 = (Byte)(lefthalf & 0xFF00);
			Byte left2 = (Byte)(lefthalf & 0x00FF);
			context.getMemory().StoreByte((uint)vAddr, left2);
			context.getMemory().StoreByte((uint)vAddr-1, left1);
		}
	}

	// Store Word
	public class MIPS_SW : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreWord((uint)vAddr, context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Store Word Right
	public class MIPS_SWR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = base.signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			uint rt = context.getRegisters()[context.getRT()].getValue();
			ushort righthalf = (ushort)(rt & 0x0000FFFF);
			context.getMemory().StoreHalf((uint)vAddr-1, righthalf);
		}
	}

	public class MIPS_CACHE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Load Linked Word
	// FIXME: Partial Support (Not atomic, requires memory rewrite)
	// Lots of Int/Uint casting going on but manual is ambiguous about negative addresses
	public class MIPS_LL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadWord((uint)vAddr));
		}
	}

	public class MIPS_SDC1 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_SDC2 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_LWCL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_PREF : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Store Conditional Word
	// FIXME: Partial Support (Not atomic, requires memory rewrite)
	public class MIPS_SC : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			int vAddr = signExtend16To32(context.getImm());
			vAddr += (int)context.getRegisters()[context.getRS()].getValue();
			// FIXME: if LLBit == 1 then
			context.getMemory().StoreWord((uint)vAddr, context.getRegisters()[context.getRT()].getValue());
			// FIXME: then rt = LLbit
			context.getRegisters()[context.getRT()].setValue(0x1);
		}
	}

	public class MIPS_SWCL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_RESERVED_INST : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			throw new MIPS_Exception(MIPS_Exception.ExceptionCode.RI);
		}
	}

	// Branch on Greater Than or Equal to Zero and Link
	public class MIPS_BGEZAL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() >= 0) {
				context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Greater Than or Equal to Zero
	public class MIPS_BGEZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() >= 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Greater Than or Equal to Zero and Link Likely
	public class MIPS_BGEZALL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() >= 0) {
				context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue() + 4);
			}
		}
	}

	// Branch on Greater Than or Equal to Zero Likely
	public class MIPS_BGEZL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() >= 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue() + 4);
			}
		}
	}

	// Branch on Less Than Zero
	public class MIPS_BLTZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Less Than Zero and Link
	public class MIPS_BLTZAL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < 0) {
				context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			}
		}
	}

	// Branch on Less Than Zero and Link Likely
	public class MIPS_BLTZALL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < 0) {
				context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue() + 4);
			}
		}
	}

	// Branch on Less Than Zero Likely
	public class MIPS_BLTZL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((int)context.getRegisters()[context.getRS()].getValue() < 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} 
			else {
				context.getPC().setValue(context.getPC().getValue() + 4);
			}
		}
	}

	// Count Leading Ones in Word
	public class MIPS_CLO : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint count = 0;
			const uint mask = 0x1;
			uint value = context.getRegisters()[context.getRS()].getValue();
			for (int i=31; i>=0; i--) {
				if (((value >> i) & mask) == mask) {
					count++;
					break;
				}
			}
			context.getRegisters()[context.getRD()].setValue(count);
		}
	}

	// Count Leading Zeroes in Word
	public class MIPS_CLZ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint count = 0;
			const uint mask = 0x1;
			uint value = context.getRegisters()[context.getRS()].getValue();
			for (int i=31; i>=0; i--) {
				if (((value >> i) & mask) == 0x0) {
					count++;
					break;
				}
			}
			context.getRegisters()[context.getRD()].setValue(count);
		}
	}

	// Disable Interrupts
	public class MIPS_DI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			const uint STATUS_IE_MASK = 0x1;

			context.getRegisters()[context.getRT()].setValue(context.getCoprocessors()[0].getRegister(12,0));
			context.getCoprocessors()[0].setRegister(12,0, (context.getCoprocessors()[0].getRegister(12,0) & (~STATUS_IE_MASK)));
		}
	}

	// Enable Interrupts
	public class MIPS_EI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			const uint STATUS_IE_MASK = 0x1;

			context.getRegisters()[context.getRT()].setValue(context.getCoprocessors()[0].getRegister(12,0));
			context.getCoprocessors()[0].setRegister(12,0, (context.getCoprocessors()[0].getRegister(12,0) | STATUS_IE_MASK));
		}
	}

	// Extract Bit Field
	// FIXME: Not terribly confident about this
	// shamt = position
	// rd = size-1
	public class MIPS_EXT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint msbd = (uint)context.getShamt();
			int lsb = (int)context.getRegisters()[context.getRD()].getValue();
			if ((lsb + msbd) > 31) {
				// UNPREDICTABLE
				return;
			}

			uint mask = ((uint)0x1 << lsb);
			uint temp = 0x0;
			uint res = 0x0;
			int shift = lsb;
			uint rs = context.getRegisters()[context.getRS()].getValue();

			for (int count=0; count != msbd; count++) {
				temp = mask & rs;
				temp >>= shift;
				shift--;
				mask <<= 1;
				res |= temp;
			}
			Console.WriteLine("DEBUG: EXT Result = 0x{0:X}", res);
			context.getRegisters()[context.getRT()].setValue(res);
		}
	}

	// Insert Bit Field
	// FIXME: Not terribly confident about this, same as EXT
	public class MIPS_INS : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			uint msbd = (uint)context.getShamt();
			int lsb = (int)context.getRegisters()[context.getRD()].getValue();
			if ((lsb + msbd) > 31) {
				// UNPREDICTABLE
				return;
			}

			uint mask = ((uint)0x1 << lsb);
			uint temp = 0x0;
			uint res = 0x0;
			int shift = lsb;
			uint rs = context.getRegisters()[context.getRS()].getValue();

			for (int count=0; count != msbd; count++) {
				temp = mask & rs;
				temp >>= shift;
				shift--;
				mask <<= 1;
				res |= temp;
			}

			res <<= (int)msbd;
			res |= context.getRegisters()[context.getRT()].getValue();
			context.getRegisters()[context.getRT()].setValue(res);
		}
	}
}