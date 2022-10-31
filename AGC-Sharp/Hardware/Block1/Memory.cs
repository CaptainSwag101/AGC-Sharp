using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal class Memory
    {
        public const word MEM_ERASABLE_END = 1023;          // Octal 1777
        public const word MEM_FIXED_BANKED_START = 3072;    // Octal 6000
        public const word MEM_FIXED_BANKED_END = 4095;      // Octal 7777

        private readonly word[] erasableMem;
        private readonly word[] fixedMem;
        private AGC agcReference;

        public Memory(AGC agc)
        {
            agcReference = agc;

            erasableMem = new word[Octal(2000)];
            fixedMem = new word[Octal(60000)];
        }

        public word ReadErasable(word address)
        {
            word temp = erasableMem[address];
            erasableMem[address] = 0;   // Erasable reads are destructive
            return temp;
        }

        public void WriteErasable(word address, word data) => erasableMem[address] = data;

        public word ReadFixed(word address) => fixedMem[address];

        public void WriteFixed(word address, word data) => fixedMem[address] = data;
    }
}
