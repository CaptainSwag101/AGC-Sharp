using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.ISA
{
    internal static class ControlPulses
    {
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
    }
}
