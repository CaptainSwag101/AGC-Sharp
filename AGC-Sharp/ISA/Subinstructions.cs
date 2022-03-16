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
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WA }));
        }
    }
}
