using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.ISA.ControlPulses;

namespace AGC_Sharp.ISA
{
    internal static class Subinstructions
    {
        public static void CA0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
        }
    }
}
