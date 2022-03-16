using System.Diagnostics;

namespace AGC_Sharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize our Cpu and Memory
            Cpu cpu = new();
            Memory memory = new(Memory.MemoryInitState.Random);

            memory.WriteErasableBlock(new ushort[1], 2047);

            // The Stopwatch class provides a high-resolution timer
            // that should serve our needs for a 1.024 MHz clock.
            Stopwatch systemClock = new();
        }
    }
}