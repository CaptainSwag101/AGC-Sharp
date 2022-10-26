using System.Buffers.Binary;

namespace AGC_Sharp.Hardware.Block1
{
    internal class AGC : Computer
    {
        public static string[] Names = { "Block1", "BlockI", "AGC4" };
        private CPU cpu;
        private Memory memory;
        private Scaler scaler;
        private Timer timer;

        public AGC(string ropeFile)
        {
            // Initialize the CPU
            cpu = new CPU();


            // Intitialize memory
            memory = new Memory();

            // Read words from the rope and convert from big-endian, then write to fixed memory.
            using BinaryReader ropeReader = new(new FileStream(ropeFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            word memAddr = 0;
            while (ropeReader.BaseStream.Position < ropeReader.BaseStream.Length)
            {
                memory.WriteFixed(memAddr++, BinaryPrimitives.ReverseEndianness(ropeReader.ReadUInt16()));
            }


            // Initialize scaler
            scaler = new Scaler();


            // Initialize timer
            timer = new Timer();
        }

        public override void Execute()
        {

        }
    }
}
