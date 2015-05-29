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
	public abstract class MIPS_Instruction
	{
		// Execution routine
		public abstract void execute(ref MIPS_InstructionContext context);

		// Sign Extend 32bits
		public Int32 signExtend32(UInt32 value) {
			return (Int32)value;
		}

		// Sign Extend 16bits
		public Int16 signExtend16(UInt16 value) {
			return (Int16)value;
		}

		// Zero Extend 32bits
		public UInt32 zeroExtend32(UInt16 value) {
			return (UInt32)value;
		}

		// Zero Extend 16bits does nothing??
		public UInt16 zeroExtend16(UInt16 value) {
			return (UInt16)value;
		}

		// Calculate Branch Transfer Address (PC-Relative)
		public UInt32 calculateBTA(UInt32 branchaddr, UInt16 imm) {
			Int16 branchoffset = signExtend16(imm);
			branchoffset <<= 2;
			branchaddr += (UInt32)signExtend32((UInt32)branchoffset);
			return branchaddr;
		}

		// Calculate Effective Target Address (PC-Region)
		public UInt32 calculateETA(UInt32 branchaddr, UInt32 jimm) {
			jimm <<= 2;
			UInt32 targetaddr = (branchaddr & 0xF0000000);
			targetaddr |= jimm;
			return targetaddr;
		}
	}

	// Shift Word Left Logical
	public class MIPS_SLL : MIPS_Instruction 
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 result = context.getRegisters()[context.getRT()].getValue();
			result <<= context.getShamt();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Logical
	public class MIPS_SRL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 result = context.getRegisters()[context.getRT()].getValue();
			result >>= context.getShamt();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Arithmetic
	public class MIPS_SRA : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 rt = context.getRegisters()[context.getRT()].getValue();
			Int32 result = (Int32)rt >> context.getShamt();
			context.getRegisters()[context.getRD()].setValue((UInt32)result);
		}
	}

	// Shift Word Left Logical Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SLLV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 result = context.getRegisters()[context.getRT()].getValue();
			result <<= (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Logical Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SRLV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 result = context.getRegisters()[context.getRT()].getValue();
			result >>= (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Shift Word Right Arithmetic Variable
	// Note: C# Does not allow right side UInt operator! Possible major bug.
	public class MIPS_SRAV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 rt = context.getRegisters()[context.getRT()].getValue();
			Int32 result = (Int32)rt >> (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRD()].setValue((UInt32)result);
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
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_BREAK : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
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
			Int64 result = context.getRegisters()[context.getRS()].getValue();
			result *= (Int32)context.getRegisters()[context.getRT()].getValue();
			context.getLO().setValue((UInt32)(result & 0x00000000FFFFFFFF));
			context.getHI().setValue((UInt32)(result >> 32));
		}
	}

	// Multiply Word Unsigned
	public class MIPS_MULTU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt64 result = context.getRegisters()[context.getRS()].getValue();
			result *= context.getRegisters()[context.getRT()].getValue();
			context.getLO().setValue((UInt32)(result & 0x00000000FFFFFFFF));
			context.getHI().setValue((UInt32)(result >> 32));
		}
	}

	// Divide Word
	public class MIPS_DIV : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				Int32 rs = (Int32)context.getRegisters()[context.getRS()].getValue();
				Int32 rt = (Int32)context.getRegisters()[context.getRT()].getValue();

				context.getLO().setValue((UInt32)(rs/rt));
				context.getHI().setValue((UInt32)(rs%rt));
			}
			catch (Exception e) {
				// No arithmetic exception occurs under any circumstances
			}
		}
	}

	// Divide Unsigned Word
	public class MIPS_DIVU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				UInt32 rs = context.getRegisters()[context.getRS()].getValue();
				UInt32 rt = context.getRegisters()[context.getRT()].getValue();

				context.getLO().setValue((rs/rt));
				context.getHI().setValue((rs%rt));
			}
			catch (Exception e) {
				// No arithmetic exception occurs under any circumstances
			}
		}
	}

	// Signed Addition
	public class MIPS_ADD : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				context.getRegisters()[context.getRD()].setValue((UInt32)checked(
					base.signExtend32(context.getRegisters()[context.getRS()].getValue()) 
					+ 
					base.signExtend32(context.getRegisters()[context.getRT()].getValue())));
			}
			catch (System.OverflowException) {
                // Rerun unchecked
                // FIXME: Is this correct behavior?
                context.getRegisters()[context.getRD()].setValue((UInt32)unchecked(
                    base.signExtend32(context.getRegisters()[context.getRS()].getValue())
                    +
                    base.signExtend32(context.getRegisters()[context.getRT()].getValue())));

                // Trigger exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.OVF);
			}
		}
	}

	// Unsigned Addition
	public class MIPS_ADDU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(
				context.getRegisters()[context.getRS()].getValue()
				+ 
				context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Subtract Word
	public class MIPS_SUB : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			try {
				context.getRegisters()[context.getRD()].setValue((UInt32)checked(
					base.signExtend32(context.getRegisters()[context.getRS()].getValue()) 
					- 
					base.signExtend32(context.getRegisters()[context.getRT()].getValue())));
			}
			catch (System.OverflowException) {
                // Rerun unchecked
                // FIXME: Is this correct behavior?
                context.getRegisters()[context.getRD()].setValue((UInt32)unchecked(
                    base.signExtend32(context.getRegisters()[context.getRS()].getValue())
                    -
                    base.signExtend32(context.getRegisters()[context.getRT()].getValue())));

                // Trigger Exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.INT);
			}
		}
	}

	// Subtract Word Unsigned
	public class MIPS_SUBU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRD()].setValue(
				context.getRegisters()[context.getRS()].getValue()
				- 
				context.getRegisters()[context.getRT()].getValue());
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
			UInt32 result = ~(context.getRegisters()[context.getRS()].getValue() | context.getRegisters()[context.getRT()].getValue());
			context.getRegisters()[context.getRD()].setValue(result);
		}
	}

	// Set on Less Than
	public class MIPS_SLT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((Int32)context.getRegisters()[context.getRS()].getValue() < (Int32)context.getRegisters()[context.getRT()].getValue()) {
				context.getRegisters()[context.getRD()].setValue(1);
			} else {
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
			} else {
				context.getRegisters()[context.getRD()].setValue(0);
			}
		}
	}

	public class MIPS_TGE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_TGEU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_TLT : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_TLTU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_TEQ : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_TNE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// REGIMM Branch Superclass
	public class MIPS_REGIMM : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			switch (context.getRegisters()[context.getRT()].getValue()) {
				// Branch on Greater Than or Equal to Zero and Link
				case 0x11: {
					if (context.getRegisters()[context.getRS()].getValue() >= 0) {
						context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					}
					break;
				}
				// Branch on Greater Than or Equal to Zero
				case 0x1: {
					if (context.getRegisters()[context.getRS()].getValue() >= 0) {
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					}
					break;
				}
				// Branch on Greater Than or Equal to Zero and Link Likely
				case 0x13: {
					if (context.getRegisters()[context.getRS()].getValue() >= 0) {
						context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					} else {
						context.getPC().setValue(context.getPC().getValue()+4);
					}
					break;
				}
				// Branch on Greater Than or Equal to Zero Likely
				case 0x3: {
					if (context.getRegisters()[context.getRS()].getValue() >= 0) {
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					} else {
						context.getPC().setValue(context.getPC().getValue()+4);
					}
					break;
				}
				// Branch on Less Than Zero
				case 0x0: {
					if (context.getRegisters()[context.getRS()].getValue() < 0) {
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					}
					break;
				}
				// Branch on Less Than Zero and Link
				case 0x10: {
					if (context.getRegisters()[context.getRS()].getValue() < 0) {
						context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					}
					break;
				}
				// Branch on Less Than Zero and Link Likely
				case 0x12: {
					if (context.getRegisters()[context.getRS()].getValue() < 0) {
						context.getRegisters()[31].setValue(context.getPC().getValue() + 4);
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					} else {
						context.getPC().setValue(context.getPC().getValue()+4);
					}
					break;
				}
				// Branch on Less Than Zero Likely
				case 0x2: {
					if (context.getRegisters()[context.getRS()].getValue() < 0) {
						context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
						context.setBranchDelay(true);
					} else {
						context.getPC().setValue(context.getPC().getValue()+4);
					}
					break;
				}
				default: {
					throw new MIPS_Exception(MIPS_Exception.ExceptionCode.CPU);
				}
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
			if (context.getRegisters()[context.getRS()].getValue() <= 0) {
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
				context.getRegisters()[context.getRT()].setValue((UInt32)checked(
					base.signExtend32(context.getRegisters()[context.getRS()].getValue()) 
					+ 
					base.signExtend32(context.getImm())));
			}
			catch (System.OverflowException) {
                // Rerun unchecked
                // FIXME: Is this correct behavior?
                context.getRegisters()[context.getRT()].setValue((UInt32)unchecked(
                    base.signExtend32(context.getRegisters()[context.getRS()].getValue())
                    +
                    base.signExtend32(context.getImm())));

                // Trigger Exception
				throw new MIPS_Exception(MIPS_Exception.ExceptionCode.OVF);
			}
		}
	}

	// Add Immediate Unsigned Word
	public class MIPS_ADDIU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt32 result = context.getRegisters()[context.getRS()].getValue();
			result += (UInt32)base.signExtend16(context.getImm());
			context.getRegisters()[context.getRT()].setValue(result);
            // FIXME: Overflow
		}
	}

	// Set on Less Than Immediate
	public class MIPS_SLTI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if ((Int32)context.getRegisters()[context.getRS()].getValue() < (Int16)base.signExtend16(context.getImm())) {
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
			if (context.getRegisters()[context.getRS()].getValue() < (UInt16)base.signExtend16(context.getImm())) {
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
				base.zeroExtend32(context.getImm()));
		}
	}

	// Or Immediate
	public class MIPS_ORI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue(
				context.getRegisters()[context.getRS()].getValue() 
				| 
				base.zeroExtend32(context.getImm()));
		}
	}

	// Exclusive OR Immediate
	public class MIPS_XORI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue(
				context.getRegisters()[context.getRS()].getValue() 
				^ 
				base.zeroExtend32(context.getImm()));
		}
	}

	// Load Upper Immediate
	public class MIPS_LUI : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			context.getRegisters()[context.getRT()].setValue((UInt32)((UInt32)context.getImm() << 16));
		}
	}

	public class MIPS_COP0 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			const byte selmask = 0x7;
			byte sel = (byte)(base.zeroExtend16(context.getImm())&selmask);
			switch (context.getRS()) {
				// Move from Coprocessor 0
				// FIXME: Requires sel field to be zero.... but who cares?
				// FIXME: Throws Coprocessor Unusuable, Reserved Instruction
				case 0x0: {
					byte cpcregister = (byte)context.getRegisters()[context.getRD()].getValue();
					UInt32 value = context.getCoprocessors()[0].getRegister(cpcregister, sel);
					context.getRegisters()[context.getRT()].setValue(value);
					break;
				}
				// Move to Coprocessor 0
				// FIXME: Requires sel field to be zero.... but who cares?
				// FIXME: Throws Coprocessor Unusuable, Reserved Instruction
				case 0x4: {
					UInt32 value = context.getRegisters()[context.getRT()].getValue();
					context.getCoprocessors()[0].setRegister(context.getRD(), sel, value);
					break;
				}
                default: {
                    throw new MIPS_Exception(MIPS_Exception.ExceptionCode.CPU);
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
			} else {
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
			} else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	// Branch on Less Than or Equal to Zero Likely
	public class MIPS_BLEZL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			if (context.getRegisters()[context.getRS()].getValue() <= 0) {
				context.getBranchTarget().setValue(base.calculateBTA(context.getPC().getValue(), context.getImm()));
				context.setBranchDelay(true);
			} else {
				context.getPC().setValue(context.getPC().getValue()+4);
			}
		}
	}

	public class MIPS_SPECIAL2 : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
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
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadByte((UInt32)vAddr));
		}
	}

	// Load Halfword
	public class MIPS_LH : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadHalf((UInt32)vAddr));
		}
	}

	public class MIPS_LWL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Load Word 
	public class MIPS_LW : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getRegisters()[context.getRT()].setValue(context.getMemory().ReadWord((UInt32)vAddr));
		}
	}

	// Load Byte Unsigned
	public class MIPS_LBU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt16 offset = zeroExtend16(context.getImm());
			UInt32 address = context.getRegisters()[context.getRD()].getValue();
			address += (UInt32)offset;
			UInt32 value = context.getMemory().ReadByte(address);
			context.getRegisters()[context.getRT()].setValue(value);
		}
	}

	// Load Halfword Unsigned
	public class MIPS_LHU : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			UInt16 offset = zeroExtend16(context.getImm());
			UInt32 address = context.getRegisters()[context.getRD()].getValue();
			address += (UInt32)offset;
			UInt32 value = context.getMemory().ReadHalf(address);
			context.getRegisters()[context.getRT()].setValue(value);
		}
	}

	public class MIPS_LWR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Store Byte
	public class MIPS_SB : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreByte((UInt32)vAddr, (byte)context.getRegisters()[context.getRT()].getValue());
		}
	}

	// Store Halfword
	public class MIPS_SH : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreHalf((UInt32)vAddr, (UInt16)context.getRegisters()[context.getRT()].getValue());
		}
	}

	public class MIPS_SWL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	// Store Word
	public class MIPS_SW : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
			Int32 vAddr = base.signExtend16(context.getImm());
			vAddr += (Int32)context.getRegisters()[context.getRS()].getValue();
			context.getMemory().StoreWord((UInt32)vAddr, context.getRegisters()[context.getRT()].getValue());
		}
	}

	public class MIPS_SWR : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_CACHE : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_LL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
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

	public class MIPS_SC : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}

	public class MIPS_SWCL : MIPS_Instruction
	{
		public override void execute(ref MIPS_InstructionContext context) {
            throw new MIPS_Exception(MIPS_Exception.ExceptionCode.UNIMPLEMENTED);
		}
	}
}