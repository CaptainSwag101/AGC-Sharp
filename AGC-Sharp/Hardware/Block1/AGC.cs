using System.Buffers.Binary;
using System.Diagnostics;

namespace AGC_Sharp.Hardware.Block1
{
    internal class AGC : Computer
    {
        public static string[] Names = { "Block1", "BlockI", "AGC4" };
        public CPU Cpu;
        public Memory Memory;
        public Scaler Scaler;
        public Timer Timer;

        public override void Initialize(string ropeFile)
        {
            // Initialize the CPU
            Cpu = new CPU(this);


            // Intitialize memory
            Memory = new Memory(this);

            // Read words from the rope and convert from big-endian, then write to fixed memory.
            if (ropeFile is not null)
            {
                using BinaryReader ropeReader = new(new FileStream(ropeFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                word memAddr = 0;
                while (ropeReader.BaseStream.Position < ropeReader.BaseStream.Length)
                {
                    Memory.WriteFixed(memAddr++, BinaryPrimitives.ReverseEndianness(ropeReader.ReadUInt16()));
                }
            }


            // Initialize scaler
            Scaler = new Scaler(this);


            // Initialize timer
            Timer = new Timer(this);
        }

        public override void Execute()
        {
            Timer.Start();
        }
    }
}
