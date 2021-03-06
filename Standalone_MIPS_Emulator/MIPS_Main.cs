﻿/*
Copyright (c) 2015, Matt Smith
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Timers;

namespace Standalone_MIPS_Emulator {
	class MainClass {
		// Performance Timer
		private static Timer perfTimer;
		private static UInt64 lastCount;

		public static MIPS_CPU CPU0;

		public static void Main (string[] args) {
			Console.WriteLine("Starting...");
			CPU0 = new MIPS_CPU();

			//CPU0.loadFile(0x00000000, "cop0_test.bin");
			//CPU0.loadFile(0x00000000, "registerload_test.bin");
			//CPU0.loadFile(0x400550, "a.bin");
			//CPU0.setPC(0x400550);
			//CPU0.elfLoader("main.o");
			CPU0.elfLoader("mmon.elf");
            //CPU0.elfLoader("netbsd-GENERIC32_IP12");
            //CPU0.makeBootArgs(0xFFFF4000,3,"debug earlycon=uart8250,mmio,0xb40003f8,115200n8 earlyprintk=serial,0xb40003f8,115200", "");
            //CPU0.makeBootArgs(0xFF004000,1,"earlycon=uart8250,mmio,0xb40003f8,115200n8", "");
			//CPU0.elfLoader("vmlinux");




			lastCount = 0;
			perfTimer = new System.Timers.Timer(60000);
			perfTimer.Elapsed += cycleSnapshotOnTrigger;
			perfTimer.Enabled = false;

			CPU0.start();

            // IO Block
            Console.ReadKey();
		}

		private static void cycleSnapshotOnTrigger(Object source, ElapsedEventArgs e) {
			UInt64 count = CPU0.cyclecount;
			double IPS = (count - lastCount) / 60;
			double MIPS = IPS / 1000000;
			Console.WriteLine("IPS: {0}", IPS);
			Console.WriteLine("MIPS: {0}", MIPS);
			Console.WriteLine("IPM: {0}", count - lastCount);
			lastCount = count;
		}
	}
}