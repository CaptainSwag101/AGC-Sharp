using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.ISA.ControlPulses;

namespace AGC_Sharp.ISA
{
    internal delegate void SubinstructionFunc(Cpu cpu);

    internal static class SubinstructionHelper
    {
        public static Dictionary<(byte Stage, bool Extend, byte Sequence), SubinstructionFunc> SubinstructionDictionary = new();

        private static List<(int Stage, bool Extend, string Sequence, SubinstructionFunc Function)> ImplementedSubinstructions = new()
        {
            (3, false, "xxxxxxx", Subinstructions.STD2),
            (3, true, "xxxxxxx", Subinstructions.STD2),
            (2, false, "xxxxxxx", Subinstructions.STD2),
            (2, true, "xxxxxxx", Subinstructions.STD2),
            (1, false, "xxxxxxx", Subinstructions.STD2),
            (1, true, "xxxxxxx", Subinstructions.STD2),
            (0, false, "xxxxxxx", Subinstructions.STD2),
            (0, true, "xxxxxxx", Subinstructions.STD2),
            (0, false, "0000xxx", Subinstructions.TC0),
            (0, false, "000101x", Subinstructions.TCF0),
            (0, false, "000110x", Subinstructions.TCF0),
            (0, false, "000111x", Subinstructions.TCF0),
        };

        public static void PopulateDictionary()
        {
            // First, generate a list of all 7-bit unsigned integers
            // to create all possible bit permutations we might encounter.
            byte permutationCount = (byte)Math.Pow(2, 7);

            // Now, for stages 0 through 3, Extend and without, assign STD2 as a placeholder
            // so we always attempt to skip ahead to the next instruction if
            // the current one isn't implemented.
            for (int extendState = 0; extendState <= 1; ++extendState)
            {
                for (byte stage = 0; stage <= 3; ++stage)
                {
                    for (byte i = 0; i < permutationCount; ++i)
                    {
                        SubinstructionDictionary.Add((stage, (extendState == 1), i), Subinstructions.STD2);
                    }
                }
            }

            // Now that we have a fully-populated dictionary, replace any
            // implemented subinstructions in the dictionary.
            foreach (var implemented in ImplementedSubinstructions)
            {
                // Generate a bit mask from the sequence string
                byte skippableBits = 0;
                byte patternToMatch = 0;
                for (byte i = 0; i < 7; ++i)
                {
                    char c = implemented.Sequence[i];

                    if (c == 'x')
                    {
                        ++skippableBits;
                    }
                    else if (c == '1')
                    {
                        patternToMatch |= (byte)(1 << i);
                    }
                }

                foreach (var sub in SubinstructionDictionary)
                {
                    // If the key matches, replace its instruction function with the proper one
                    if (sub.Key.Stage == implemented.Stage
                        && sub.Key.Extend == implemented.Extend
                        && (byte)(sub.Key.Sequence >> skippableBits) == patternToMatch)
                    {
                        SubinstructionDictionary[sub.Key] = implemented.Function;
                    }
                }
            }
        }
    }

    internal static class Subinstructions
    {
        public static void AD0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new List<ControlPulseFunc>() { RU, WA }));
        }

        public static void CA0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WA }));
        }

        public static void CS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RC, WA }));
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

        public static void LXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WL }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void NDX0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { TRSM }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { ST1 }));
        }

        public static void NDX1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RB, WZ }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RZ, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RU, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WA }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RU, WB }));
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

        public static void STD2(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS }));
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
    }
}
