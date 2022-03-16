using System;
using System.Collections;
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

        public static void CCS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WB, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RZ, WY12 }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RZ, WY12, PONEX }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RZ, WY12, PTWOX }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RZ, WY12, PONEX, PTWOX }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RU, WZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WY, MONEX, CI, ST2 }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WY, ST2 }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RC, WY, MONEX, CI, ST2 }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WY, ST2 }));
            cpu.ControlPulseQueue.Enqueue((11, new List<ControlPulseFunc>() { RU, WA }));
        }

        public static void GOJ1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RSTRT, WS, WB }));
        }

        public static void TS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RA, WB, TOV }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RZ, WY12 }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RZ, WY12 }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB1, WA }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { R1C, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void XCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void LXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WL }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void QXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RQ, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WQ }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void TC0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RZ, WQ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS }));
        }

        public static void TCF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS }));
        }

        public static void STD2(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS }));
        }
    }
}
