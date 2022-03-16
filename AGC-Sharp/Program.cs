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
            Memory memory = new(Memory.MemoryInitState.Random);

            memory.WriteErasableBlock(new ushort[1], 2046);

            // DEBUG: Run a test subinstruction
            ISA.Subinstructions.CA0(cpu);

            // The Stopwatch class provides a high-resolution timer
            // that should serve our needs for a 1.024 MHz clock.
            Stopwatch systemClock = new();
            systemClock.Start();

            while (true)
            {
                if (systemClock.Elapsed.TotalSeconds >= (1 / CLOCK_FREQUENCY))
                {
                    systemClock.Restart();
                    cpu.Tick();
                }
            }
        }
    }
}