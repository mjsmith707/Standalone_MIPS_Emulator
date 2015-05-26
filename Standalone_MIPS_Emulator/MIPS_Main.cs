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

namespace Standalone_MIPS_Emulator
{
	class MainClass
	{
		// Performance Timer
		private static Timer perfTimer;

		public static MIPS_CPU CPU0;

		public static void Main (string[] args)
		{
			CPU0 = new MIPS_CPU();
			// 001000 00001 00010 00000 00000 001111

			CPU0.loadFile(0x00000000, "main3.bin");
            //CPU0.loadFile(0x400550, "a.bin");
            //CPU0.elfLoader("a.out");

			perfTimer = new System.Timers.Timer(60000);
			perfTimer.Elapsed += OnTimedEvent;
			perfTimer.Enabled = true;

			CPU0.start();
		}

		private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    	{
        	Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
			Console.WriteLine("Cycle" + "    = {0}", CPU0.cyclecount);
    	}
	}
}
