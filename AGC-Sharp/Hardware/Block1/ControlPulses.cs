using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.Bitmasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal partial class CPU
    {
        public static void CI(CPU cpu)
        {
            cpu.ExplicitCarry = true;
            cpu.UpdateAdder();
        }

        public static void NISQ(CPU cpu)
        {
            cpu.FetchNextInstruction = true;
        }

        public static void RB(CPU cpu)
        {
            cpu.WriteBus |= cpu.B;
        }

        public static void RC(CPU cpu)
        {
            cpu.WriteBus |= (word)(~cpu.B);
        }

        public static void RG(CPU cpu)
        {
            cpu.WriteBus |= cpu.G;
        }

        public static void RSC(CPU cpu)
        {
            switch (cpu.S)
            {
                case 0:
                    cpu.WriteBus |= cpu.A;
                    break;
                case 1:
                    cpu.WriteBus |= cpu.Q;
                    break;
                case 2:
                    cpu.WriteBus |= cpu.Z;
                    break;
                case 3:
                    cpu.WriteBus |= cpu.LP;
                    break;
                case 4:
                    cpu.WriteBus |= cpu.IN0;
                    break;
                case 5:
                    cpu.WriteBus |= cpu.IN1;
                    break;
                case 6:
                    cpu.WriteBus |= cpu.IN2;
                    break;
                case 7:
                    cpu.WriteBus |= cpu.IN3;
                    break;
                case 8:
                    cpu.WriteBus |= cpu.OUT0;
                    break;
                case 9:
                    cpu.WriteBus |= cpu.OUT1;
                    break;
                case 10:
                    cpu.WriteBus |= cpu.OUT2;
                    break;
                case 11:
                    cpu.WriteBus |= cpu.OUT3;
                    break;
                case 12:
                    cpu.WriteBus |= cpu.OUT4;
                    break;
                case 13:
                    cpu.WriteBus |= cpu.BNK;
                    break;
            };
        }

        public static void RU(CPU cpu)
        {
            cpu.WriteBus |= cpu.U;
        }

        public static void RZ(CPU cpu)
        {
            cpu.WriteBus |= cpu.Z;
        }

        public static void WB(CPU cpu)
        {
            cpu.B = cpu.WriteBus;
        }

        public static void WG(CPU cpu)
        {
            word temp = cpu.WriteBus;

            word correctS = (cpu.SBuffer > 0) ? cpu.SBuffer : cpu.S;
            if (correctS >= 020 && correctS <= 023)
            {
                switch (correctS)
                {
                    case 020:   // Cycle Right
                        {
                            word bottom_to_top = (word)((cpu.WriteBus & 1) << 15); // Cycle bit 1 to bit 16
                            temp = (word)(((cpu.WriteBus & ~BITMASK_15_16) >> 1) | bottom_to_top);    // Mask out bits 15 and 16 before shifting so they are blank afterwards
                            temp |= (word)((cpu.WriteBus & BITMASK_16) >> 2);  // Copy the old bit 16 into bit 14
                            break;
                        }
                    case 021:   // Shift Right
                        {
                            word top_bit = (word)(cpu.WriteBus & BITMASK_16);    // Remember bit 16
                            temp = (word)(((cpu.WriteBus & ~BITMASK_15_16) >> 1) | top_bit);  // Mask out bits 15 and 16 before shifting so they are blank afterwards
                            temp |= (word)((cpu.WriteBus & BITMASK_16) >> 2);  // Copy the old bit 16 into bit 14
                            break;
                        }
                    case 022:   // Cycle Left
                        {
                            word top_to_bottom = (word)((cpu.WriteBus & BITMASK_16) >> 15); // Cycle the most significant bit to the least
                            word new_top = (word)((cpu.WriteBus & BITMASK_14) << 2); // Remember bit 14 and double-shift it so it ends up in bit 16
                            temp = (word)(((cpu.WriteBus & ~BITMASK_14_15) << 1) | top_to_bottom | new_top);  // Mask out bits 14 and 15 before shifting so they are blank afterwards
                            break;
                        }
                    case 023:  // Shift Left
                        {
                            word top_bit = (word)(cpu.WriteBus & BITMASK_16);    // Remember bit 16
                            temp <<= 1;
                            unchecked // Necessary because C# doesn't like inverting bit 16 even on explicitly unsigned integers
                            {
                                temp &= (word)(~BITMASK_16);
                            }
                            temp |= top_bit;    // Reuse old bit 16; THIS MAY BE INCORRECT LOGIC!!!!
                            break;
                        }
                }
            }

            cpu.G = temp;
        }

        public static void WS(CPU cpu)
        {
            cpu.S = (word)(cpu.WriteBus & BITMASK_1_12);
        }

        public static void WSC(CPU cpu)
        {
            switch (cpu.S)
            {
                case 0:
                    cpu.A = cpu.WriteBus;
                    break;
                case 1:
                    cpu.Q = cpu.WriteBus;
                    break;
                case 2:
                    cpu.Z = cpu.WriteBus;
                    break;
                case 3:
                    cpu.LP = cpu.WriteBus;
                    break;
                case 4:
                    cpu.IN0 = cpu.WriteBus;
                    break;
                case 5:
                    cpu.IN1 = cpu.WriteBus;
                    break;
                case 6:
                    cpu.IN2 = cpu.WriteBus;
                    break;
                case 7:
                    cpu.IN3 = cpu.WriteBus;
                    break;
                case 8:
                    cpu.OUT0 = cpu.WriteBus;
                    break;
                case 9:
                    cpu.OUT1 = cpu.WriteBus;
                    break;
                case 10:
                    cpu.OUT2 = cpu.WriteBus;
                    break;
                case 11:
                    cpu.OUT3 = cpu.WriteBus;
                    break;
                case 12:
                    cpu.OUT4 = cpu.WriteBus;
                    break;
                case 13:
                    cpu.BNK = cpu.WriteBus;
                    break;
            };
        }

        public static void WY(CPU cpu)
        {
            cpu.Y = cpu.WriteBus;
            cpu.UpdateAdder();
        }

        public static void WZ(CPU cpu)
        {
            cpu.Z = cpu.WriteBus;
        }
    }
}
