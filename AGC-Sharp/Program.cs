using System.Buffers.Binary;
using System.Diagnostics;

namespace AGC_Sharp
{
    internal class Program
    {
        private const double CLOCK_FREQUENCY = 1024000.0;

        static void Main(string[] args)
        {
            // Initialize our CPU and Memory
            Cpu cpu = new();
            Memory memory = new(Memory.MemoryInitState.BitsClear);

            // Generate bitfield permutations for all implemented subinstructions
            ISA.SubinstructionHelper.PopulateDictionary();

            // Load core rope into fixed memory
            List<ushort> coreRope = new();
            BinaryReader binReader = new(new FileStream("Retread50.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
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
            long totalTicks = 0;
            while (true)
            {
                //if (systemClock.Elapsed.TotalSeconds >= (1 / CLOCK_FREQUENCY))
                //{
                    //systemClock.Restart();
                    cpu.Tick(memory);
                    ++totalTicks;
                //}
            }
        }
    }
}