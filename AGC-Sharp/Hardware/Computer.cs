using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware
{
    internal abstract class Computer
    {
        public string[] Names { get; protected set; }
        protected IMemory Memory { get; set; }
        protected IScaler Scaler { get; set; }
        protected ITimer Timer { get; set; }
    }
}
