using System;
using System.Collections.Generic;

// https://en.wikibooks.org/wiki/Serial_Programming/8250_UART_Programming#UART_Registers

// Early stages. Won't trigger interrupts or anything yet.
// Purely for grabbing early_printk output
namespace Standalone_MIPS_Emulator
{
	public class UART8250 : MIPS_MemoryMappedIO
	{
		// List of Port Addresses
		private List<uint> addresses;
		const uint UART_BASE = 0xB40003f8;
		private byte THR;
		private byte RBR;
		private byte DLL;
		private byte IER;
		private byte DLH;
		private byte IIR;
		private byte FCR;
		private byte LCR;
		private byte MCR;
		private byte LSR;
		private byte MSR;
		private byte SR;

        // Character Buffer
        private const byte BUFFSIZE = 16;
        private byte[] buffer;
        private byte bufferSavePos;
        private byte bufferReadPos;
        private byte numChars;

        // Misc
        private bool rbravail;
        private const byte ONE = 0x1;
        private const byte ZERO = 0x0;

        public UART8250() {
			addresses = new List<uint>();

			// UART Addresses
			// Base+0
			// THR Transmitter Holding Buffer
			// RBR Receiver Buffer
			// DLL Divisor Latch Low Byte
			addresses.Add(UART_BASE);	

			// Base+1
			// IER Interrupt Enable Register
			// DLH Divisor Latch High Byte
			addresses.Add(UART_BASE+1);

			// Base+2
			// IIR Interrupt Identification Register
			// FCR FIFO Control Register
			addresses.Add(UART_BASE+2);

			// Base+3
			// LCR Line Control Register
			addresses.Add(UART_BASE+3);

			// Base+4
			// MCR Modem Control Register
			addresses.Add(UART_BASE+4);

			// Base+5
			// LSR Line Status Register
			addresses.Add(UART_BASE+5);

			// Base+6
			// MSR Modem Status Register
			addresses.Add(UART_BASE+6);

			// Base+7
			// SR  Scratch Register
			addresses.Add(UART_BASE+7);
		}

		// Device initializer
		public override void initDevice() {
            buffer = new byte[BUFFSIZE];
            bufferSavePos = 0;
            bufferReadPos = 0;
            numChars = 0;
            THR = 0x0;
			RBR = 0x0;
			DLL = 0x0;
			IER = 0x0;
			DLH = 0x0;
			IIR = 0x0;
			FCR = 0x0;
			LCR = 0x0;
			MCR = 0x0;
			LSR = 0x60;
			MSR = 0x0;
			SR = 0x0;
		}

		// List of overridden addresses
		public override List<uint> getAddresses() {
			return addresses;
		}

		// Test if the Divisor Latch Access Bit is set
		private bool DLABSet() {
			return (LCR & 0x40) > 0;
		}

		// Read a single byte
		public override byte ReadByte(uint address) {
			switch (address) {
				case UART_BASE: {
					if (DLABSet()) {
						return DLL;
					}
					else {
                        // Clear Data Ready bit
                        byte mask = 1;
                        LSR &= (byte)~mask;
                        return RBR;
					}
				}
				case UART_BASE+1: {
					if (DLABSet()) {
						return DLH;
					}
					else {
						return IER;
					}
				}
				case UART_BASE+2: {
					return IIR;
				}
				case UART_BASE+3: {
					return LCR;
				}
				case UART_BASE+4: {
					return MCR;
				}
				case UART_BASE+5: {
					return LSR;
				}
				case UART_BASE+6: {
					return MSR;
				}
				case UART_BASE+7: {
					return SR;
				}
				default: {
					throw new ApplicationException("Memory tried to read from invalid UART address");
				}
			}
		}

		// Store a single byte
		public override void StoreByte(uint address, byte value) {
			switch (address) {
				case UART_BASE: {
					if (DLABSet()) {
						DLL = value;
					}
					else {
						THR = value;
						Console.Write((char)THR);
					}
					break;
				}
				case UART_BASE+1: {
					if (DLABSet()) {
						DLH = value;
					}
					else {
						IER = value;
					}
					break;
				}
				case UART_BASE+2: {
					FCR = value;
					break;
				}
				case UART_BASE+3: {
					LCR = value;
					break;
				}
				case UART_BASE+4: {
					MCR = value;
					break;
				}
				case UART_BASE+5: {
					break;
				}
				case UART_BASE+6: {
					break;
				}
				case UART_BASE+7: {
					SR = value;
					break;
				}
				default: {
					throw new ApplicationException("Memory tried to write to invalid UART address");
				}
			}
		}

        // Method to receive character from console
        public void sendChar(char ch) {
            // Save to buffer
            RBR = (byte)ch;
            // Signal that there is data available
            LSR |= ONE;
        }
	}
}

