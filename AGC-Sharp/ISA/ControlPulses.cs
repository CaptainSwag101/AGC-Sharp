using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.ISA
{
    internal static class ControlPulses
    {
        public static void RB(Cpu cpu)
        {
            cpu.WriteBus = cpu.RegisterB;
        }

        public static void RG(Cpu cpu)
        {
            cpu.WriteBus = cpu.RegisterG;
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
            cpu.WriteBus = val;
        }

        public static void RZ(Cpu cpu)
        {
            cpu.WriteBus = cpu.RegisterZ;
        }

        public static void ST1(Cpu cpu)
        {
            cpu.RegisterST = 1; // TODO: THIS IS PROBABLY WRONG!
        }

        public static void ST2(Cpu cpu)
        {
            cpu.RegisterST = 2; // TODO: THIS IS PROBABLY WRONG!
        }

        public static void WA(Cpu cpu)
        {
            cpu.RegisterA = cpu.WriteBus;
            cpu.WriteBus = 0;
        }

        public static void WB(Cpu cpu)
        {
            cpu.RegisterB = cpu.WriteBus;
            cpu.WriteBus = 0;
        }

        public static void WG(Cpu cpu)
        {
            if (cpu.RegisterS >= 0x10 && cpu.RegisterS <= 0x13)
            {
                // TODO: Special case
            }
            else
            {
                cpu.RegisterG = cpu.WriteBus;
                cpu.WriteBus = 0;
            }
        }

        public static void WS(Cpu cpu)
        {
            cpu.RegisterS = (ushort)(cpu.WriteBus & 0x0FFF);
            cpu.WriteBus = 0;
        }

        public static void WZ(Cpu cpu)
        {
            cpu.RegisterZ = cpu.WriteBus;
            cpu.WriteBus = 0;
        }
    }
}
