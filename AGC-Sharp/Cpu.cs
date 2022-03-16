using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp
{
    internal delegate void ControlPulseFunc(Cpu cpu);

    internal class Cpu
    {
        #region Registers
        public byte RegisterST { get; set; }    // Instruction state
        public byte RegisterBR { get; set; }    // Branch register
        public ushort RegisterS { get; set; }   // Memory address the current instruction is accessing
        public ushort RegisterA { get; set; }   // Accumulator
        public ushort RegisterB { get; set; }
        public ushort RegisterG { get; set; }   // Memory word temporary storage
        public ushort RegisterL { get; set; }   // Low-order accumulator
        public ushort RegisterQ { get; set; }   // Current instruction word
        public ushort RegisterSQ { get; set; }
        public ushort RegisterZ { get; set; }   // Program Counter
        public ushort RegisterEB { get; set; }  // Erasable bank
        public ushort RegisterFB { get; set; }  // Fixed bank
        #endregion

        #region Internal Data
        public bool InhibitInterrupts { get; set; }
        public bool Extend { get; set; }
        public ushort WriteBus { get; set; }

        #endregion

        #region Control Pulse Data
        public Queue<(byte PulseNum, List<ControlPulseFunc> PulseList)> ControlPulseQueue { get; set; }  // Control pulse activation number, function
        public byte ControlPulseCount { get; private set; }   // Reset upon every new instruction
        #endregion

        /// <summary>
        /// Initializes the CPU and its control pulse queue.
        /// </summary>
        public Cpu()
        {
            ControlPulseQueue = new();
            ControlPulseCount = 1;
        }

        /// <summary>
        /// Performs one control pulse tick on the CPU.
        /// If there are remaining control pulses to be performed in the queue, perform them.
        /// Otherwise, increment the program counter and execute the next instruction.
        /// </summary>
        public void Tick()
        {
            if (ControlPulseQueue.Count > 0)
            {
                if (ControlPulseQueue.Peek().PulseNum == ControlPulseCount)
                {
                    List<ControlPulseFunc> pulses = ControlPulseQueue.Dequeue().PulseList;
                    foreach (var pulse in pulses)
                    {
                        pulse(this);
                    }
                }
            }
            else if (ControlPulseCount % 12 == 1)
            {
                // Reset the control pulse count and load the next instruction
                ControlPulseCount = 1;
            }

            ++ControlPulseCount;
        }
    }
}
