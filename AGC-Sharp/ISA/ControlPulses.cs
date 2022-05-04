using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.Helpers;

namespace AGC_Sharp.ISA
{
    public static class ControlPulses
    {
        /// <summary>
        /// Copy A1-16 into X1-16 by private line.
        /// </summary>
        /// <param name="cpu"></param>
        public static void A2X(Cpu cpu)
        {
            cpu.AdderX = cpu.RegisterA;
        }

        /// <summary>
        /// Set bit 15 of X to 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void B15X(Cpu cpu)
        {
            cpu.AdderX |= (1 << 14);
        }

        /// <summary>
        /// Insert carry into bit 1 of the adder.
        /// </summary>
        /// <param name="cpu"></param>
        public static void CI(Cpu cpu)
        {
            cpu.AdderCarry = true;
        }

        /// <summary>
        /// Set the Extend flip flop.
        /// </summary>
        /// <param name="cpu"></param>
        public static void EXT(Cpu cpu)
        {
            cpu.Extend_Next = true;
        }

        /// <summary>
        /// Copy G4-15,16,1 into L1-12,16,15.
        /// </summary>
        /// <param name="cpu"></param>
        public static void G2LS(Cpu cpu)
        {
            cpu.RegisterL = CopyWordBits(cpu.RegisterG, cpu.RegisterL, 4..15, 1..12, BitCopyMode.ClearChanged);
            cpu.RegisterL = CopyWordBits(cpu.RegisterG, cpu.RegisterL, 16..16, 16..16, BitCopyMode.ClearChanged);
            cpu.RegisterL = CopyWordBits(cpu.RegisterG, cpu.RegisterL, 1..1, 15..15, BitCopyMode.ClearChanged);
        }

        public static void INVALID(Cpu cpu)
        {
            throw new InvalidOperationException("This control pulse indicates an illogical condition has been reached.");
        }

        /// <summary>
        /// Set bit 16 of L to 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void L16(Cpu cpu)
        {
            cpu.RegisterL |= 0x8000;    // Set bit 16 of L to 1
        }

        /// <summary>
        /// Copy L1-14,16 into G2-15,16 -- also MCRO into G1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void L2GD(Cpu cpu)
        {
            cpu.RegisterG = CopyWordBits(cpu.RegisterL, cpu.RegisterG, 1..14, 2..15, BitCopyMode.ClearAll); // L bits 1-14 into G bits 2-15
            cpu.RegisterG = CopyWordBits(cpu.RegisterL, cpu.RegisterG, 16..16, 16..16, BitCopyMode.ClearChanged);   // L bit 16 into G bit 16
            cpu.RegisterG |= (ushort)(cpu.MCRO ? 1 : 0);    // MCRO into G bit 1, bit 1 must be cleared before this point to work
        }

        /// <summary>
        /// Set bits 2-16 of X to ones.
        /// </summary>
        /// <param name="cpu"></param>
        public static void MONEX(Cpu cpu)
        {
            cpu.AdderX |= 0xFFFE;   // Set all but bit 1 to 1
        }

        /// <summary>
        /// Permit end around carry after end of MP3.
        /// </summary>
        /// <param name="cpu"></param>
        public static void NEACOF(Cpu cpu)
        {
            cpu.NoEAC = false;
        }

        /// <summary>
        /// Inhibit end around carry until NEACOF.
        /// </summary>
        /// <param name="cpu"></param>
        public static void NEACON(Cpu cpu)
        {
            cpu.NoEAC = true;
        }

        /// <summary>
        /// Next instruction is to be loaded into SQ. Also
        /// frees certain restrictions- permits increments and
        /// interrupts.
        /// </summary>
        /// <param name="cpu"></param>
        public static void NISQ(Cpu cpu)
        {
            cpu.NextInstruction = true;
            cpu.InhibitInterrupts = false;
        }

        /// <summary>
        /// Set bit 1 of X to 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void PONEX(Cpu cpu)
        {
            cpu.AdderX |= 1;
        }

        /// <summary>
        /// Set bit 2 of X to 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void PTWOX(Cpu cpu)
        {
            cpu.AdderX |= 2;
        }

        /// <summary>
        /// Read address of next cycle. This appears at the end
        /// of an instruction and normally is interpreted
        /// as RG. If the next instruction is to be a
        /// pseudo code (INHINT, RELINT, EXTEND), it is instead
        /// interpreted as RZ ST2.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RAD(Cpu cpu)
        {
            switch (cpu.RegisterG)
            {
                case 3:     // RELINT
                    cpu.InhibitInterrupts = false;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                case 4:     // INHINT
                    cpu.InhibitInterrupts = true;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                case 6:     // EXTEND
                    cpu.Extend_Next = true;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                default:    // ANYTHING ELSE
                    RG(cpu);
                    break;
            }
        }

        /// <summary>
        /// Place octal 177776 = -1 on WL's.
        /// </summary>
        /// <param name="cpu"></param>
        public static void R1C(Cpu cpu)
        {
            cpu.WriteBus |= 0xFFFE;
        }

        /// <summary>
        /// Place octal 000001 on the WL's.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RB1(Cpu cpu)
        {
            cpu.WriteBus |= 1;
        }

        /// <summary>
        /// Read A1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RA(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterA;
        }

        /// <summary>
        /// Read B1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RB(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterB;
        }

        /// <summary>
        /// Read the content of B inverted: C1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RC(Cpu cpu)
        {
            cpu.WriteBus |= (ushort)(cpu.RegisterB ^ 0xFFFF);
        }

        /// <summary>
        /// Read the content of the input or output channel
        /// specified by the current content of S:
        /// Channel bits 1-14 to WL1-14, and bit 16 to WL15,16.
        /// Channels 1 and 2 read as RL and RQ.
        /// </summary>
        /// <param name="cpu"></param>
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
                cpu.WriteBus |= Bit16To15(cpu.IOChannels[(byte)(cpu.RegisterS & 0x3F)], false);
            }
        }

        /// <summary>
        /// Read G1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RG(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterG;
        }

        /// <summary>
        /// Read L1-14 to WL1-14, and L16 to WL15 and 16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RL(Cpu cpu)
        {
            cpu.WriteBus = CopyWordBits(cpu.RegisterL, cpu.WriteBus, 1..14, 1..14, BitCopyMode.ClearChanged);
            cpu.WriteBus = CopyWordBits(cpu.RegisterL, cpu.WriteBus, 16..16, 15..15, BitCopyMode.ClearChanged);
            cpu.WriteBus = CopyWordBits(cpu.RegisterL, cpu.WriteBus, 16..16, 16..16, BitCopyMode.ClearChanged);
        }

        /// <summary>
        /// Read low 10 bits of B to WL 1-10.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RL10BB(Cpu cpu)
        {
            cpu.WriteBus = CopyWordBits(cpu.RegisterB, cpu.WriteBus, 1..10, 1..10, BitCopyMode.ClearChanged);
        }

        /// <summary>
        /// Read Q1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RQ(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterQ;
        }

        /// <summary>
        /// Read the content of central store defined by
        /// the address currently in S:
        /// Central store bits 1-16 are copied to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
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

        /// <summary>
        /// Read U1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RU(Cpu cpu)
        {
            cpu.WriteBus |= cpu.AdderOutput;
        }

        /// <summary>
        /// Read U1-16 to !L1-14, and U15 to WL15 and 16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RUS(Cpu cpu)
        {
            cpu.WriteBus |= (ushort)(cpu.AdderOutput | ((cpu.AdderOutput << 1) & 0x8000));  // OR the 15th bit into the 16th
        }

        /// <summary>
        /// Read Z1-16 to WL1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RZ(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterZ;
        }

        /// <summary>
        /// Place octal 004000 = Block 2 start address on WL's.
        /// </summary>
        /// <param name="cpu"></param>
        public static void RSTRT(Cpu cpu)
        {
            cpu.WriteBus = 0x800;   // Octal 4000
        }

        /// <summary>
        /// Set Stage1 flip flop next T12.
        /// </summary>
        /// <param name="cpu"></param>
        public static void ST1(Cpu cpu)
        {
            cpu.RegisterST_Next |= 1;
        }

        /// <summary>
        /// Set Stage2 flip flop next T12.
        /// </summary>
        /// <param name="cpu"></param>
        public static void ST2(Cpu cpu)
        {
            cpu.RegisterST_Next |= 2;
        }

        /// <summary>
        /// Copy L15 into BR1.
        /// </summary>
        /// <param name="cpu"></param>
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

        /// <summary>
        /// Test for resume address on INDEX. ST2 if (S)=0017.
        /// </summary>
        /// <param name="cpu"></param>
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

        /// <summary>
        /// Clear and write WL1-16 into A1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WA(Cpu cpu)
        {
            cpu.RegisterA = cpu.WriteBus;
        }

        /// <summary>
        /// Clear and write into A1-14 from WL3-16. Clear and
        /// write into L13,14 from WL1,2. Clear and write into
        /// A15,16 from G16 (if G1=0) or from WL16 (if G1=1).
        /// </summary>
        /// <param name="cpu"></param>
        public static void WALS(Cpu cpu)
        {
            cpu.RegisterA = CopyWordBits(cpu.WriteBus, cpu.RegisterA, 3..16, 1..14, BitCopyMode.ClearAll);  // WL bits 3-16 into A bits 1-14
            cpu.RegisterL = CopyWordBits(cpu.WriteBus, cpu.RegisterL, 1..2, 13..14, BitCopyMode.ClearChanged);  // WL bits 1-2 into L bits 13-14

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

        /// <summary>
        /// Clear and write WL1-16 into B1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WB(Cpu cpu)
        {
            cpu.RegisterB = cpu.WriteBus;
        }

        /// <summary>
        /// Clear and write WL1-14,16,parity into channel bits
        /// 1-14,16,parity. Channels 1 and 2 write as WL and WQ.
        /// The channel to be loaded is specified by the
        /// current content of S.
        /// </summary>
        /// <param name="cpu"></param>
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
                cpu.IOChannels[(byte)(cpu.RegisterS & 0x3F)] = Bit16To15(cpu.WriteBus, false);  // TODO: Maybe this should be 'true'
            }
        }

        /// <summary>
        /// Clear and write WL1-16 into G1-16 except
        /// for addresses octal 20-23, which cause editing.
        /// </summary>
        /// <param name="cpu"></param>
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

        /// <summary>
        /// Clear and write WL1-16 into L1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WL(Cpu cpu)
        {
            cpu.RegisterL = cpu.WriteBus;
        }

        /// <summary>
        /// Test for overflow during counter increments and
        /// program initiated increments (INCR and AUG). RUPT if
        /// overflow occurs when addressing certain counters.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WOVR(Cpu cpu)
        {
            // Ignoring the crap out of this one for now
        }

        /// <summary>
        /// Clear and write WL1-16 into Q1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WQ(Cpu cpu)
        {
            cpu.RegisterQ = cpu.WriteBus;
        }

        /// <summary>
        /// Clear and write WL1-12 into S1-12.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WS(Cpu cpu)
        {
            cpu.RegisterS = CopyWordBits(cpu.WriteBus, cpu.RegisterS, 1..12, 1..12, BitCopyMode.ClearAll);
        }

        /// <summary>
        /// Clear and write WL1-16 into the central register
        /// specified by the current content of S. Bits
        /// 1-16 into positions 1-16.
        /// </summary>
        /// <param name="cpu"></param>
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

        /// <summary>
        /// Clear Y and X. Write WL1-16 into Y1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WY(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = cpu.WriteBus;
            cpu.AdderCarry = false;
        }

        /// <summary>
        /// Clear Y and X. Write WL1-12 into Y1-12.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WY12(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = CopyWordBits(cpu.WriteBus, cpu.AdderY, 1..12, 1..12, BitCopyMode.ClearAll);
            cpu.AdderCarry = false;
        }

        /// <summary>
        /// Clear Y and X. Write WL1-14 into Y2-15.
        /// Write WL16 into Y16. Write WL16 into Y1 except:
        /// (1) when end around carry is inhibited by NEACON,
        /// (2) during SHINC sequence, or
        /// (3) PIFL is active and L15 = 1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WYD(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = (ushort)((cpu.WriteBus & 0x3FFF) << 1);    // WL bits 1-14 into Y bits 2-15
            cpu.AdderY |= (ushort)(cpu.WriteBus & 0x8000);  // WL bit 16 into Y bit 16
            // WL bit 16 into Y bit 1 if circumstances allow
            if (!cpu.NoEAC && !cpu.ShincSequence && !(cpu.PIFL && (cpu.RegisterL & 0x4000) > 0))
                cpu.AdderY |= (ushort)((cpu.WriteBus & 0x8000) >> 15);
            cpu.AdderCarry = false;
        }

        /// <summary>
        /// Clear and write WL1-16 int Z1-16.
        /// </summary>
        /// <param name="cpu"></param>
        public static void WZ(Cpu cpu)
        {
            cpu.RegisterZ = cpu.WriteBus;
        }

        /// <summary>
        /// Always implies RU, G2LS, and WALS.
        /// </summary>
        /// <param name="cpu"></param>
        public static void ZAP(Cpu cpu)
        {
            RU(cpu);
            G2LS(cpu);
            WALS(cpu);
        }

        /// <summary>
        /// Always implies A2X and L2GD. Also if L15,2,1 are:
        /// L15 L2 L1   READ    WRITE   CARRY   REMEMBER
        /// 
        ///  0   0  0   -       WY      -       -
        ///  0   0  1   RB      WY      -       -
        ///  0   1  0   RB      WYD     -       -
        ///  0   1  1   RC      WY      CI      MCRO
        ///  1   0  0   RB      WY      -       -
        ///  1   0  1   RB      WYD     -       -
        ///  1   1  0   RC      WY      CI      MCRO
        ///  1   1  1   -       WY      -       MCRO
        /// </summary>
        /// <param name="cpu"></param>
        /// <exception cref="InvalidDataException"></exception>
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

            // We need to do A2X at the end because the potential for WY above will clear the adder.
            A2X(cpu);
            L2GD(cpu);
        }
    }
}
