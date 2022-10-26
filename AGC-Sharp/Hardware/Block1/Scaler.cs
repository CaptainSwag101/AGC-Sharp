using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal class Scaler
    {
        private ulong currentState;
        private ulong prevState;

        public void Tick()
        {
            // We increment currentState and compare its bits to prevState to detect
            // state changes and serve as binary divisions of the rate at which the
            // Tick() function is called. For example, the first bit of the current/prev
            // pair will change state every tick, while the second bit will only change every
            // two ticks, the third bit every four ticks. For any bit N of the current/prev
            // pair, that bit's state will change every 2^N ticks.
            prevState = currentState;
            ++currentState;


        }
    }
}
