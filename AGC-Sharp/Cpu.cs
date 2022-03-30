using System;
using System.Collections;
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
        public byte RegisterST_Next { get; set; } // Next instruction state
        public bool RegisterBR1 { get; set; }   // Branch register bit 1
        public bool RegisterBR2 { get; set; }   // Branch register bit 2
        public ushort RegisterS { get; set; }   // Memory address the current instruction is accessing
        public ushort RegisterS_Temp { get; set; } // Flip-flop for temporary storage of S for erasable memory writeback
        public ushort RegisterA { get; set; }   // Accumulator
        public ushort RegisterB { get; set; }   // Value of the next instruction
        public ushort RegisterG { get; set; }   // Memory word temporary storage
        public ushort RegisterL { get; set; }   // Low-order accumulator
        public ushort RegisterQ { get; set; }   // Current instruction word
        public ushort RegisterSQ { get; set; }
        public ushort RegisterZ { get; set; }   // Program Counter
        public ushort RegisterEB { get; set; }  // Erasable bank
        private ushort _registerFB;
        public ushort RegisterFB                // Fixed bank
        {
            get
            {
                return _registerFB;
            }
            set
            {
                _registerFB = (ushort)(Helpers.Bit16To15(value, true) & (0x1F << 10));
            }
        }
        public ushort RegisterBB
        {
            get
            {
                return (ushort)(RegisterFB | (RegisterEB >> 8));
            }
            set
            {
                RegisterEB = (ushort)((value & 7) << 8);
                RegisterFB = value;
                _registerFB &= 0x7C00;
            }
        }
        #endregion

        #region I/O
        public Dictionary<byte, ushort> IOChannels { get; set; }
        #endregion

        #region Internal Data
        public ushort AdderX { get; set; }      // Adder component X
        public ushort AdderY { get; set; }      // Adder component Y
        public bool AdderCarry { get; set; }    // Adder carry bit
        public bool NextInstruction { get; set; }   // Flagged by NISQ control pulse
        public bool InhibitInterrupts { get; set; }
        public bool NoEAC { get; set; } // Whether end-around carry should be inhibited
        public bool MCRO { get; set; }  // Special bit value for multiplication
        public bool ShincSequence { get; set; } // True when in the middle of a SHINC subinstruction
        public bool PIFL { get; set; }
        public bool Extend { get; set; }
        public bool Extend_Next { get; set; }
        public ushort WriteBus { get; set; }
        public ulong NightWatchman { get; set; }
        #endregion

        #region Control Pulse Data
        public Queue<(byte PulseNum, List<ControlPulseFunc> PulseList)> ControlPulseQueue { get; set; }  // Control pulse activation number, function
        private byte controlPulseCount { get; set; }    // Reset upon every new subinstruction
        #endregion

        /// <summary>
        /// Initializes the CPU and its control pulse queue.
        /// </summary>
        public Cpu()
        {
            ControlPulseQueue = new();
            controlPulseCount = 1;
            NightWatchman = 0;

            // Init the first two I/O channels so we can pass self-tests
            IOChannels = new();
            IOChannels.Add(9, 0);
        }

        /// <summary>
        /// Performs one control pulse tick on the CPU.
        /// If there are remaining control pulses to be performed in the queue, perform them.
        /// Otherwise, increment the program counter and execute the next instruction.
        /// </summary>
        public void Tick(Memory memory)
        {
            // Every timepulse, reset the MCRO flag for multiplication
            MCRO = false;

            // Before pulse 1, do INKBT1
            if (controlPulseCount == 1)
            {
                // TODO: Add INKL handling once we implement I/O!
                if (RegisterST != 2)
                {
                    NextInstruction = false;
                    Extend_Next = false;
                }
            }

            // If there are control pulses left to be performed, perform them if it's the proper pulse number
            if (ControlPulseQueue.Count > 0)
            {
                // If we have multiple possible lists of control pulses to perform at the current time pulse,
                // copy them into a list and choose the correct one afterward.
                List<List<ControlPulseFunc>> branchPulseList = new();
                foreach (var ctrlPulseEntry in ControlPulseQueue)
                {
                    if (ctrlPulseEntry.PulseNum == controlPulseCount)
                    {
                        branchPulseList.Add(ctrlPulseEntry.PulseList);
                    }
                }

                // Dequeue and discard however many pulse lists we copied in the previous step
                for (int i = 0; i < branchPulseList.Count; ++i)
                {
                    _ = ControlPulseQueue.Dequeue();
                }

                // If there is more than one pulse list, execute the list index indicated
                // by the BR register (between 0 and 3).
                if (branchPulseList.Count > 1)
                {
                    // Sanity check: Throw if we have more than 4 possible branches,
                    // it means we screwed up our subinstruction.
                    if (branchPulseList.Count > 4)
                        throw new InvalidDataException($"The current subinstruction at timepulse {controlPulseCount} has {branchPulseList.Count} possible branches, which is invalid.");

                    List<ControlPulseFunc>? selectedPulseList = null;
                    if (RegisterBR1 == false && RegisterBR2 == false)
                        selectedPulseList = branchPulseList[0];
                    else if (RegisterBR1 == true && RegisterBR2 == false)
                        selectedPulseList = branchPulseList[2];
                    else if (RegisterBR1 == false && RegisterBR2 == true)
                        selectedPulseList = branchPulseList[1];
                    else if (RegisterBR1 == true && RegisterBR2 == true)
                        selectedPulseList = branchPulseList[3];

                    // Execute the selected pulse list.
                    // We have evaluated all possible binary permutations of the BR register,
                    // So it cannot be null here.
                    foreach (var pulse in selectedPulseList!)
                    {
                        pulse(this);
                    }
                }
                // Otherwise just execute the first and only entry.
                else if (branchPulseList.Count > 0)
                {
                    foreach (var pulse in branchPulseList[0])
                    {
                        pulse(this);
                    }
                }
            }

            // Memory reads are done after pulse 4
            if (controlPulseCount == 4)
            {
                // Check if the read is for erasable or fixed memory
                if (RegisterS < 0x400)  // Erasable memory
                {
                    if (RegisterS >= 8)  // Anything less than octal 10 is a register, only read actual memory
                    {
                        RegisterS_Temp = RegisterS; // Preserve the state of S in case it's changed before our writeback
                        RegisterG = memory.ReadWord(RegisterS, this);
                        RegisterG = Helpers.Bit16To15(RegisterG, false);
                    }
                }
                else                    // Fixed memory
                {
                    RegisterG = memory.ReadWord(RegisterS, this);
                    RegisterG = Helpers.Bit16To15(RegisterG, false);
                }

                // Print mid-instruction debug info
                Console.WriteLine($"G updated to {Convert.ToString(RegisterG, 8)}");
            }

            // Only perform writeback if we performed an erasable read earlier
            if (controlPulseCount == 9 && RegisterS_Temp > 0)
            {
                // TODO: Theoretically we should be re-computing the parity bit because it is discarded when loading into G
                memory.WriteErasableWord(RegisterS_Temp, RegisterG, this);  // This is the only place we ever write to erasable memory!
                RegisterS_Temp = 0;
            }

            // After executing pulse 12, reset the control pulse count.
            // Then, perform bookkeeping tasks to prepare to load the next instruction.
            if (controlPulseCount == 12)
            {
                controlPulseCount = 0;  // This will be incremented to 1 shortly hereafter
                RegisterST = RegisterST_Next;
                RegisterST_Next = 0;

                if (NextInstruction)
                {
                    RegisterSQ = (ushort)(RegisterB & 0xBE00);  // Copy bits 16,14-10
                    Extend = Extend_Next;
                }

                PrepNextSubinstruction();
            }

            // Because writes to the write bus use binary OR,
            // we need to zero it out after every time pulse.
            WriteBus = 0;
            ++controlPulseCount;
        }

        private void PrepNextSubinstruction()
        {
            byte regSQ16_10_Spliced = (byte)(Helpers.Bit16To15(RegisterSQ, true) >> 9); // Use only bits 16,14-10

            // Pick proper subinstruction
            (string subinstructionName, ISA.SubinstructionFunc subinstructionFunc) = ISA.SubinstructionHelper.SubinstructionDictionary[(RegisterST, Extend, regSQ16_10_Spliced)];

            // Print subinstruction debug info
            Console.WriteLine();
            Console.WriteLine($"{subinstructionName}");
            Console.WriteLine($"Z = {Convert.ToString(RegisterZ, 8)}, A = {Convert.ToString(RegisterA, 8)}, L = {Convert.ToString(RegisterL, 8)}, B = {Convert.ToString(RegisterB, 8)},");
            Console.WriteLine($"S = {Convert.ToString(RegisterS, 8)}, G = {Convert.ToString(RegisterG, 8)}, Q = {Convert.ToString(RegisterQ, 8)}, SQ = {Convert.ToString(RegisterSQ, 8)},");
            Console.WriteLine($"EB = {Convert.ToString(RegisterEB, 8)}, FB = {Convert.ToString(RegisterFB, 8)}, BB = {Convert.ToString(RegisterBB, 8)}, ST = {Convert.ToString(RegisterST, 8)}, BR1 = {RegisterBR1}, BR2 = {RegisterBR2}");

            // Execute subinstruction (queue its control pulses)
            subinstructionFunc(this);
        }
    }
}
