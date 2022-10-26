using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal class Memory : IMemory
    {
        private readonly word[] erasableMem;
        private readonly word[] fixedMem;

        public Memory()
        {
            erasableMem = new word[Octal(2000)];
            fixedMem = new word[Octal(60000)];
        }

        public word ReadMemory(word address)
        {
            return 0;
        }

        public void WriteMemory(word address)
        {

        }
    }
}
