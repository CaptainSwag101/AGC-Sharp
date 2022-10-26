using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal class Block1Computer : Hardware.Computer
    {
        public Block1Computer()
        {
            Names = new[] { "Block1", "BlockI", "AGC4" };
            Memory = new Memory();
        }
    }
}
