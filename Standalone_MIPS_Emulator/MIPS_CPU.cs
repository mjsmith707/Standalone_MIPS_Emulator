/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

// Global Verbose Debugging
#define MIPS_DEBUG_CPU

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;

// MIPS32R2 Instruction Formats
// R-Type
// opcode rs	rt	  rd	shamt funct
// 000000 00000 00000 00000 00000 000000
// Mask
// opcode 	0xFC000000
// rs		0x3E00000
// rt		0x1F0000
// rd		0xF800
// shamt	0x7C0
// funct	0x3F
//
// I-Type
// opcode rs	rt	  imm
// 000000 00000 00000 0000000000000000
// Mask
// opcode 	0xFC000000
// rs		0x3E00000
// rt		0x1F0000
// imm		0xFFFF
//
// J-Type
// opcode jimm
// 000000 0000000000000000000000000
// Mask
// opcode 	0xFC000000
// jimm		0x3FFFFFF

namespace Standalone_MIPS_Emulator {
	public class MIPS_CPU {
		// Decoder Constants
		private const UInt32 OPCODEMASK = 0xFC000000;
		private const byte OPCODESHIFT 	= 26;
		private const UInt32 RSMASK 	= 0x03E00000;
		private const byte RSSHIFT 		= 21;
		private const UInt32 RTMASK 	= 0x001F0000;
		private const byte RTSHIFT 		= 16;
		private const UInt32 RDMASK 	= 0x0000F800;
		private const byte RDSHIFT 		= 11;
		private const UInt32 SHAMTMASK 	= 0x000007C0;
		private const byte SHAMTSHIFT 	= 6;
		private const UInt32 FUNCTMASK 	= 0x0000003F;
		private const byte FUNCTSHIFT 	= 0;
		private const UInt32 IMMMASK 	= 0x0000FFFF;
		private const byte IMMSHIFT 	= 0;
		private const UInt32 JIMMMASK 	= 0x03FFFFFF;
		private const byte JIMMSHIFT 	= 0;

		// CPU Components
		private MIPS_Coprocessor[] coprocessors;
		private MIPS_Register[] registerFile;
		private MIPS_Instruction[] funct;
		private MIPS_Instruction[] regimm;
		private MIPS_Instruction[] cop0;
		private MIPS_Instruction[] opcode;
		private MIPS_InstructionContext context;
		private MIPS_Register PC;
		private MIPS_Register IR;
		private MIPS_Register hi;
		private MIPS_Register lo;
		public UInt64 cyclecount;
		private MIPS_Memory mainMemory;
		private bool branch;
		private MIPS_Boolean branchDelay;
		private MIPS_Register branchTarget;

		// Interrupt/Exception Stuff
		private ConcurrentQueue<MIPS_Exception> interrupts;

		// Coprocessor Register Masks
		private const UInt32 STATUS_BEV_MASK = 0x400000;
		private const byte STATUS_BEV_SHIFT = 22;
		private const UInt32 STATUS_IM_MASK = 0x8000;
		private const byte STATUS_IM_SHIFT = 8;
		private const UInt32 STATUS_IE_MASK = 0x1;
		private const byte STATUS_IE_SHIFT = 0;
		private const UInt32 STATUS_EXL_MASK = 0x00000010;
		private const byte STATUS_EXL_SHIFT = 0x1;
		private const UInt32 STATUS_ERL_MASK = 0x00000100;
		private const byte STATUS_ERL_SHIFT = 0x2;
		private const UInt32 CAUSE_BD_MASK = 0x80000000;
		private const byte CAUSE_BD_SHIFT = 31;
		private const UInt32 CAUSE_CE_MASK = 0x30000000;
		private const byte CAUSE_CE_SHIFT = 28;
		private const UInt32 CAUSE_IV_MASK = 0x800000;
		private const byte CAUSE_IV_SHIFT = 23;
		private const UInt32 CAUSE_IP_MASK = 0x7C00;
		private const byte CAUSE_IP_SHIFT = 10;
		private const UInt32 CAUSE_IPRQ_MASK = 0x300;
		private const byte CAUSE_IPRQ_SHIFT = 8;
		private const UInt32 CAUSE_EXCCODE_MASK = 0x7C;
		private const byte CAUSE_EXCCODE_SHIFT = 2;


		// Default Constructor
		public MIPS_CPU() {
			cyclecount = 0;

			// Initialize Special Registers
			PC = new MIPS_Register();
			IR = new MIPS_Register();
			hi = new MIPS_Register();
			lo = new MIPS_Register();

			// Branch/Delay Slot Controls
			branch = false;
			branchDelay = new MIPS_Boolean(false);
			branchTarget = new MIPS_Register();

			// Initialize Coprocessors
			coprocessors = new MIPS_Coprocessor[4];
			coprocessors[0] = new MIPS_CPC0();

			// Initialize Register File
			registerFile = new MIPS_Register[32];

			// Initialize Interrupt Queue
			interrupts = new ConcurrentQueue<MIPS_Exception>();

			// Mute r0
			registerFile [0] = new MIPS_Register(0, true);

			// Initialize Remaining Registers
			for (int i = 1; i < 32; i++) {
				registerFile[i] = new MIPS_Register();
			}

			// Initialize SP/FP Values (for testing purposes)
			registerFile[29].setValue(0x10010000);
			registerFile[30].setValue(0x10010000);

			// Initialize Memory
			mainMemory = new MIPS_Memory();

			// Initialize Instruction Context
			context = new MIPS_InstructionContext(ref mainMemory, ref registerFile, ref PC, ref hi, ref lo, ref branchTarget, ref branchDelay, ref coprocessors);

			// Initialize Instruction Set
			initialize_InstructionSet();
		}

		// MIPS32 Architecture For Programmers Volume II: The MIPS32 Instruction Set
		// Initialize Jump Tables
		private void initialize_InstructionSet() {
			// Register Encoded Instructions (Funct/SPECIAL)
			funct = new MIPS_Instruction[64];
			funct[0x00] = new MIPS_SLL();
			funct[0x02] = new MIPS_SRL();
			funct[0x03] = new MIPS_SRA();
			funct[0x04] = new MIPS_SLLV();
			funct[0x06] = new MIPS_SRLV();
			funct[0x07] = new MIPS_SRAV();
			funct[0x08] = new MIPS_JR();
			funct[0x09] = new MIPS_JALR();
			funct[0x0A] = new MIPS_MOVZ();
			funct[0x0B] = new MIPS_MOVN();
			funct[0x0C] = new MIPS_SYSCALL();
			funct[0x0D] = new MIPS_BREAK();
			funct[0x0F] = new MIPS_SYNC();
			funct[0x10] = new MIPS_MFHI();
			funct[0x11] = new MIPS_MTHI();
			funct[0x12] = new MIPS_MFLO();
			funct[0x13] = new MIPS_MTLO();
			funct[0x18] = new MIPS_MULT();
			funct[0x19] = new MIPS_MULTU();
			funct[0x1A] = new MIPS_DIV();
			funct[0x1B] = new MIPS_DIVU();
			funct[0x20] = new MIPS_ADD();
			funct[0x21] = new MIPS_ADDU();
			funct[0x22] = new MIPS_SUB();
			funct[0x23] = new MIPS_SUBU();
			funct[0x24] = new MIPS_AND();
			funct[0x25] = new MIPS_OR();
			funct[0x26] = new MIPS_XOR();
			funct[0x27] = new MIPS_NOR();
			funct[0x2A] = new MIPS_SLT();
			funct[0x2B] = new MIPS_SLTU();
			funct[0x30] = new MIPS_TGE();
			funct[0x31] = new MIPS_TGEU();
			funct[0x32] = new MIPS_TLT();
			funct[0x33] = new MIPS_TLTU();
			funct[0x34] = new MIPS_TEQ();
			funct[0x36] = new MIPS_TNE();

			// REGIMM Instructions
			regimm = new MIPS_Instruction[32];
			regimm[0x00] = new MIPS_BLTZ();
			regimm[0x01] = new MIPS_BGEZ();
			regimm[0x02] = new MIPS_BLTZL();
			regimm[0x03] = new MIPS_BGEZL();
			regimm[0x10] = new MIPS_BLTZAL();
			regimm[0x11] = new MIPS_BGEZAL();
			regimm[0x12] = new MIPS_BLTZALL();
			regimm[0x13] = new MIPS_BGEZALL();

			// COP0 Instructions
			cop0 = new MIPS_Instruction[32];
			cop0[0x00] = new MIPS_MFC0();
			cop0[0x04] = new MIPS_MTC0();

			// Immediate and J Instructions (Opcode)
			opcode = new MIPS_Instruction[64];
			opcode[0x02] = new MIPS_J();
			opcode[0x03] = new MIPS_JAL();
			opcode[0x04] = new MIPS_BEQ();
			opcode[0x05] = new MIPS_BNE();
			opcode[0x06] = new MIPS_BLEZ();
			opcode[0x07] = new MIPS_BGTZ();
			opcode[0x08] = new MIPS_ADDI();
			opcode[0x09] = new MIPS_ADDIU();
			opcode[0x0A] = new MIPS_SLTI();
			opcode[0x0B] = new MIPS_SLTIU();
			opcode[0x0C] = new MIPS_ANDI();
			opcode[0x0D] = new MIPS_ORI();
			opcode[0x0E] = new MIPS_XORI();
			opcode[0x0F] = new MIPS_LUI();
			opcode[0x11] = new MIPS_COP1();
			opcode[0x12] = new MIPS_COP2();
			opcode[0x14] = new MIPS_BEQL();
			opcode[0x15] = new MIPS_BNEL();
			opcode[0x16] = new MIPS_BLEZL();
			opcode[0x17] = new MIPS_BGTZL();
			opcode[0x1C] = new MIPS_SPECIAL2();
			opcode[0x1F] = new MIPS_RESERVED_INST();
			opcode[0x20] = new MIPS_LB();
			opcode[0x21] = new MIPS_LH();
			opcode[0x22] = new MIPS_LWL();
			opcode[0x23] = new MIPS_LW();
			opcode[0x24] = new MIPS_LBU();
			opcode[0x25] = new MIPS_LHU();
			opcode[0x26] = new MIPS_LWR();
			opcode[0x28] = new MIPS_SB();
			opcode[0x29] = new MIPS_SH();
			opcode[0x2A] = new MIPS_SWL();
			opcode[0x2B] = new MIPS_SW();
			opcode[0x2E] = new MIPS_SWR();
			opcode[0x2F] = new MIPS_CACHE();
			opcode[0x30] = new MIPS_LL();
			opcode[0x3D] = new MIPS_SDC1();
			opcode[0x3E] = new MIPS_SDC2();
			opcode[0x31] = new MIPS_LWCL();
			opcode[0x33] = new MIPS_PREF();
			opcode[0x38] = new MIPS_SC();
			opcode[0x39] = new MIPS_SWCL();
		}

		// FDX Loop
		// Needs execution cap
		public void start() {
			while (true) {
				try {
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("");
						Console.WriteLine("Cycle" + "    = {0}", cyclecount);
						printGPRRegisters();
						printCPC0Registers();
					#endif
					
					// Service a pending interrupt (if any)
					serviceints();

					// Fetch next instruction
					fetch();

					// Decode instruction
					decode();

					// Execute instruction
					execute();

					// Update cycle counter
					// This is _not_ a cop0 performance counter
					cyclecount++;
				}
				catch (MIPS_Exception e) {
					// Catch Internal Interrupts/Exceptions
					// Enqueue immediately
					interrupts.Enqueue(e);
				}
				catch (Exception e) {
					// Simulation Error
					// Stop FDX Loop
					Console.WriteLine("Exception: " + e.Message);
					return;
				}
			}
		}

		// Service enqueued interrupts/exceptions
		// Mainly update coprocessor 0 registers
		// and redirect program control to ISR
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void serviceints() {
			MIPS_Exception e;
			if (interrupts.TryDequeue(out e)) {
				#if (MIPS_DEBUG_CPU)
					Console.WriteLine("=== service interrupts ===");
					Console.WriteLine(e.getCode());
				#endif

				// Interrupt Handler
				if (e.getCode() == MIPS_Exception.ExceptionCode.INT) {
					// Test if Status Register Global Interrupt Enable is on
					if (readBits(coprocessors [0].getRegister(12, 0), STATUS_IE_MASK, STATUS_IE_SHIFT) == 0) {
						// Interrupts disabled
						// FIXME: Test for unmaskable
						return;
						// Test if Status Register Interrupt Enable is on
					}
					else if (readBits(coprocessors [0].getRegister(12, 0), STATUS_IM_MASK, ((Int32)e.getIntNumber () - 1)) == 0) {
						// Interrupt disabled
						// FIXME: Test for unmaskable
						return;
						// Test if EXL and ERL Bits = 0
					}
					else if ((readBits(coprocessors [0].getRegister(12, 0), STATUS_EXL_MASK, STATUS_EXL_SHIFT) == 0x1)
					         && (readBits(coprocessors [0].getRegister(12, 0), STATUS_ERL_MASK, STATUS_ERL_SHIFT) == 0x1)) {
						return;
					}
				}

				// Combined Interrupt/Exception Handler
				// mips vol3 pg 24

				UInt32 vectorOffset = 0;
				// if StatusEXL = 0
				if (readBits(coprocessors[0].getRegister(12, 0), STATUS_EXL_MASK, STATUS_EXL_SHIFT) == 0) {
					if (branchDelay.getValue()) {
						// EPC = PC-8
						coprocessors[0].setRegisterHW(30, 0, PC.getValue()-8);
						// CauseBD = 1
						coprocessors[0].setRegisterHW(13, 0, (coprocessors[0].getRegister(13, 0) | CAUSE_BD_MASK));
					}
					else {
						// EPC = PC
						coprocessors[0].setRegisterHW(30, 0, PC.getValue()-4);
						// CauseBD = 0
						coprocessors[0].setRegisterHW(13, 0, (coprocessors[0].getRegister(13, 0) & (~CAUSE_BD_MASK)));
					}
					// TLBRefill Exception
					if (e.getCode() == MIPS_Exception.ExceptionCode.TLBL || e.getCode() == MIPS_Exception.ExceptionCode.TLBS) {
						vectorOffset = 0;
					}
					// Interrupt Vector
					else if ((e.getCode() == MIPS_Exception.ExceptionCode.INT) && (readBits(coprocessors[0].getRegister(13, 0), CAUSE_IV_MASK, CAUSE_IV_SHIFT) == 0x1)) {
						vectorOffset = 0x200;
					}
					// General Offset
					else {
						vectorOffset = 0x180;
					}
				}
				// General Offset
				else {
					vectorOffset = 0x180;

					// Not specifically stated in manual
					// but what would happen if EPC is empty?
					// EPC = PC
					coprocessors[0].setRegisterHW(30, 0, PC.getValue()-4);
				}

				// CauseCE = ????
				// If we had floating-point I suppose this would be different than 0
				coprocessors[0].setRegisterHW(13, 0, (coprocessors[0].getRegister(13, 0) & (~CAUSE_CE_MASK)));

				// CauseExcCode = ExceptionType
				coprocessors[0].setRegisterHW(13, 0, (coprocessors[0].getRegister(13, 0) | (CAUSE_EXCCODE_MASK & (UInt32)e.getCode())));

				// StatusEXL = 1
				coprocessors[0].setRegisterHW(12, 0, (coprocessors[0].getRegister(12, 0) | STATUS_EXL_MASK));

				// Update Program Counter to ISR
				if (readBits(coprocessors[0].getRegister(12, 0), STATUS_BEV_MASK, STATUS_BEV_SHIFT) == 1) {
					PC.setValue(0xBFC00200 + vectorOffset);
				}
				else {
					PC.setValue(0x80000000 + vectorOffset);
				}

				// Reset Branch Controls
				branch = false;
				branchDelay.setValue(false);

				// Specific Exception State Modifications
				switch (e.getCode()) {
				case MIPS_Exception.ExceptionCode.INT:
					break;
				case MIPS_Exception.ExceptionCode.MOD:
					break;
				case MIPS_Exception.ExceptionCode.TLBL:
					break;
				case MIPS_Exception.ExceptionCode.TLBS:
					break;
				case MIPS_Exception.ExceptionCode.ADEL:
					break;
				case MIPS_Exception.ExceptionCode.ADES:
					break;
				case MIPS_Exception.ExceptionCode.IBE:
					break;
				case MIPS_Exception.ExceptionCode.DBUS:
					break;
				case MIPS_Exception.ExceptionCode.SYSCALL:
					break;
				case MIPS_Exception.ExceptionCode.BKPT:
					break;
				case MIPS_Exception.ExceptionCode.RI:
					break;
				case MIPS_Exception.ExceptionCode.CPU:
					break;
				case MIPS_Exception.ExceptionCode.OVF:
					break;
				case MIPS_Exception.ExceptionCode.TRAP:
					break;
				case MIPS_Exception.ExceptionCode.Reserved14:
					break;
				case MIPS_Exception.ExceptionCode.FPE:
					break;
				case MIPS_Exception.ExceptionCode.Reserved16:
					break;
				case MIPS_Exception.ExceptionCode.Reserved17:
					break;
				case MIPS_Exception.ExceptionCode.C2E:
					break;
				case MIPS_Exception.ExceptionCode.Reserved19:
					break;
				case MIPS_Exception.ExceptionCode.Reserved20:
					break;
				case MIPS_Exception.ExceptionCode.Reserved21:
					break;
				case MIPS_Exception.ExceptionCode.MDMX:
					break;
				case MIPS_Exception.ExceptionCode.WATCH:
					break;
				case MIPS_Exception.ExceptionCode.MCheck:
					break;
				case MIPS_Exception.ExceptionCode.Reserved25:
					break;
				case MIPS_Exception.ExceptionCode.Reserved26:
					break;
				case MIPS_Exception.ExceptionCode.Reserved27:
					break;
				case MIPS_Exception.ExceptionCode.Reserved28:
					break;
				case MIPS_Exception.ExceptionCode.Reserved29:
					break;
				case MIPS_Exception.ExceptionCode.CacheErr:
					break;
				case MIPS_Exception.ExceptionCode.Reserved31:
					break;
				}
			}
			else {
				return;
			}
		}

		// Fetch next instruction or execute branch
		// During execution PC points to next instruction, not current.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void fetch()
		{
			// Instruction in branch delay slot was executed.
			// Now execute the branch itself.
			if (branch) {
				// Reset Branch Controls
				branch = false;
				branchDelay.setValue(false);

				// Update Program Counter to Branch Target
				this.PC.setValue(branchTarget.getValue());
				// Update Instruction Register
				this.IR.setValue(mainMemory.ReadWord(PC.getValue()));

				#if (MIPS_DEBUG_CPU)
					Console.WriteLine("==== Fetch ====");
					Console.WriteLine("PC     = 0x{0:X}", PC.getValue());
					Console.WriteLine("IR     = 0x{0:X}", IR.getValue());
				#endif
				
				// Advance Program Counter
				this.PC.setValue(PC.getValue() + 4);
			}
			else {
				// Update Instruction Register
				this.IR.setValue(mainMemory.ReadWord(PC.getValue()));

				#if (MIPS_DEBUG_CPU)
					Console.WriteLine("==== Fetch ====");
					Console.WriteLine("PC     = 0x{0:X}", PC.getValue());
					Console.WriteLine("IR     = 0x{0:X}", IR.getValue());
				#endif
				
				// Advance Program Counter
				this.PC.setValue(PC.getValue() + 4);
			}

			// Branch Instruction was executed.
			// Next time around, execute the branch.
			if (branchDelay.getValue()) {
				branch = true;
			}
		}

		// MIPS32 Decoder
		// Decoded fields stored into context.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void decode() {
			// Slice up current value in Instruction Register
			byte opcodec 	= (byte)((IR.getValue() & OPCODEMASK) >> OPCODESHIFT);
			byte rs 		= (byte)((IR.getValue() & RSMASK) >> RSSHIFT);
			byte rt 		= (byte)((IR.getValue() & RTMASK) >> RTSHIFT);
			byte rd 		= (byte)((IR.getValue() & RDMASK) >> RDSHIFT);
			byte shamt 		= (byte)((IR.getValue() & SHAMTMASK) >> SHAMTSHIFT);
			byte functc 	= (byte)((IR.getValue() & FUNCTMASK) >> FUNCTSHIFT);
			UInt16 imm 		= (UInt16)((IR.getValue() & IMMMASK) >> IMMSHIFT);
			UInt32 jimm 	= (UInt32)((IR.getValue() & JIMMMASK) >> JIMMSHIFT);

			// Store into the Instruction Context
			context.setContext(opcodec, rs, rt, rd, shamt, functc, imm, jimm);

			#if (MIPS_DEBUG_CPU)
				Console.WriteLine("==== Decode ====");
				Console.WriteLine("opcode = 0x{0:X}", opcodec);
				Console.WriteLine("rs     = 0x{0:X}", rs);
				Console.WriteLine("rt     = 0x{0:X}", rt);
				Console.WriteLine("rd     = 0x{0:X}", rd);
				Console.WriteLine("shamt  = 0x{0:X}", shamt);
				Console.WriteLine("functc = 0x{0:X}", functc);
				Console.WriteLine("imm    = 0x{0:X}", imm);
				Console.WriteLine("jimm   = 0x{0:X}", jimm);
			#endif
		}

		// Execution Caller
		// Functions split into multiple tables for sanity
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void execute() {
			#if (MIPS_DEBUG_CPU)
				Console.WriteLine("==== Execute ====");
				Console.WriteLine("=Disassembly=");
				if (branchDelay.getValue()) {
					Console.WriteLine("-Delay Slot-");
				}
			#endif
			
			switch (context.getOpcode()) {
				// Funct/SPECIAL Instructions
				case 0x0: {
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("SPECIAL      INST      RS RT RD IMM JIMM SHAMT");
						Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", funct[context.getFunct()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
					#endif

					// Execute Funct
					funct[context.getFunct()].execute(ref context);
					break;
				}
				// REGIMM Instructions
				case 0x1: {
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("REGIMM       INST      RS RT RD IMM JIMM SHAMT");
						Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", regimm[context.getRT()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
					#endif

					// Execute REGIMM
					regimm[context.getRT()].execute(ref context);
					break;
				}
				// COP0 Instructions
				case 0x10: {
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("COP0         INST      RS RT RD IMM JIMM SHAMT");
						Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", cop0[context.getRS()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
					#endif

					// Execute COP0
					cop0[context.getRS()].execute(ref context);
					break;
				}
				// Opcode Format
				default: {
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("OPCODE       INST      RS RT RD IMM JIMM SHAMT");
						Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", opcode[context.getOpcode()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
					#endif

					// Execute Opcode
					opcode[context.getOpcode()].execute(ref context);
					break;
				}
			}
		}

		// Convenience Function
		// Returns masked bits downshifted from a value (i.e. register)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private UInt32 readBits(UInt32 value, UInt32 mask, byte shift) {
			return (UInt32)((value & mask) >> shift);
		}

		// Convenience Function Integer Edition
		// Returns masked bits downshifted from a value (i.e. register)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private UInt32 readBits(UInt32 value, UInt32 mask, Int32 shift) {
			return (UInt32)((value & mask) >> shift);
		}

		// Convenience Function
		// Write bits
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private UInt32 writeBits(UInt32 dst, UInt32 src, byte shift) {
			return (UInt32)((dst) | (src << shift));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool inKernelMode() {
			return (readBits(coprocessors [0].getRegister (12, 0), STATUS_EXL_MASK, STATUS_EXL_SHIFT) == 0x1)
			       || (readBits(coprocessors [0].getRegister (12, 0), STATUS_ERL_MASK, STATUS_ERL_SHIFT) == 0x1);
		}

		// Inserts given word at address in memory
		public void loadText(UInt32 address, UInt32 word) {
			mainMemory.StoreWord(address, word);
		}

		// Reads bare metal binary file into memory at address
		public void loadFile(UInt32 address, String filename) {
			try {
				using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open))) {
					uint count = 0;
					byte[] array = new byte[4];
					for (uint i=0; i<reader.BaseStream.Length; i++) {
						array[count] = reader.ReadByte();
						count++;
						if (count == 4) {
							UInt32 word = byteArrayToUInt32(array);
							
							#if (MIPS_DEBUG_CPU)
								Console.WriteLine("address: 0x{0:X}      = 0x{1:X}", address, word);
							#endif
							
							loadText(address, word);
							address += 4;
							count = 0;
						}
					}
				}
				#if (MIPS_DEBUG_CPU)
					Console.WriteLine("File Loaded");
				#endif
			} catch (Exception e) {
				Console.WriteLine("Error: " + e.Message);
			}
		}

		// ELFSharp-based loader
		// Only does naive loading so far
		// i.e. no relocation
		public void elfLoader(String filename) {
			var elfloader = ELFSharp.ELF.ELFReader.Load<uint>(filename);
			foreach (var header in elfloader.Sections) {
				Console.WriteLine(header);
			}

			//var textseg = elfloader.GetSection(".text");
			var address = elfloader.EntryPoint;

			// Set Program Counter
			PC.setValue(address);

			#if (MIPS_DEBUG_CPU)
				Console.WriteLine("ELF Entry Point: 0x{0:X}", address);
			#endif
			
			// Load all Prog Sections
			foreach (var section in elfloader.GetSections<ELFSharp.ELF.Sections.ProgBitsSection<uint>>()) {
				if (section.LoadAddress == 0) {
					continue;
				}
				else {
					address = section.LoadAddress;
					
					#if (MIPS_DEBUG_CPU)
						Console.WriteLine("Section: {0}         0x{1:X}", section.Name, address);
					#endif
					
					uint count = 0;
					byte[] array = new byte[4];

					foreach (var inst in section.GetContents()) {
						array[count] = inst;
						count++;
						if (count == 4) {
							UInt32 word = byteArrayToUInt32(array);
							loadText(address, word);
							
							#if (MIPS_DEBUG_CPU)
								//Console.WriteLine("address: 0x{0:X}      = 0x{1:X}", address, word);
							#endif
							
							address += 4;
							count = 0;
						}
					}
				}
			}
		}

		// No union support so here we are.
		// Joins 4 byte array into UInt32
		private UInt32 byteArrayToUInt32(byte[] array) {
			UInt32 word = (UInt32)array[0] << 24;
			word |= (UInt32)array[1] << 16;
			word |= (UInt32)array[2] << 8;
			word |= (UInt32)array[3];
			return word;
		}

		// Print out all GPRs and HI/LO Registers sadistically
		public void printGPRRegisters() {
			Console.WriteLine("==== Register File ====");
			Console.WriteLine("$zero (r0):     = 0x{0:X}", registerFile[0].getValue());
			Console.WriteLine("$at   (r1):     = 0x{0:X}", registerFile[1].getValue());
			Console.WriteLine("$v0   (r2):     = 0x{0:X}", registerFile[2].getValue());
			Console.WriteLine("$v1   (r3):     = 0x{0:X}", registerFile[3].getValue());
			Console.WriteLine("$a0   (r4):     = 0x{0:X}", registerFile[4].getValue());
			Console.WriteLine("$a1   (r5):     = 0x{0:X}", registerFile[5].getValue());
			Console.WriteLine("$a2   (r6):     = 0x{0:X}", registerFile[6].getValue());
			Console.WriteLine("$a3   (r7):     = 0x{0:X}", registerFile[7].getValue());
			Console.WriteLine("$t0   (r8):     = 0x{0:X}", registerFile[8].getValue());
			Console.WriteLine("$t1   (r9):     = 0x{0:X}", registerFile[9].getValue());
			Console.WriteLine("$t2   (r10):    = 0x{0:X}", registerFile[10].getValue());
			Console.WriteLine("$t3   (r11):    = 0x{0:X}", registerFile[11].getValue());
			Console.WriteLine("$t4   (r12):    = 0x{0:X}", registerFile[12].getValue());
			Console.WriteLine("$t5   (r13):    = 0x{0:X}", registerFile[13].getValue());
			Console.WriteLine("$t6   (r14):    = 0x{0:X}", registerFile[14].getValue());
			Console.WriteLine("$t7   (r15):    = 0x{0:X}", registerFile[15].getValue());
			Console.WriteLine("$s0   (r16):    = 0x{0:X}", registerFile[16].getValue());
			Console.WriteLine("$s1   (r17):    = 0x{0:X}", registerFile[17].getValue());
			Console.WriteLine("$s2   (r18):    = 0x{0:X}", registerFile[18].getValue());
			Console.WriteLine("$s3   (r19):    = 0x{0:X}", registerFile[19].getValue());
			Console.WriteLine("$s4   (r20):    = 0x{0:X}", registerFile[20].getValue());
			Console.WriteLine("$s5   (r21):    = 0x{0:X}", registerFile[21].getValue());
			Console.WriteLine("$s6   (r22):    = 0x{0:X}", registerFile[22].getValue());
			Console.WriteLine("$s7   (r23):    = 0x{0:X}", registerFile[23].getValue());
			Console.WriteLine("$t8   (r24):    = 0x{0:X}", registerFile[24].getValue());
			Console.WriteLine("$t9   (r25):    = 0x{0:X}", registerFile[25].getValue());
			Console.WriteLine("$k0   (r26):    = 0x{0:X}", registerFile[26].getValue());
			Console.WriteLine("$k1   (r27):    = 0x{0:X}", registerFile[27].getValue());
			Console.WriteLine("$gp   (r28):    = 0x{0:X}", registerFile[28].getValue());
			Console.WriteLine("$sp   (r29):    = 0x{0:X}", registerFile[29].getValue());
			Console.WriteLine("$s8   (r30):    = 0x{0:X}", registerFile[30].getValue());
			Console.WriteLine("$ra   (r31):    = 0x{0:X}", registerFile[31].getValue());
			Console.WriteLine("$HI             = 0x{0:X}", hi.getValue());
			Console.WriteLine("$LO             = 0x{0:X}", lo.getValue());
		}

		// Print out Coprocessor 0 Registers sadistically
		public void printCPC0Registers() {
			Console.WriteLine("==== Coprocessor 0 Registers ====");
			Console.WriteLine("Index    (0,0):      = 0x{0:X}", coprocessors[0].getRegister(0,0));
			Console.WriteLine("Random   (1,0):      = 0x{0:X}", coprocessors[0].getRegister(1,0));
			Console.WriteLine("EntryLo0 (2,0):      = 0x{0:X}", coprocessors[0].getRegister(2,0));
			Console.WriteLine("EntryLo1 (3,0):      = 0x{0:X}", coprocessors[0].getRegister(3,0));
			Console.WriteLine("Context  (4,0):      = 0x{0:X}", coprocessors[0].getRegister(4,0));
			Console.WriteLine("PageMask (5,0):      = 0x{0:X}", coprocessors[0].getRegister(5,0));
			Console.WriteLine("Wired    (6,0):      = 0x{0:X}", coprocessors[0].getRegister(6,0));
			Console.WriteLine("RSVD     (7,0):      = 0x{0:X}", coprocessors[0].getRegister(7,0));
			Console.WriteLine("BadVAddr (8,0):      = 0x{0:X}", coprocessors[0].getRegister(8,0));
			Console.WriteLine("Count    (9,0):      = 0x{0:X}", coprocessors[0].getRegister(9,0));
			Console.WriteLine("RSVD     (9,6):      = 0x{0:X}", coprocessors[0].getRegister(9,6));
			Console.WriteLine("RSVD     (9,7):      = 0x{0:X}", coprocessors[0].getRegister(9,7));
			Console.WriteLine("EntryHi  (10,0):     = 0x{0:X}", coprocessors[0].getRegister(10,0));
			Console.WriteLine("Compare  (11,0):     = 0x{0:X}", coprocessors[0].getRegister(11,0));
			Console.WriteLine("RSVD     (11,6):     = 0x{0:X}", coprocessors[0].getRegister(11,6));
			Console.WriteLine("RSVD     (11,7):     = 0x{0:X}", coprocessors[0].getRegister(11,7));
			Console.WriteLine("Status   (12,0):     = 0x{0:X}", coprocessors[0].getRegister(12,0));
			Console.WriteLine("Cause    (13,0):     = 0x{0:X}", coprocessors[0].getRegister(13,0));
			Console.WriteLine("EPCR     (14,0):     = 0x{0:X}", coprocessors[0].getRegister(14,0));
			Console.WriteLine("ProcID   (15,0):     = 0x{0:X}", coprocessors[0].getRegister(15,0));
			Console.WriteLine("ConfReg0 (16,0):     = 0x{0:X}", coprocessors[0].getRegister(16,0));
			Console.WriteLine("ConfReg1 (16,1):     = 0x{0:X}", coprocessors[0].getRegister(16,1));
			Console.WriteLine("ConfReg2 (16,2):     = 0x{0:X}", coprocessors[0].getRegister(16,2));
			Console.WriteLine("ConfReg3 (16,3):     = 0x{0:X}", coprocessors[0].getRegister(16,3));
			Console.WriteLine("RSVD     (16,6):     = 0x{0:X}", coprocessors[0].getRegister(16,6));
			Console.WriteLine("RSVD     (16,7):     = 0x{0:X}", coprocessors[0].getRegister(16,7));
			Console.WriteLine("LLAddr   (17,0):     = 0x{0:X}", coprocessors[0].getRegister(17,0));
			Console.WriteLine("WatchLo  (18,0):     = 0x{0:X}", coprocessors[0].getRegister(18,0));
			Console.WriteLine("WatchHi  (19,0):     = 0x{0:X}", coprocessors[0].getRegister(19,0));
			Console.WriteLine("XContext (20,0):     = 0x{0:X}", coprocessors[0].getRegister(20,0));
			Console.WriteLine("RSVD     (21,0):     = 0x{0:X}", coprocessors[0].getRegister(21,0));
			Console.WriteLine("RSVD     (22,0):     = 0x{0:X}", coprocessors[0].getRegister(22,0));
			Console.WriteLine("EJTAG DB (23,0):     = 0x{0:X}", coprocessors[0].getRegister(23,0));
			Console.WriteLine("EJTAG DEPC(24,0):    = 0x{0:X}", coprocessors[0].getRegister(24,0));
			Console.WriteLine("Perf Ctr (25,0):     = 0x{0:X}", coprocessors[0].getRegister(25,0));
			Console.WriteLine("ErrCtl   (26,0):     = 0x{0:X}", coprocessors[0].getRegister(26,0));
			Console.WriteLine("CacheErr0(27,0):     = 0x{0:X}", coprocessors[0].getRegister(27,0));
			Console.WriteLine("CacheErr1(27,1):     = 0x{0:X}", coprocessors[0].getRegister(27,1));
			Console.WriteLine("CacheErr2(27,2):     = 0x{0:X}", coprocessors[0].getRegister(27,2));
			Console.WriteLine("CacheErr3(27,3):     = 0x{0:X}", coprocessors[0].getRegister(27,3));
			Console.WriteLine("TagLo    (28,0):     = 0x{0:X}", coprocessors[0].getRegister(28,0));
			Console.WriteLine("DataLo   (28,1):     = 0x{0:X}", coprocessors[0].getRegister(28,1));
			Console.WriteLine("TagHi    (29,0):     = 0x{0:X}", coprocessors[0].getRegister(29,0));
			Console.WriteLine("DataHi   (29,1):     = 0x{0:X}", coprocessors[0].getRegister(29,1));
			Console.WriteLine("ErrorEPC (30,0):     = 0x{0:X}", coprocessors[0].getRegister(30,0));
			Console.WriteLine("DESAVE   (31,0):     = 0x{0:X}", coprocessors[0].getRegister(31,0));
		}
	}
}