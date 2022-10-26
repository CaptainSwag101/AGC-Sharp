using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware
{
    internal interface IMemory
    {
        public word ReadMemory(word address);
        public void WriteMemory(word address);
    }
}
