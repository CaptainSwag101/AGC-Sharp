using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal class Timer
    {
        private AGC agcReference;
        private readonly TimeSpan EXECUTION_BATCH_INTERVAL = new(0, 0, 0, 1, 0);
        private const long EXECUTION_BATCH_TICKS_PER_INTERVAL = 1024000;
        private Stopwatch executionTimer = new();
        private ulong totalTicks;
        private bool stop = false;

        public Timer(AGC agc)
        {
            agcReference = agc;
        }

        public void Start()
        {
            while (!stop)
            {
                executionTimer.Start();
                for (long i = 0; i < EXECUTION_BATCH_TICKS_PER_INTERVAL; ++i)
                {
                    ++totalTicks;

                    agcReference.Cpu.Tick();


                    // TODO: Update scaler's interrupt state since that happens more often than
                    // the scaler actually ticks, and is needed for RUPT LOCK alarm functionality.


                    // TODO: If the CPU is executing a TC instruction at any time, tell the scaler.


                    // Tick the scaler every 102.4 KHz (1/10 division of master clock frequency)
                    if ((totalTicks % 10) == 0)
                    {
                        agcReference.Scaler.Tick();
                    }
                }
                executionTimer.Stop();


                // Sleep for remaining time of interval.
                TimeSpan remainingTime = EXECUTION_BATCH_INTERVAL - executionTimer.Elapsed;
                double ratioToRealtime = EXECUTION_BATCH_INTERVAL.TotalMilliseconds / executionTimer.Elapsed.TotalMilliseconds;
                LogLine($"Finished {EXECUTION_BATCH_TICKS_PER_INTERVAL} ticks in {executionTimer.Elapsed.TotalMilliseconds}ms, {ratioToRealtime}x real-time speed.");

                if (remainingTime.TotalMilliseconds > 0)
                {
                    Thread.Sleep(remainingTime);
                }

                executionTimer.Reset();
            }
        }

        public void Stop()
        {
            stop = true;
        }
    }
}
