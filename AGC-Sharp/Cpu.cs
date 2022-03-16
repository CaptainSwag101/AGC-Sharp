using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ushort RegisterS_Temp { get; set; } // Flip-flop for temporary storage of S for erasable memory writeback
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
        public ulong NightWatchman { get; set; }
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
            NightWatchman = 0;
        }

        /// <summary>
        /// Performs one control pulse tick on the CPU.
        /// If there are remaining control pulses to be performed in the queue, perform them.
        /// Otherwise, increment the program counter and execute the next instruction.
        /// </summary>
        public void Tick(Memory memory)
        {
            // If there are control pulses left to be performed, perform them if it's the proper pulse number
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

            // Memory reads are done after pulse 4
            if (ControlPulseCount == 4)
            {
                // Check if the read is for erasable or fixed memory
                if (RegisterS < 0x400)  // Erasable memory
                {
                    if (RegisterS >= 8)  // Anything less than octal 10 is a register, only read actual memory
                    {
                        RegisterS_Temp = GetBankedErasableAddress(); // Preserve the state of S in case it's changed before our writeback
                        RegisterG = memory.ReadWord(RegisterS, this);
                    }
                }
                else                    // Fixed memory
                {
                    RegisterG = memory.ReadWord(RegisterS, this);
                }
            }

            // Only perform writeback if we performed an erasable read earlier
            if (ControlPulseCount == 9 && RegisterS_Temp > 0)
            {
                memory.WriteErasableWord(RegisterS_Temp, RegisterG);    // This is the only place we ever write to erasable memory!
                RegisterS_Temp = 0;
            }

            // Because writes to the write bus use binary OR,
            // we need to zero it out after every time pulse.
            WriteBus = 0;
            ++ControlPulseCount;
        }

        private ushort GetBankedErasableAddress()
        {
            return (ushort)((RegisterS & 0xFF) | (RegisterEB & 0x700));
        }
    }
}
