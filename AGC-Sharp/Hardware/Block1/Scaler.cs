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
        private AGC agcReference;

        public Scaler(AGC agc)
        {
            agcReference = agc;
        }

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

            // TODO: Perform necessary tasks for any of the 18 ring counter stages.
            if (ScalerRingState(9) && ScalerRingChanged(9)) // F09B
            {
                // If not FS10
                if (!ScalerRingState(10))
                {
                    agcReference.Cpu.Counters[(int)CounterSlot.TIME4] = CounterAction.Up;
                }

                // TODO: Generate KEYRUPT1, KEYRUPT2, or MARKRUPT if keys are pending.
                // TODO: Read the DSKY queue.
            }
        }

        private bool ScalerRingState(byte number)
        {
            ulong bitSelect = (ulong)Math.Pow(2, number);
            return ((currentState & bitSelect) > 0);
        }

        private bool ScalerRingChanged(byte number)
        {
            ulong bitSelect = (ulong)Math.Pow(2, number);
            return ((currentState & bitSelect) != (prevState & bitSelect));
        }
    }
}
