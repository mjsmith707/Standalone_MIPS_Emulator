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

		public UART8250() {
			addresses = new List<uint>();

			// UART Addresses
			// Base+0
			// THR Transmitter Holding Buffer
			// RBR Receiver Buffer
			// DLL Divisor Latch Low Byte
			addresses.Add(0x000003F8);	

			// Base+1
			// IER Interrupt Enable Register
			// DLH Divisor Latch High Byte
			addresses.Add(0x000003F9);

			// Base+2
			// IIR Interrupt Identification Register
			// FCR FIFO Control Register
			addresses.Add(0x000003FA);

			// Base+3
			// LCR Line Control Register
			addresses.Add(0x000003FB);

			// Base+4
			// MCR Modem Control Register
			addresses.Add(0x000003FC);

			// Base+5
			// LSR Line Status Register
			addresses.Add(0x000003FD);

			// Base+6
			// MSR Modem Status Register
			addresses.Add(0x000003FE);

			// Base+7
			// SR  Scratch Register
			addresses.Add(0x000003FF);
		}

		// Device initializer
		public override void initDevice() {
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
				case 0x000003F8: {
					if (DLABSet()) {
						return DLL;
					}
					else {
						return RBR;
					}
				}
				case 0x000003F9: {
					if (DLABSet()) {
						return DLH;
					}
					else {
						return IER;
					}
				}
				case 0x000003FA: {
					return IIR;
				}
				case 0x000003FB: {
					return LCR;
				}
				case 0x000003FC: {
					return MCR;
				}
				case 0x000003FD: {
					return LSR;
				}
				case 0x000003FE: {
					return MSR;
				}
				case 0x000003FF: {
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
				case 0x000003F8: {
					if (DLABSet()) {
						DLL = value;
					}
					else {
						THR = value;
						Console.Write((char)THR);
					}
					break;
				}
				case 0x000003F9: {
					if (DLABSet()) {
						DLH = value;
					}
					else {
						IER = value;
					}
					break;
				}
				case 0x000003FA: {
					FCR = value;
					break;
				}
				case 0x000003FB: {
					LCR = value;
					break;
				}
				case 0x000003FC: {
					MCR = value;
					break;
				}
				case 0x000003FD: {
					break;
				}
				case 0x000003FE: {
					break;
				}
				case 0x000003FF: {
					SR = value;
					break;
				}
				default: {
					throw new ApplicationException("Memory tried to write to invalid UART address");
				}
			}
		}
	}
}

