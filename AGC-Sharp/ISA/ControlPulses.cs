using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.ISA
{
    internal static class ControlPulses
    {
        public static void CI(Cpu cpu)
        {
            cpu.AdderCarry = true;
        }

        public static void NISQ(Cpu cpu)
        {
            cpu.NextInstruction = true;
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
                    cpu.Extend = true;
                    RZ(cpu);
                    ST2(cpu);
                    break;
                default:
                    RG(cpu);
                    break;
            }
        }

        public static void RA(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterA;
        }

        public static void RB(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterB;
        }

        public static void RG(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterG;
        }

        public static void RL(Cpu cpu)
        {
            cpu.WriteBus |= cpu.RegisterL;
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
            ushort val = cpu.RegisterS switch
            {
                0 => cpu.RegisterA,
                1 => cpu.RegisterL,
                2 => cpu.RegisterQ,
                3 => cpu.RegisterEB,
                4 => cpu.RegisterFB,
                5 => cpu.RegisterZ,
                6 => (ushort)(cpu.RegisterFB | (cpu.RegisterEB >> 8)),
                _ => 0
            };
            cpu.WriteBus |= val;
        }

        public static void RU(Cpu cpu)
        {
            // Perform one's complement addition, store result in write bus
            uint temp = (uint)cpu.AdderX + (uint)cpu.AdderY;
            temp += ((temp >> 16) & 1) | (uint)(cpu.AdderCarry ? 1 : 0);    // Handle end-around-carry and explicit carry bit
            cpu.WriteBus |= (ushort)(temp & ushort.MaxValue);
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

        public static void TMZ(Cpu cpu)
        {
            if (cpu.WriteBus == 0xFFFF)
            {
                cpu.RegisterBR |= 2;
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
                cpu.RegisterBR |= 2;
            }
        }

        /// <summary>
        /// Test sign. Copy WL16 to BR1.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TSGN(Cpu cpu)
        {
            // Copy bit 16 of the write bus into BR bit 1, and implicitly preserve BR bit 2
            cpu.RegisterBR &= (byte)((cpu.WriteBus >> 15) | 2);
        }

        /// <summary>
        /// Test sign. Copy WL16 to BR2.
        /// </summary>
        /// <param name="cpu"></param>
        public static void TSGN2(Cpu cpu)
        {
            // Copy bit 16 of the write bus into BR bit 2, and implicitly preserve BR bit 1
            cpu.RegisterBR &= (byte)((cpu.WriteBus >> 14) | 1);
        }

        public static void WA(Cpu cpu)
        {
            cpu.RegisterA = cpu.WriteBus;
        }

        public static void WB(Cpu cpu)
        {
            cpu.RegisterB = cpu.WriteBus;
        }

        public static void WG(Cpu cpu)
        {
            if (cpu.RegisterS >= 0x10 && cpu.RegisterS <= 0x13)
            {
                // TODO: Special case, used for bit shifts and the like
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
                    // TODO: These may not be the correct bit shifts/masks
                    cpu.RegisterEB = (ushort)((cpu.WriteBus & 7) << 8);
                    cpu.RegisterFB = (ushort)(cpu.WriteBus & 0xF800);
                    break;
            };
        }

        public static void WQ(Cpu cpu)
        {
            cpu.RegisterQ = cpu.WriteBus;
        }

        public static void WY12(Cpu cpu)
        {
            cpu.AdderX = 0;
            cpu.AdderY = cpu.WriteBus;
            cpu.AdderCarry = false;
        }

        public static void WZ(Cpu cpu)
        {
            cpu.RegisterZ = cpu.WriteBus;
        }
    }
}
