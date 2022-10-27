using System.Buffers.Binary;
using System.Diagnostics;

namespace AGC_Sharp.Hardware.Block1
{
    internal class AGC : Computer
    {
        public static string[] Names = { "Block1", "BlockI", "AGC4" };
        private CPU cpu;
        private Memory memory;
        private Scaler scaler;
        private Timer timer;
        private readonly TimeSpan EXECUTION_BATCH_INTERVAL = new(0, 0, 0, 1, 0);
        private const long EXECUTION_BATCH_TICKS_PER_INTERVAL = 1024000;
        private Stopwatch executionBatcher = new();

        public AGC(string ropeFile)
        {
            // Initialize the CPU
            cpu = new CPU();


            // Intitialize memory
            memory = new Memory();

            // Read words from the rope and convert from big-endian, then write to fixed memory.
            if (ropeFile is not null)
            {
                using BinaryReader ropeReader = new(new FileStream(ropeFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                word memAddr = 0;
                while (ropeReader.BaseStream.Position < ropeReader.BaseStream.Length)
                {
                    memory.WriteFixed(memAddr++, BinaryPrimitives.ReverseEndianness(ropeReader.ReadUInt16()));
                }
            }


            // Initialize scaler
            scaler = new Scaler();


            // Initialize timer
            timer = new Timer();
        }

        public override void Execute()
        {
            executionBatcher.Start();
            for (long i = 0; i < EXECUTION_BATCH_TICKS_PER_INTERVAL; ++i)
            {
                cpu.Tick();
            }
            executionBatcher.Stop();

            // Sleep for remaining time of interval.
            TimeSpan remainingTime = EXECUTION_BATCH_INTERVAL - executionBatcher.Elapsed;
            double ratioToRealtime = EXECUTION_BATCH_INTERVAL.TotalMilliseconds / executionBatcher.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Finished {EXECUTION_BATCH_TICKS_PER_INTERVAL} ticks in {executionBatcher.Elapsed.TotalMilliseconds}ms, {ratioToRealtime}x real-time speed.");

            if (remainingTime.TotalMilliseconds > 0)
            {
                Thread.Sleep(remainingTime);
            }

            executionBatcher.Reset();
        }
    }
}
