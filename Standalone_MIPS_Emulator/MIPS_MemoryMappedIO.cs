using System;
using System.Collections.Generic;

namespace Standalone_MIPS_Emulator
{
	public abstract class MIPS_MemoryMappedIO {
		// Device initializer
		public abstract void initDevice();

		// List of overridden addresses
		public abstract List<uint> getAddresses();

		// Read a single byte
		public abstract byte ReadByte(uint address);

		// Store a single byte
		public abstract void StoreByte(uint address, byte value);
	}
}