using System.Buffers.Binary;
using System.Diagnostics;

namespace AGC_Sharp
{
    internal class Program
    {
        private const double CLOCK_FREQUENCY = 1_024_000.0;   // 1.024 MHz, derived from 2.048 MHz crystal

        static void Main(string[] args)
        {
            // Initialize our CPU and Memory
            Cpu cpu = new();
            Memory memory = new(Memory.MemoryInitState.BitsClear);

            // Generate bitfield permutations for all implemented subinstructions
            ISA.SubinstructionHelper.PopulateDictionary();

            // Load core rope into fixed memory
            List<ushort> coreRope = new();
            BinaryReader binReader = new(new FileStream("Ropes/Retread50.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
            while (binReader.BaseStream.Position < binReader.BaseStream.Length)
            {
                // The AGC's memory is big-endian so we need to swap the endianness when loading each word
                coreRope.Add(BinaryPrimitives.ReverseEndianness(binReader.ReadUInt16()));
            }
            memory.WriteFixedBlock(coreRope.ToArray());

            // DEBUG: Hack in a partial GOJAM so we start execution at the start of the rope
            ISA.Subinstructions.GOJ1(cpu);

            // The Stopwatch class provides a high-resolution timer
            // that should serve our needs for a 1.024 MHz clock.
            Stopwatch systemClock = new();
            systemClock.Start();

            // Main execution loop
            ulong totalTicks = 0;
            while (true)
            {
                // Batch the 12 time pulses per MCT together
                if (systemClock.Elapsed.TotalSeconds >= (1 / (CLOCK_FREQUENCY / 12.0)))
                {
                    for (int t = 1; t <= 12; ++t)
                    {
                        cpu.Tick(memory);
                        ++totalTicks;
                    }
                    systemClock.Restart();
                }
            }
        }
    }
}