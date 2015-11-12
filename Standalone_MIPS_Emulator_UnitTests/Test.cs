using NUnit.Framework;
using System;
using Standalone_MIPS_Emulator;

namespace Standalone_MIPS_Emulator_UnitTests
{
	[TestFixture ()]
	public class Test
	{
		// CPU Initialization Code
		[SetUp]
		public void CPUInit() {
		}

		// ISA Tests
		// Add
		[Test ()]
		public void add() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x00436020);
			cpu.loadText(0x00000014, 0x00456820);
			cpu.loadText(0x00000018, 0x00837020);
			cpu.loadText(0x0000001C, 0x00857820);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(700, cpu.getRegister(12));
			Assert.AreEqual(-300, (int)cpu.getRegister(13));
			Assert.AreEqual(300, cpu.getRegister(14));
			Assert.AreEqual(-700, (int)cpu.getRegister(15));
		}

		// Addi
		[Test ()]
		public void addi() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x204801f4);
			cpu.loadText(0x00000014, 0x2049fe0c);
			cpu.loadText(0x00000018, 0x208a01f4);
			cpu.loadText(0x0000001C, 0x208bfe0c);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(200, cpu.getRegister(2));
			Assert.AreEqual(500, cpu.getRegister(3));
			Assert.AreEqual(-200, (int)cpu.getRegister(4));
			Assert.AreEqual(-500, (int)cpu.getRegister(5));
			Assert.AreEqual(700, cpu.getRegister(8));
			Assert.AreEqual(-300, (int)cpu.getRegister(9));
			Assert.AreEqual(300, cpu.getRegister(10));
			Assert.AreEqual(-700, (int)cpu.getRegister(11));
		}

		// Addiu
		[Test ()]
		public void addiu() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x244801f4);
			cpu.loadText(0x00000014, 0x2449fe0c);
			cpu.loadText(0x00000018, 0x248a01f4);
			cpu.loadText(0x0000001C, 0x248bfe0c);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(700, cpu.getRegister(8));
			Assert.AreEqual(-300, (int)cpu.getRegister(9));
			Assert.AreEqual(300, cpu.getRegister(10));
			Assert.AreEqual(-700, (int)cpu.getRegister(11));
		}

		// Addu
		[Test ()]
		public void addu() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x00434021);
			cpu.loadText(0x00000014, 0x00454821);
			cpu.loadText(0x00000018, 0x00835021);
			cpu.loadText(0x0000001C, 0x00855821);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(700, cpu.getRegister(8));
			Assert.AreEqual(-300, (int)cpu.getRegister(9));
			Assert.AreEqual(300, cpu.getRegister(10));
			Assert.AreEqual(-700, (int)cpu.getRegister(11));
		}

		// And
		[Test ()]
		public void and() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x00434024);
			cpu.loadText(0x00000014, 0x00454824);
			cpu.loadText(0x00000018, 0x00835024);
			cpu.loadText(0x0000001C, 0x00855824);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(192, cpu.getRegister(8));
			Assert.AreEqual(8, cpu.getRegister(9));
			Assert.AreEqual(304, cpu.getRegister(10));
			Assert.AreEqual(0xfffffe08, cpu.getRegister(11));
		}

		// Andi
		[Test ()]
		public void andi() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00000000);
			cpu.loadText(0x00000000, 0x200200c8);
			cpu.loadText(0x00000004, 0x200301f4);
			cpu.loadText(0x00000008, 0x2004ff38);
			cpu.loadText(0x0000000C, 0x2005fe0c);
			cpu.loadText(0x00000010, 0x304801f4);
			cpu.loadText(0x00000014, 0x3049fe0c);
			cpu.loadText(0x00000018, 0x308a01f4);
			cpu.loadText(0x0000001C, 0x308bfe0c);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(192, cpu.getRegister(8));
			Assert.AreEqual(8, cpu.getRegister(9));
			Assert.AreEqual(304, cpu.getRegister(10));
			Assert.AreEqual(65032, cpu.getRegister(11));
		}

		// Beq - branch on equal
		[Test ()]
		public void beq() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x1043fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Beql - branch on equal likely
		[Test ()]
		public void beql() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x5043FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bgez - branch on greater than or equal to zero
		[Test ()]
		public void bgez() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0441fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bgezal - branch on greater than or equal to zero and link
		[Test ()]
		public void bgezal() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0451fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
			Assert.AreEqual(0x00400010, cpu.getRegister(31));
		}

		// Bgezall - branch on greater than or equal to zero and link likely
		[Test ()]
		public void bgezall() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0453FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
			Assert.AreEqual(0x00400010, cpu.getRegister(31));
		}

		// Bgezl - branch on greater than or equal to zero likely
		[Test ()]
		public void bgezl() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0443FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bgtz - branch on greater than zero
		[Test ()]
		public void bgtz() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x1c40fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bgtzl - branch on greater than zero likely
		[Test ()]
		public void bgtzl() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x200200c8);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x5C40FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Blez - branch on less than or equal to zero
		[Test ()]
		public void blez() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x1840fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Blezl - branch on less than or equal to zero likely
		[Test ()]
		public void blezl() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x5840FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bltz - branch on less than zero
		[Test ()]
		public void bltz() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0440fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bltzal - branch on less than zero and link
		[Test ()]
		public void bltzal() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0450fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
			Assert.AreEqual(0x00400010, cpu.getRegister(31));
		}

		// Bltzall - branch on less than zero and link likely
		[Test ()]
		public void bltzall() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0452fffD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
			Assert.AreEqual(0x00400010, cpu.getRegister(31));
		}

		// Bltzl - branch on less than zero likely
		[Test ()]
		public void bltzl() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0442FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bne - branch on not equal
		[Test ()]
		public void bne() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x1443fffd);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Bnel - branch on not equal likely
		[Test ()]
		public void bnel() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x5443FFFD);
			cpu.loadText(0x0040000C, 0x00000000);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Break - breakpoint
		[Test ()]
		public void brk() {
			Assert.AreEqual(0, 1);
		}

		// Cache - perform cache operation
		[Test ()]
		public void cache() {
			Assert.AreEqual(0, 1);
		}

		// Clo - count leading ones in word
		[Test ()]
		public void clo() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x70402021);
			cpu.loadText(0x0040000C, 0x70602821);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00000018, cpu.getRegister(4));
			Assert.AreEqual(0x00000000, cpu.getRegister(5));
		}

		// Clz - count leading zeros in word
		[Test ()]
		public void clz() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ff38);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x70402020);
			cpu.loadText(0x0040000C, 0x70602820);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x00000000, cpu.getRegister(4));
			Assert.AreEqual(0x00000018, cpu.getRegister(5));
		}

		// Div - divide word
		[Test ()]
		public void div() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ffcb);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0043001a);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0xffffffcb, cpu.getHi());
			Assert.AreEqual(0x00000000, cpu.getLo());
			cpu.loadText(0x0040000C, 0x0062001a);
			cpu.singleStep();
			Assert.AreEqual(0x00000029, cpu.getHi());
			Assert.AreEqual(0xfffffffd, cpu.getLo());
		}

		// Divu - divide unsigned word
		[Test ()]
		public void divu() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x2002ffcb);
			cpu.loadText(0x00400004, 0x200300c8);
			cpu.loadText(0x00400008, 0x0043001b);
			cpu.singleStep();
			cpu.singleStep();
			cpu.singleStep();
			Assert.AreEqual(0x0000002b, cpu.getHi());
			Assert.AreEqual(0x0147ae14, cpu.getLo());
			cpu.loadText(0x0040000C, 0x0062001b);
			cpu.singleStep();
			Assert.AreEqual(0x000000c8, cpu.getHi());
			Assert.AreEqual(0x00000000, cpu.getLo());
		}

		// Eret - exception return
		[Test ()]
		public void eret () {
			Assert.AreEqual(0, 1);
		}

		// J - jump
		[Test ()]
		public void j() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x00000000);
			cpu.loadText(0x00400004, 0x00000000);
			cpu.loadText(0x00400008, 0x00000000);
			cpu.loadText(0x0040000c, 0x00000000);
			cpu.loadText(0x00400010, 0x00000000);
			cpu.loadText(0x00400014, 0x00000000);
			cpu.loadText(0x00400018, 0x00000000);
			cpu.loadText(0x0040001c, 0x00000000);
			cpu.loadText(0x00400020, 0x00000000);
			cpu.loadText(0x00400024, 0x00000000);
			cpu.loadText(0x00400028, 0x08100000);
			cpu.loadText(0x0040002c, 0x00000000);

			for (int i=0; i<13; i++) {
				cpu.singleStep();
			}

			Assert.AreEqual(0x00400000, cpu.getPC()-4);
		}

		// Jal - jump and link
		[Test ()]
		public void jal() {
			MIPS_CPU cpu = new MIPS_CPU();
			cpu.setPC(0x00400000);
			cpu.loadText(0x00400000, 0x00000000);
			cpu.loadText(0x00400004, 0x00000000);
			cpu.loadText(0x00400008, 0x00000000);
			cpu.loadText(0x0040000c, 0x00000000);
			cpu.loadText(0x00400010, 0x00000000);
			cpu.loadText(0x00400014, 0x00000000);
			cpu.loadText(0x00400018, 0x00000000);
			cpu.loadText(0x0040001c, 0x00000000);
			cpu.loadText(0x00400020, 0x00000000);
			cpu.loadText(0x00400024, 0x00000000);
			cpu.loadText(0x00400028, 0x0c100000);
			cpu.loadText(0x0040002c, 0x00000000);

			for (int i=0; i<13; i++) {
				cpu.singleStep();
			}

			Assert.AreEqual(0x00400000, cpu.getPC()-4);
			Assert.AreEqual(0x00400030, cpu.getRegister(31));
		}

		[Test ()]
		public void jalr() {
			// FIXME: jalr can select a return address register to save in different from 31
			// It is the only instruction that can do this!
			Assert.AreEqual(0, 1);
		}
	}
}

