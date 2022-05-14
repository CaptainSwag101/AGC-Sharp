using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.ISA.ControlPulses;
using static AGC_Sharp.ISA.Subinstructions;

namespace AGC_Sharp.ISA
{
    public delegate void SubinstructionFunc(Cpu cpu);

    public static class SubinstructionHelper
    {
        public static Dictionary<(byte Stage, bool Extend, byte Sequence), (string Name, SubinstructionFunc Func)> SubinstructionDictionary = new();

        private static List<(int Stage, bool Extend, string Sequence, string Name, SubinstructionFunc Function)> ImplementedSubinstructions = new()
        {
            (2, false, "xxxxxx", "STD2", STD2),
            (2, true, "xxxxxx", "STD2", STD2),
            (0, false, "000xxx", "TC0", TC0),
            (0, true, "00100x", "DV0", DV0),
            (1, true, "00100x", "DV1", DV1),
            (3, true, "00100x", "DV3", DV3),
            (7, true, "00100x", "DV7", DV7),
            (6, true, "00100x", "DV6", DV6),
            //(4, true, "00100x", "DV4", DV4),
            (0, false, "00101x", "TCF0", TCF0),
            (0, false, "00110x", "TCF0", TCF0),
            (0, false, "00111x", "TCF0", TCF0),
            (0, true,  "00101x", "BZF0", BZF0),
            (0, true,  "00110x", "BZF0", BZF0),
            (0, true,  "00111x", "BZF0", BZF0),
            (0, true,  "11001x", "BZMF0", BZMF0),
            (0, true,  "11010x", "BZMF0", BZMF0),
            (0, true,  "11011x", "BZMF0", BZMF0),
            (0, true,  "01000x", "MSU0", MSU0),
            (0, true,  "01010x", "AUG0", AUG0),
            (0, true,  "01011x", "DIM0", DIM0),
            (0, true,  "011xxx", "DCA0", DCA0),
            (1, true,  "011xxx", "DCA1", DCA1),
            (0, true,  "100xxx", "DCS0", DCS0),
            (1, true,  "100xxx", "DCS1", DCS1),
            (0, true,  "11000x", "SU0", SU0),
            (0, false, "111xxx", "MSK0", MSK0),
            (0, false, "00100x", "CCS0", CCS0),
            (0, false, "011xxx", "CA0", CA0),
            (0, false, "100xxx", "CS0", CS0),
            (0, false, "01000x", "DAS0", DAS0),
            (1, false, "01000x", "DAS1", DAS1),
            (0, false, "10110x", "TS0", TS0),
            (0, false, "10111x", "XCH0", XCH0),
            (0, false, "01001x", "LXCH0", LXCH0),
            (0, true,  "01001x", "QXCH0", QXCH0),
            (0, false, "10100x", "NDX0", NDX0),
            (1, false, "10100x", "NDX1", NDX1),
            (0, true,  "101xxx", "NDXX0", NDXX0),
            (1, true,  "101xxx", "NDXX1", NDXX1),
            (0, false, "110xxx", "AD0", AD0),
            (0, false, "01011x", "ADS0", ADS0),
            (0, false, "01010x", "INCR0", INCR0),
            (1, false, "000xxx", "GOJ1", GOJ1),
            (0, true,  "000011", "WAND0", WAND0),
            (0, true,  "000101", "WOR0", WOR0),
            (0, false, "10101x", "DXCH0", DXCH0),
            (1, false, "10101x", "DXCH1", DXCH1),
            (0, true,  "111xxx", "MP0", MP0),
            (1, true,  "111xxx", "MP1", MP1),
            (3, true,  "111xxx", "MP3", MP3),
        };

        public static void PopulateDictionary()
        {
            // First, generate a list of all 7-bit unsigned integers
            // to create all possible bit permutations we might encounter.
            byte permutationCount = (byte)Math.Pow(2, 6);

            // Now, for stages 0 through 3, Extend and without, assign STD2 as a placeholder
            // so we always attempt to skip ahead to the next instruction if
            // the current one isn't implemented.
            for (int extendState = 0; extendState <= 1; ++extendState)
            {
                for (byte stage = 0; stage <= 7; ++stage)
                {
                    for (byte i = 0; i < permutationCount; ++i)
                    {
                        SubinstructionDictionary.Add((stage, (extendState == 1), i), ("STD2", STD2));
                    }
                }
            }

            // Now that we have a fully-populated dictionary, replace any
            // implemented subinstructions in the dictionary.
            foreach (var implemented in ImplementedSubinstructions)
            {
                // Generate a bit mask from the sequence string
                byte maskPattern = 0;
                byte patternToMatch = 0;
                for (byte i = 0; i < implemented.Sequence.Length; ++i)
                {
                    char c = implemented.Sequence[i];

                    if (c != 'x')
                    {
                        maskPattern |= (byte)(1 << (5 - i));
                    }
                    
                    if (c == '1')
                    {
                        patternToMatch |= (byte)(1 << (5 - i));
                    }
                }

                _ = 0;

                foreach (var sub in SubinstructionDictionary)
                {
                    // If the key matches, replace its instruction function with the proper one
                    if (sub.Key.Stage == implemented.Stage
                        && sub.Key.Extend == implemented.Extend
                        && (byte)(sub.Key.Sequence & maskPattern) == patternToMatch)
                    {
                        SubinstructionDictionary[sub.Key] = (implemented.Name, implemented.Function);
                    }
                }
            }
        }
    }

    internal static class Subinstructions
    {
        public static void AD0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RB, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));
        }

        public static void ADS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WSC, WG, TOV }));
            cpu.ControlPulseQueue.Enqueue((7, new() { WA }));       // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { WA, RB1 }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { WA, R1C }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { WA }));       // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, TMZ }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));
        }

        public static void AUG0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WY, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((6, new() { PONEX }));    // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { PONEX }));    // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { MONEX }));    // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { MONEX }));    // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RU, WSC, WG, WOVR }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void BZF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RA, WG, TSGN, TMZ }));
            cpu.ControlPulseQueue.Enqueue((2, new() { TPZG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() {  }));             // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY12, CI })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() {  }));             // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY12, CI })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() {  }));         // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() {  }));         // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));          // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS, NISQ }));    // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));          // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS, NISQ }));    // BR1 = 0, BR2 = 0
        }

        public static void BZMF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RA, WG, TSGN, TMZ }));
            cpu.ControlPulseQueue.Enqueue((2, new() { TPZG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() {  }));             // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY12, CI })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY12, CI })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY12, CI })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() {  }));         // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));          // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS, NISQ }));    // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS, NISQ }));    // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS, NISQ }));    // BR1 = 1, BR2 = 1
        }

        public static void CA0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RB, WA }));
        }

        public static void CS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WA }));
        }

        public static void CCS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RZ, WY12 }));                 // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RZ, WY12, PONEX }));          // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RZ, WY12, PTWOX }));          // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RZ, WY12, PONEX, PTWOX }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RB, WY, MONEX, CI, ST2 }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { WY, ST2 }));                 // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WY, MONEX, CI, ST2 }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { WY, ST2 }));                 // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));
        }

        public static void DAS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS, WY12, MONEX, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RL, WA }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RU, WL }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WA }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RU, WSC, WG, TOV }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, ST1 }));         // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, ST1, PONEX }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, ST1, MONEX }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, ST1 }));         // BR1 = 1, BR2 = 1
        }

        public static void DAS1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WA }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WG, WSC, TOV }));
            cpu.ControlPulseQueue.Enqueue((7, new() { WA }));       // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RB1, WA }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { R1C, WA }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { WA }));       // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, TMZ }));
            cpu.ControlPulseQueue.Enqueue((10, new() { WL }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));    // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { WL }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));    // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() {  }));        // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() {  }));        // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));  // BR1 = 1, BR2 = 1
        }

        public static void DCA0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RB, WY12, MONEX, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RB, WL, ST1 }));
        }

        public static void DCA1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RB, WA }));
        }

        public static void DCS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RB, WY12, MONEX, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WL, ST1 }));
        }

        public static void DCS1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WA }));
        }

        public static void DIM0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WY, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((6, new() { MONEX }));    // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() {  }));         // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { PONEX }));    // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((6, new() {  }));         // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RU, WSC, WG, WOVR }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void DXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS, WY12, MONEX, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WL }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WS, WB }));
            cpu.ControlPulseQueue.Enqueue((10, new() { ST1 }));
        }

        public static void DXCH1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void DV0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RA, WB, TSGN, TMZ }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RC, WA, TMZ, DVST }));    // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RC, WA, TMZ, DVST }));    // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((2, new() { DVST }));                 // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { DVST }));                 // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WB, STAGE }));
            // Shifted in from real DV1
            cpu.ControlPulseQueue.Enqueue((4, new() { RL, WB }));       // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RL, WB, TSGN })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((4, new() { RL, WB }));       // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RL, WB, TSGN })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY, B15X }));         // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, WY, B15X }));         // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RC, WY, B15X, Z16 }));    // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RC, WY, B15X, Z16 }));    // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WL, TOV }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, RSC, WB, TSGN }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RA, WY, PONEX }));    // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RA, WY }));           // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RA, WY, PONEX }));    // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RA, WY }));           // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WA }));           // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WA }));           // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, WA, Z15 }));      // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, WA, Z15 }));      // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RL, WYD }));
            cpu.ControlPulseQueue.Enqueue((12, new() { RU, WL }));
            // Continued in real DV1
        }

        public static void DV1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WB, STAGE }));
            // Shifted in from real DV3
            cpu.ControlPulseQueue.Enqueue((4, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((7, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((10, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((12, new() { RU, WB }));
            // Continued in real DV3
        }

        public static void DV3(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WB, STAGE }));
            // Shifted in from real DV7
            cpu.ControlPulseQueue.Enqueue((4, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((7, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((10, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((12, new() { RU, WB }));
            // Continued in real DV7
        }

        public static void DV7(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WB, STAGE }));
            // Shifted in from real DV6
            cpu.ControlPulseQueue.Enqueue((4, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((7, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, CLXC, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((8, new() { RG, TSGU, RB1F, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((10, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, CLXC, WL }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RG, TSGU, RB1F, WL }));  // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((12, new() { RU, WB }));
            // Continued in real DV6
        }

        public static void DV6(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, CLXC, WL, DVST })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((2, new() { RG, TSGU, RB1F, WL, DVST })); // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((3, new() { RU, WB, STAGE }));
            // Shifted in from real DV4
            cpu.ControlPulseQueue.Enqueue((4, new() { L2GD, RB, PIFL, WYD, A2X }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB, WA, TSGU, CLXC }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB, WA, TSGU, CLXC }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB, WA, TSGU, RB1F }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB, WA, TSGU, RB1F }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RZ, TOV }));
            cpu.ControlPulseQueue.Enqueue((7, new() { }));          // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RC, WA }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RC, WA }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RC, WA }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2, TSGN, RSTSTG }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RU, WB, WL }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WL }));  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WL }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));        // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));        // BR1 = 1, BR2 = 1
            // DV4 is gone because it's been totally integrated into DV6 here
        }

        public static void GOJ1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RSTRT, WS, WB }));
        }

        public static void INCR0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WY, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((6, new() { PONEX }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RU, WSC, WG, WOVR }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void LXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WL }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void MP0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB, TSGN }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RB, WL }));   // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RB, WL }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((4, new() { RC, WL }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RC, WL }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB, TSGN2 }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WY }));       // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WY, CI }));   // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, WY, CI }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, WY }));       // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { RU, WB, TSGN, ST1, NEACON }));
            cpu.ControlPulseQueue.Enqueue((11, new() { WA }));                  // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { WA }));                  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RB1, R1C, WA, L16 }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RB1, R1C, WA, L16 }));   // BR1 = 1, BR2 = 1
        }

        public static void MP1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { ZIP }));
            cpu.ControlPulseQueue.Enqueue((2, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((3, new() { ZIP }));
            cpu.ControlPulseQueue.Enqueue((4, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((5, new() { ZIP }));
            cpu.ControlPulseQueue.Enqueue((6, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((7, new() { ZIP }));
            cpu.ControlPulseQueue.Enqueue((8, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((9, new() { ZIP }));
            cpu.ControlPulseQueue.Enqueue((10, new() { ZAP, ST1, ST2 }));
            cpu.ControlPulseQueue.Enqueue((11, new() { ZIP }));
        }

        public static void MP3(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((2, new() { ZIP, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new() { ZAP }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ, TL15 })); // NEACOF should be here but not really, EAC is inhibited until end of MP3 due to original hardware logic bugs
            cpu.ControlPulseQueue.Enqueue((7, new() {  }));             // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() {  }));             // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WY, A2X }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WY, A2X }));  // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RA }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RL }));
            cpu.ControlPulseQueue.Enqueue((11, new() {  }));        // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() {  }));        // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));  // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((12, new() { NEACOF }));  // Hack to release NoEAC when real hardware does
        }

        public static void MSK0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RC, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RC, RA, WY }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RU, WB }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RC, WA }));
        }

        public static void MSU0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RC, WY, CI, A2X }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RUS, WA, TSGN }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));                // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() {  }));                // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, MONEX }));   // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((10, new() { RA, WY, MONEX }));   // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((11, new() { RUS, WA }));
        }

        public static void NDX0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new() { TRSM }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { ST1 }));
        }

        public static void NDX1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RB, WZ }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RZ, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WA }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RU, WB }));
        }

        public static void NDXX0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { ST1 }));
        }

        public static void NDXX1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RB, WZ }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RZ, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RU, WS }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WA }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RU, WB, EXT }));
        }

        public static void QXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RQ, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WQ }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void STD2(Cpu cpu)
        {
            // Include a debug message when we are executing STD2 while not in stage 2
            // to help debug unimplemented instructions.
            if (cpu.RegisterST != 2)
                Console.WriteLine($"Unimplemented instruction " +
                    $"{Convert.ToString(cpu.RegisterB, 8)} (Extend == {cpu.Extend}, Stage = {cpu.RegisterST}) " +
                    $"at {Convert.ToString(cpu.RegisterZ - 1, 8)}");

            cpu.ControlPulseQueue.Enqueue((1, new() { RZ, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS }));
        }

        public static void SU0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new() { RC, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new() { RU, WA }));
        }

        public static void TC0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RZ, WQ }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS }));
        }

        public static void TCF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG, NISQ }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RAD, WB, WS }));
        }

        public static void TS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB, TOV }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RZ, WY12 }));     // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RZ, WY12, CI })); // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((4, new() { RZ, WY12, CI })); // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((4, new() { RZ, WY12 }));     // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { }));          // BR1 = 0, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { RB1, WA }));  // BR1 = 0, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((5, new() { R1C, WA }));  // BR1 = 1, BR2 = 0
            cpu.ControlPulseQueue.Enqueue((5, new() { }));          // BR1 = 1, BR2 = 1
            cpu.ControlPulseQueue.Enqueue((6, new() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void WAND0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RC, WY }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RCH, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RC, RU, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RC, WA, WCH }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void WOR0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RB, WY }));
            cpu.ControlPulseQueue.Enqueue((4, new() { RCH, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RB, RU, WA, WCH }));
            cpu.ControlPulseQueue.Enqueue((6, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }

        public static void XCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new() { RG, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new() { RZ, WS, ST2 }));
        }
    }
}
