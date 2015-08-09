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

// Basic memory system for testing
// Does not support virtual memory.
namespace Standalone_MIPS_Emulator {
public class MIPS_Memory {

	// Physical Frame Table
	private Hashtable pagetable;
	private const byte defaultflags = 0x0;

	// Default Constructor
	public MIPS_Memory () {
		pagetable = new Hashtable();
	}

	// Checks if the frame table contains a given frame
	private bool pageExists(UInt32 address) {
		return pagetable.ContainsKey(address&0xFFFFF000);
	}

	// Read a single byte
	public byte ReadByte(UInt32 address) {
		// If the physical frame doesn't exist
		// create a new one full of zeroes.
		if (!pageExists(address)) {
			pagetable.Add(address&0xFFFFF000, new MIPS_MemoryPage(address, defaultflags));
		}

		MIPS_MemoryPage page1 = (MIPS_MemoryPage)pagetable[address&0xFFFFF000];
		return page1.readByte(address);
	}

	// Read two bytes
	public UInt16 ReadHalf(UInt32 address) {
		UInt16 half = 0;

		half = ReadByte(address);
		half <<= 8;
		half |= ReadByte(address+1);

		return half;
	}

	// Read four bytes
	public UInt32 ReadWord(UInt32 address) {
		UInt32 word = 0;

		word = ReadByte(address);
		word <<= 8;
		word |= ReadByte(address+1);
		word <<= 8;
		word |= ReadByte(address+2);
		word <<= 8;
		word |= ReadByte(address+3);

		return word;
	}

	// Store a single byte
	public void StoreByte(UInt32 address, byte value) {
		// If the physical frame doesn't exist
		// create a new one.
		if (!pageExists(address)) {
			pagetable.Add(address&0xFFFFF000, new MIPS_MemoryPage(address, defaultflags));
		}

		MIPS_MemoryPage page1 = (MIPS_MemoryPage)pagetable[address&0xFFFFF000];
		page1.writeByte(address, value);
	}

	// Store two bytes
	public void StoreHalf(UInt32 address, UInt16 value) {
		StoreByte(address, (byte)(value >> 8));
		StoreByte(address+1, (byte)(value));
	}

	// Store four bytes
	public void StoreWord(UInt32 address, UInt32 value) {
		StoreByte(address, (byte)(value >> 24));
		StoreByte(address+1, (byte)(value >> 16));
		StoreByte(address+2, (byte)(value >> 8));
		StoreByte(address+3, (byte)(value));
	}
}
}