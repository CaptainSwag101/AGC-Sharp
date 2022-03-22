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
        public static Dictionary<(byte Stage, bool Extend, byte Sequence), (string Name, SubinstructionFunc Func)> SubinstructionDictionary = new();

        private static List<(int Stage, bool Extend, string Sequence, string Name, SubinstructionFunc Function)> ImplementedSubinstructions = new()
        {
            (2, false, "xxxxxx", "STD2", Subinstructions.STD2),
            (2, true,  "xxxxxx", "STD2", Subinstructions.STD2),
            (0, false, "000xxx", "TC0", Subinstructions.TC0),
            (0, false, "00101x", "TCF0", Subinstructions.TCF0),
            (0, false, "00110x", "TCF0", Subinstructions.TCF0),
            (0, false, "00111x", "TCF0", Subinstructions.TCF0),
            (0, true,  "00101x", "BZF0", Subinstructions.BZF0),
            (0, true,  "00110x", "BZF0", Subinstructions.BZF0),
            (0, true,  "00111x", "BZF0", Subinstructions.BZF0),
            (0, true,  "11001x", "BZMF0", Subinstructions.BZMF0),
            (0, true,  "11010x", "BZMF0", Subinstructions.BZMF0),
            (0, true,  "11011x", "BZMF0", Subinstructions.BZMF0),
            (0, false, "111xxx", "MSK0", Subinstructions.MSK0),
            (0, false, "00100x", "CCS0", Subinstructions.CCS0),
            (0, false, "011xxx", "CA0", Subinstructions.CA0),
            (0, false, "100xxx", "CS0", Subinstructions.CS0),
            (0, false, "10110x", "TS0", Subinstructions.TS0),
            (0, false, "10111x", "XCH0", Subinstructions.XCH0),
            (0, false, "01001x", "LXCH0", Subinstructions.LXCH0),
            (0, true,  "01001x", "QXCH0", Subinstructions.QXCH0),
            (0, false, "10100x", "NDX0", Subinstructions.NDX0),
            (1, false, "10100x", "NDX1", Subinstructions.NDX1),
            (0, false, "110xxx", "AD0", Subinstructions.AD0),
            (0, false, "01011x", "ADS0", Subinstructions.ADS0),
            (0, false, "01010x", "INCR0", Subinstructions.INCR0),
            (1, false, "000xxx", "GOJ1", Subinstructions.GOJ1),
            (0, true,  "000011", "WAND0", Subinstructions.WAND0),
            (0, true,  "000101", "WOR0", Subinstructions.WOR0),
            (0, false, "10101x", "DXCH0", Subinstructions.DXCH0),
            (1, false, "10101x", "DXCH1", Subinstructions.DXCH1),
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
                for (byte stage = 0; stage <= 3; ++stage)
                {
                    for (byte i = 0; i < permutationCount; ++i)
                    {
                        SubinstructionDictionary.Add((stage, (extendState == 1), i), ("STD2", Subinstructions.STD2));
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
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RB, WG }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { RB, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((11, new List<ControlPulseFunc>() { RU, WA }));
        }

        public static void ADS0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WY, A2X }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WSC, WG, TOV }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { WA }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { WA, RB1 }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { WA, R1C }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((9, new List<ControlPulseFunc>() { RC, TMZ }));
            cpu.ControlPulseQueue.Enqueue((11, new List<ControlPulseFunc>() { RU, WA }));
        }

        public static void BZF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RA, WG, TSGN, TMZ }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { TPZG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS, NISQ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS, NISQ }));
        }

        public static void BZMF0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RA, WG, TSGN, TMZ }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { TPZG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, WY12, CI }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() {  }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS, NISQ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS, NISQ }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RAD, WB, WS, NISQ }));
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

        public static void DXCH0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS, WY12, MONEX, CI }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RL, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WL }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RU, WS, WB }));
            cpu.ControlPulseQueue.Enqueue((10, new List<ControlPulseFunc>() { ST1 }));
        }

        public static void DXCH1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RSC, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void GOJ1(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RSTRT, WS, WB }));
        }

        public static void INCR0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RG, WY, TSGN, TMZ, TPZG }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { PONEX }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RU, WSC, WG, WOVR }));
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

        public static void MSK0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RSC, WG }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RC, WA }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RG, WB }));
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
            // Include a debug message when we are executing STD2 while not in stage 2
            // to help debug unimplemented instructions.
            if (cpu.RegisterST != 2)
                Console.WriteLine($"Unimplemented instruction " +
                    $"0o{Convert.ToString(cpu.RegisterB, 8)} (Extend == {cpu.Extend}, Stage = {cpu.RegisterST}) " +
                    $"at {Convert.ToString(cpu.RegisterZ - 1, 8)}");

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
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB1, WA }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { R1C, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RU, WZ }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RB, WSC, WG }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void WAND0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RC, WY }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RCH, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RC, RU, WA }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((7, new List<ControlPulseFunc>() { RC, WA, WCH }));
            cpu.ControlPulseQueue.Enqueue((8, new List<ControlPulseFunc>() { RZ, WS, ST2 }));
        }

        public static void WOR0(Cpu cpu)
        {
            cpu.ControlPulseQueue.Enqueue((1, new List<ControlPulseFunc>() { RL10BB, WS }));
            cpu.ControlPulseQueue.Enqueue((2, new List<ControlPulseFunc>() { RA, WB }));
            cpu.ControlPulseQueue.Enqueue((3, new List<ControlPulseFunc>() { RB, WY }));
            cpu.ControlPulseQueue.Enqueue((4, new List<ControlPulseFunc>() { RCH, WB }));
            cpu.ControlPulseQueue.Enqueue((5, new List<ControlPulseFunc>() { RB, RU, WA, WCH }));
            cpu.ControlPulseQueue.Enqueue((6, new List<ControlPulseFunc>() { RA, WB }));
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
