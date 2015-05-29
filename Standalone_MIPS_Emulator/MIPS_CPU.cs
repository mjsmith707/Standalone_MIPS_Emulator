/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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

namespace Standalone_MIPS_Emulator
{
	public class MIPS_CPU
	{
		// Global Verbose Debugging
        public const bool DEBUG_CPU = true;

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

		public MIPS_CPU()
		{
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
		private void initialize_InstructionSet() {
			// Register Encoded Instructions (AKA Funct/SPECIAL)
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

			// Immediate and J Instructions (Opcode)
			opcode = new MIPS_Instruction[64];
			opcode[0x01] = new MIPS_REGIMM();
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
			opcode[0x10] = new MIPS_COP0();
			opcode[0x11] = new MIPS_COP1();
			opcode[0x12] = new MIPS_COP2();
			opcode[0x14] = new MIPS_BEQL();
			opcode[0x15] = new MIPS_BNEL();
			opcode[0x16] = new MIPS_BLEZL();
			opcode[0x17] = new MIPS_BGTZL();
			opcode[0x1C] = new MIPS_SPECIAL2();
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
					if (DEBUG_CPU) {
						printRegisters();
                        printCPC0Registers();
                        Console.WriteLine("Cycle" + "    = {0}", cyclecount);
					}
                    /*
                    else if (cyclecount % 500000 == 0) {
                        Console.WriteLine("Cycle" + "    = {0}", cyclecount);
                    }
                    */

					serviceints();
					fetch();
					decode();
					execute();
					cyclecount++;
				}
                catch (MIPS_Exception e) {
					// Catch Internal Interrupts/Exceptions
					// Enqueue immediately
					interrupts.Enqueue(e);
                }
				catch (Exception e) {
					Console.WriteLine("Exception: " + e.Message);
                    break;
				}
			}
		}

		// Service enqueued interrupts/exceptions
		// Mainly update coprocessor 0 registers
		// and redirect program control to ISR
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void serviceints() {
			MIPS_Exception e;
			if (interrupts.TryDequeue(out e)) {
				Console.WriteLine("serviceint caught exception");
				Console.WriteLine(e.Message);
			} else {
				return;
			}
        }

		// Fetch next instruction or execute branch
		// During execution PC points to next instruction, not current.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void fetch()
		{
			if (branch) {
				branch = false;
				branchDelay.setValue(false);
				this.PC.setValue(branchTarget.getValue());
				this.IR.setValue(mainMemory.ReadWord(PC.getValue()));
				if (DEBUG_CPU) {
					Console.WriteLine("==== Fetch ====");
					Console.WriteLine("PC     = 0x{0:X}", PC.getValue ());
					Console.WriteLine("IR     = 0x{0:X}", IR.getValue());
				}
				this.PC.setValue(PC.getValue() + 4);
			}
			else {
				this.IR.setValue(mainMemory.ReadWord(PC.getValue()));
				if (DEBUG_CPU) {
					Console.WriteLine("==== Fetch ====");
					Console.WriteLine("PC     = 0x{0:X}", PC.getValue ());
					Console.WriteLine("IR     = 0x{0:X}", IR.getValue());
				}
				this.PC.setValue(PC.getValue() + 4);
			}
				
			if (branchDelay.getValue()) {
				branch = true;
			}
		}

		// MIPS32 Decoder
		// Decoded fields stored into context.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void decode() {
			byte opcodec 	= (byte)((IR.getValue() & OPCODEMASK) >> OPCODESHIFT);
			byte rs 		= (byte)((IR.getValue() & RSMASK) >> RSSHIFT);
			byte rt 		= (byte)((IR.getValue() & RTMASK) >> RTSHIFT);
			byte rd 		= (byte)((IR.getValue() & RDMASK) >> RDSHIFT);
			byte shamt 		= (byte)((IR.getValue() & SHAMTMASK) >> SHAMTSHIFT);
			byte functc 	= (byte)((IR.getValue() & FUNCTMASK) >> FUNCTSHIFT);
			UInt16 imm 		= (UInt16)((IR.getValue() & IMMMASK) >> IMMSHIFT);
			UInt32 jimm 	= (UInt32)((IR.getValue() & JIMMMASK) >> JIMMSHIFT);

			context.setContext(opcodec, rs, rt, rd, shamt, functc, imm, jimm);

			if (DEBUG_CPU) {
				Console.WriteLine("==== Decode ====");
				Console.WriteLine("opcode = 0x{0:X}", opcodec);
				Console.WriteLine("rs     = 0x{0:X}", rs);
				Console.WriteLine("rt     = 0x{0:X}", rt);
				Console.WriteLine("rd     = 0x{0:X}", rd);
				Console.WriteLine("shamt  = 0x{0:X}", shamt);
				Console.WriteLine("functc = 0x{0:X}", functc);
				Console.WriteLine("imm    = 0x{0:X}", imm);
				Console.WriteLine("jimm   = 0x{0:X}", jimm);
			}
		}

		// Execution Caller
		// For speed purposes, common arithmetic functions are in a separate
		// jump table from the opcode table.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void execute() {
			if (DEBUG_CPU) {
				Console.WriteLine("==== Execute ====");
                Console.WriteLine("=Disassembly=");
			}

			if (context.getOpcode() == 0) {
                if (DEBUG_CPU) {
                    Console.WriteLine("             INST      RS RT RD IMM JIMM SHAMT");
                    Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", funct[context.getFunct()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
                }
                
				funct[context.getFunct()].execute(ref context);
			}
			else {
                if (DEBUG_CPU) {
                    Console.WriteLine("             INST      RS RT RD IMM JIMM SHAMT");
                    Console.WriteLine("Instruction: {0}, {1}, {2}, {3}, {4}, {5}, {6}", opcode[context.getOpcode()].GetType().Name, context.getRS(), context.getRT(), context.getRD(), context.getImm(), context.getJimm(), context.getShamt());
                }

				opcode[context.getOpcode()].execute(ref context);
			}
		}

		// Inserts given word at address in memory
		public void loadText(UInt32 address, UInt32 word) {
			mainMemory.StoreWord(address, word);
		}

		// Reads bare metal binary file into memory at address
        // Needs a minor rewrite..
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
                            if (DEBUG_CPU) {
                                Console.WriteLine("address: 0x{0:X}      = 0x{1:X}", address, word);
                            }
							loadText(address, word);
							address += 4;
							count = 0;
						}
					}
				}

                if (DEBUG_CPU) {
                    Console.WriteLine("File Loaded");
                }
			} catch (Exception e) {
				Console.WriteLine("Error: " + e.Message);
			}
		}

		// ELFSharp-based loader
        // Only does naive loading so far
        // i.e. no relocation
		public void elfLoader(String filename) {
            var elfloader = ELFSharp.ELF.ELFReader.Load<uint>(filename);
            foreach (var header in elfloader.Sections)
            {
                Console.WriteLine(header);
            }

            var textseg = elfloader.GetSection(".text");
            var address = textseg.LoadAddress;

            // Set Program Counter
            PC.setValue(address);

            if (DEBUG_CPU) {
                Console.WriteLine("ELF Entry Point: 0x{0:X}", address);
            }

            // Load all Prog Sections
            foreach (var section in elfloader.GetSections<ELFSharp.ELF.Sections.ProgBitsSection<uint>>()) {
                if (section.LoadAddress == 0) {
                    continue;
                }
                else {
                    address = section.LoadAddress;

                    if (DEBUG_CPU) {
                        Console.WriteLine("Section: {0}         0x{1:X}", section.Name, address);
                    }
                    
                    uint count = 0;
                    byte[] array = new byte[4];

                    foreach (var inst in textseg.GetContents()) {
                        array[count] = inst;
                        count++;
                        if (count == 4) {
                            UInt32 word = byteArrayToUInt32(array);
                            loadText(address, word);
                            address += 4;
                            count = 0;

                            if (DEBUG_CPU) {
                                Console.WriteLine("address: 0x{0:X}      = 0x{1:X}", address, word);
                            }
                        }
                    }
                }
            }
		}

		// No union support so here we are.
		// Joins 4 byte array into UInt32
		public UInt32 byteArrayToUInt32(byte[] array) {
			UInt32 word = (UInt32)array[0] << 24;
			word |= (UInt32)array[1] << 16;
			word |= (UInt32)array[2] << 8;
			word |= (UInt32)array[3];
			return word;
		}

		// Print out all GPRs and HI/LO Registers
		public void printRegisters() {
			Console.WriteLine("==== Register File ====");
			for (uint i=0; i<32; i++) {
				Console.WriteLine("%r" + i + "     = 0x{0:X}", registerFile[i].getValue());
			}
			Console.WriteLine("%HI" + "      = 0x{0:X}", hi.getValue());
			Console.WriteLine("%LO" + "      = 0x{0:X}", lo.getValue());
		}

        // Print out Coprocessor 0 Registers sadistically
        public void printCPC0Registers()
        {
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