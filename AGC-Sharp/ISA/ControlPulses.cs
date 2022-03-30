using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.ISA
{
    internal static class ControlPulses
    {
        public static void A2X(Cpu cpu)
        {
            cpu.AdderX = cpu.RegisterA;
        }

        public static void CI(Cpu cpu)
        {
            cpu.AdderCarry = true;
        }

        public static void G2LS(Cpu cpu)
        {
            cpu.RegisterL &= 0b0011000000000000;    // We're replacing bits 1-12,15,16
            ushort mainBits = (ushort)((cpu.RegisterG & 0x7FFF) >> 3);  // G bits 4-15 to L bits 1-12
            ushort upperBits = (ushort)(cpu.RegisterG & 0x8000);    // G bit 16 to L bit 16
            upperBits |= (ushort)((cpu.RegisterG & 1) << 14);       // G bit 1 to L bit 15
            cpu.RegisterL |= (ushort)(mainBits | upperBits);        // All together now
        }

        public static void INVALID(Cpu cpu)
        {
            throw new InvalidOperationException("This control pulse indicates an illogical condition has been reached.");
        }

        public static void L16(Cpu cpu)
        {
            cpu.RegisterL |= 0x8000;    // Set bit 16 of L to 1
        }

        public static void L2GD(Cpu cpu)
        {
            cpu.RegisterG = (ushort)((cpu.RegisterL & 0x3FFF) << 1);    // L bits 1-14 into G bits 2-15
            cpu.RegisterG |= (ushort)(cpu.RegisterL & 0x8000);          // L bit 16 into G bit 16
            cpu.RegisterG |= (ushort)(cpu.MCRO ? 1 : 0);                // MCRO into G bit 1
        }

        public static void MONEX(Cpu cpu)
        {
            cpu.AdderX |= 0xFFFE;   // Set all but bit 1 to 1
        }

        public static void NEACOF(Cpu cpu)
        {
            cpu.NoEAC = false;
        }

        public static void NEACON(Cpu cpu)
        {
            cpu.NoEAC = true;
        }

        public static void NISQ(Cpu cpu)
        {
            cpu.NextInstruction = true;
        }

        public static void PONEX(Cpu cpu)
        {
            cpu.AdderX |= 1;
        }

        public static void PTWOX(Cpu cpu)
        {
            cpu.AdderX |= 2;
        }

        public static void RAD(Cpu cpu)
        {
            switch (cpu.RegisterG)
            {
                case 3:
                    cpu.InhibitInterrupts = false;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                case 4:
                    cpu.InhibitInterrupts = true;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                case 6:
                    cpu.Extend_Next = true;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                default:
                    RG(cpu);
                    break;
            }
        }

        public static void R1C(Cpu cpu)
        {
            cpu.WriteBus |= 0xFFFE;
        }

        public static void RB1(Cpu cpu)
        {
            cpu.WriteBus |= 1;
        }

        public static void RA(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterA;
        }

        public static void RB(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterB;
        }

        public static void RC(Cpu cpu)
        {
            cpu.WriteBus |= (ushort)(cpu.RegisterB ^ 0xFFFF);
        }

        public static void RCH(Cpu cpu)
        {
            if (cpu.RegisterS == 1)
            {
                RL(cpu);
            }
            else if (cpu.RegisterS == 2)
            {
                RQ(cpu);
            }
            else
            {
                cpu.WriteBus |= Helpers.Bit16To15(cpu.IOChannels[(byte)(cpu.RegisterS & 0x3F)], false);
            }
        }

        public static void RG(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterG;
        }

        public static void RL(Cpu cpu)
        {
            cpu.WriteBus |= Helpers.Bit16To15(cpu.RegisterL, false);
        }

        public static void RL10BB(Cpu cpu)
        {
            cpu.WriteBus |= (ushort)(cpu.RegisterB & 0x3FF);
        }

        public static void RQ(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterQ;
        }

        public static void RSC(Cpu cpu)
        {
            switch (cpu.RegisterS)
            {
                case 0:
                    RA(cpu);
                    break;
                case 1:
                    RL(cpu);
                    break;
                case 2:
                    RQ(cpu);
                    break;
                case 3:
                    cpu.WriteBus |= cpu.RegisterEB;
                    break;
                case 4:
                    cpu.WriteBus |= cpu.RegisterFB;
                    break;
                case 5:
                    RZ(cpu);
                    break;
                case 6:
                    cpu.WriteBus |= cpu.RegisterBB;
                    break;
            }
        }

        // Perform one's complement addition, store result in write bus
        public static void RU(Cpu cpu)
        {
            cpu.WriteBus |= cpu.AdderOutput;
        }

        public static void RUS(Cpu cpu)
        {
            cpu.WriteBus |= (ushort)(cpu.AdderOutput | ((cpu.AdderOutput << 1) & 0x8000));  // OR the 15th bit into the 16th
        }

        public static void RZ(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterZ;
        }

        public static void RSTRT(Cpu cpu)
        {
            cpu.WriteBus = 0x800;   // Octal 4000
        }

        public static void ST1(Cpu cpu)
        {
            cpu.RegisterST_Next |= 1;
        }

        public static void ST2(Cpu cpu)
        {
            cpu.RegisterST_Next |= 2;
        }

        public static void TL15(Cpu cpu)
        {
            cpu.RegisterBR1 = ((cpu.RegisterL & 0x4000) > 0);   // L bit 15 into BR1
        }

        /// <summary>
        /// Test WL1-16 for all ones (-0). Set BR2 if true.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TMZ(Cpu cpu)
        {
            if (cpu.WriteBus == 0xFFFF)
            {
                cpu.RegisterBR2 = true;
            }
            else
            {
                cpu.RegisterBR2 = false;
            }
        }

        /// <summary>
        /// Test for + or - overflow. Set BR1,2 to 00 if no
        /// overflow, 01 if + overflow, 10 if - overflow.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TOV(Cpu cpu)
        {
            byte overflowTest = (byte)(cpu.WriteBus >> 14);

            switch (overflowTest)
            {
                case 1: // Bits are 01, positive overflow
                    cpu.RegisterBR1 = false;
                    cpu.RegisterBR2 = true;
                    break;
                case 2: // Bits are 10, negative overflow
                    cpu.RegisterBR1 = true;
                    cpu.RegisterBR2 = false;
                    break;
                default:    // No overflow
                    cpu.RegisterBR1 = false;
                    cpu.RegisterBR2 = false;
                    break;
            }
        }

        /// <summary>
        /// Test content of G for plus zero. If true set BR2 = 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TPZG(Cpu cpu)
        {
            if (cpu.RegisterG == 0)
            {
                cpu.RegisterBR2 = true;
            }
        }

        public static void TRSM(Cpu cpu)
        {
            if (cpu.RegisterS == 17)
            {
                ST2(cpu);
            }
        }

        /// <summary>
        /// Test sign. Copy WL16 to BR1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TSGN(Cpu cpu)
        {
            cpu.RegisterBR1 = ((cpu.WriteBus >> 15) == 1);
        }

        /// <summary>
        /// Test sign. Copy WL16 to BR2.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TSGN2(Cpu cpu)
        {
            cpu.RegisterBR2 = ((cpu.WriteBus >> 15) == 1);
        }

        public static void WA(Cpu cpu)
        {
            cpu.RegisterA = cpu.WriteBus;
        }

        public static void WALS(Cpu cpu)
        {
            cpu.RegisterA = (ushort)(cpu.WriteBus >> 2);            // WL bits 3-16 into A bits 1-14
            cpu.RegisterL &= 0b1100111111111111;    // We're replacing L bits 13,14
            cpu.RegisterL |= (ushort)((cpu.WriteBus & 3) << 12);    // WL bits 1,2 into L bits 13,14

            if ((cpu.RegisterG & 1) == 0)
            {
                cpu.RegisterA |= (ushort)(cpu.RegisterG & 0x8000);          // G bit 16 into A bit 16
                cpu.RegisterA |= (ushort)((cpu.RegisterG & 0x8000) >> 1);   // G bit 16 into A bit 15
            }
            else
            {
                cpu.RegisterA |= (ushort)(cpu.WriteBus & 0x8000);           // WL bit 16 into A bit 16
                cpu.RegisterA |= (ushort)((cpu.WriteBus & 0x8000) >> 1);    // WL bit 16 into A bit 15
            }
        }

        public static void WB(Cpu cpu)
        {
            cpu.RegisterB = cpu.WriteBus;
        }

        public static void WCH(Cpu cpu)
        {
            if (cpu.RegisterS == 1)
            {
                WL(cpu);
            }
            else if (cpu.RegisterS == 2)
            {
                WQ(cpu);
            }
            else
            {
                cpu.IOChannels[(byte)(cpu.RegisterS & 0x3F)] = Helpers.Bit16To15(cpu.WriteBus, false);  // TODO: Maybe this should be 'true'
            }
        }

        public static void WG(Cpu cpu)
        {
            if (cpu.RegisterS >= 0x10 && cpu.RegisterS <= 0x13)
            {
                switch (cpu.RegisterS)
                {
                    case 0x10:  // Cycle Right
                        ushort bottomToTop = (ushort)(cpu.WriteBus << 15);  // Cycle the least significant bit to the most
                        cpu.RegisterG = (ushort)((cpu.WriteBus >> 1) | bottomToTop);
                        break;
                    case 0x11:  // Shift Right
                        ushort topBit = (ushort)(cpu.WriteBus & 0b1000000000000000);
                        cpu.RegisterG = (ushort)((cpu.WriteBus >> 2) | topBit);  // Shift right, preserving top bit
                        break;
                    case 0x12:  // Cycle Left
                        ushort topToBottom = (ushort)((cpu.WriteBus & 1) >> 15);  // Cycle the most significant bit to the least
                        cpu.RegisterG = (ushort)((cpu.WriteBus << 1) | topToBottom);
                        break;
                    case 0x13:  // EDOP
                        cpu.RegisterG = (ushort)((cpu.WriteBus & 0b0011111110000000) >> 7);
                        break;
                }
            }
            else
            {
                cpu.RegisterG = cpu.WriteBus;
            }
        }

        public static void WL(Cpu cpu)
        {
            cpu.RegisterL = cpu.WriteBus;
        }

        public static void WOVR(Cpu cpu)
        {
            // Ignoring the crap out of this one for now
        }

        public static void WS(Cpu cpu)
        {
            cpu.RegisterS = (ushort)(cpu.WriteBus & 0x0FFF);
        }

        public static void WSC(Cpu cpu)
        {
            switch (cpu.RegisterS)
            {
                case 0:
                    cpu.RegisterA = cpu.WriteBus;
                    break;
                case 1:
                    cpu.RegisterL = cpu.WriteBus;
                    break;
                case 2:
                    cpu.RegisterQ = cpu.WriteBus;
                    break;
                case 3:
                    cpu.RegisterEB = cpu.WriteBus;
                    break;
                case 4:
                    cpu.RegisterFB = cpu.WriteBus;
                    break;
                case 5:
                    cpu.RegisterZ = cpu.WriteBus;
                    break;
                case 6:
                    cpu.RegisterBB = cpu.WriteBus;
                    break;
            };
        }

        public static void WQ(Cpu cpu)
        {
            cpu.RegisterQ = cpu.WriteBus;
        }

        public static void WY(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = cpu.WriteBus;
            cpu.AdderCarry = false;
        }

        public static void WYD(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = (ushort)((cpu.WriteBus & 0x3FFF) << 1);    // WL bits 1-14 into Y bits 2-15
            // WL bit 16 into Y bit 16 if circumstances allow
            if (!cpu.NoEAC && !cpu.ShincSequence && !(cpu.PIFL && (cpu.RegisterL & 0x4000) > 0))
                cpu.AdderY |= (ushort)(cpu.WriteBus & 0x8000);
            cpu.AdderCarry = false;
        }

        public static void WY12(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = (ushort)(cpu.WriteBus & 0x0FFF);
            cpu.AdderCarry = false;
        }

        public static void WZ(Cpu cpu)
        {
            cpu.RegisterZ = cpu.WriteBus;
        }

        public static void ZAP(Cpu cpu)
        {
            RU(cpu);
            G2LS(cpu);
            WALS(cpu);
        }

        public static void ZIP(Cpu cpu)
        {
            // Prep based on state table
            int stateBits = 0;
            stateBits |= (((cpu.RegisterL >> 14) & 1) << 2);    // L bit 15 into state bit 3
            stateBits |= (cpu.RegisterL & 2);                   // L bit 2 into state bit 2
            stateBits |= (cpu.RegisterL & 1);                   // L bit 1 into state bit 1

            switch (stateBits)
            {
                case 0:
                    WY(cpu);
                    cpu.MCRO = false;
                    break;
                case 1:
                    RB(cpu);
                    WY(cpu);
                    cpu.MCRO = false;
                    break;
                case 2:
                    RB(cpu);
                    WYD(cpu);
                    cpu.MCRO = false;
                    break;
                case 3:
                    RC(cpu);
                    WY(cpu);
                    CI(cpu);
                    cpu.MCRO = true;
                    break;
                case 4:
                    RB(cpu);
                    WY(cpu);
                    cpu.MCRO = false;
                    break;
                case 5:
                    RB(cpu);
                    WYD(cpu);
                    cpu.MCRO = false;
                    break;
                case 6:
                    RC(cpu);
                    WY(cpu);
                    CI(cpu);
                    cpu.MCRO = true;
                    break;
                case 7:
                    WY(cpu);
                    cpu.MCRO = true;
                    break;
                default:
                    throw new InvalidDataException($"{nameof(stateBits)} should only have 7 possible states");
                    break;
            }

            A2X(cpu);
            L2GD(cpu);
        }
    }
}
