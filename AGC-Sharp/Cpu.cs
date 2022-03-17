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
        public byte RegisterBR { get; set; }    // Branch register
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
                return DoubleSignBitCreate(_registerFB);
            }
            set
            {
                _registerFB = DoubleSignBitDelete(value, true);
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

        #region Internal Data
        public ushort AdderX { get; set; }      // Adder component X
        public ushort AdderY { get; set; }      // Adder component Y
        public bool AdderCarry { get; set; }    // Adder carry bit
        public bool NextInstruction { get; set; }   // Flagged by NISQ control pulse
        public bool InhibitInterrupts { get; set; }
        public bool Extend { get; set; }
        public ushort WriteBus { get; set; }
        public ulong NightWatchman { get; set; }
        #endregion

        #region Control Pulse Data
        public Queue<(byte PulseNum, List<ControlPulseFunc> PulseList)> ControlPulseQueue { get; set; }  // Control pulse activation number, function
        public byte ControlPulseCount { get; private set; }   // Reset upon every new subinstruction
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
                // If we have multiple possible lists of control pulses to perform at the current time pulse,
                // copy them into a list and choose the correct one afterward.
                List<List<ControlPulseFunc>> branchPulseList = new();
                foreach (var ctrlPulseEntry in ControlPulseQueue)
                {
                    if (ctrlPulseEntry.PulseNum == ControlPulseCount)
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
                    foreach (var pulse in branchPulseList[RegisterBR])
                    {
                        pulse(this);
                    }
                }
                else if (branchPulseList.Count > 0)     // Otherwise just execute the first and only entry.
                {
                    foreach (var pulse in branchPulseList[0])
                    {
                        pulse(this);
                    }
                }
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

                RegisterG = DoubleSignBitDelete(RegisterG, false);
            }

            // Only perform writeback if we performed an erasable read earlier
            if (ControlPulseCount == 9 && RegisterS_Temp > 0)
            {
                // TODO: Theoretically we should be re-computing the parity bit because it is discarded when loading into G
                memory.WriteErasableWord(RegisterS_Temp, RegisterG);    // This is the only place we ever write to erasable memory!
                RegisterS_Temp = 0;
            }

            // After executing pulse 12, reset the control pulse count.
            // Then, perform bookkeeping tasks to prepare to load the next instruction.
            if (ControlPulseCount == 12)
            {
                ControlPulseCount = 0;  // This will be incremented to 1 shortly hereafter
                RegisterST = RegisterST_Next;
                RegisterST_Next = 0;

                if (NextInstruction)
                    RegisterSQ = (ushort)(RegisterB & 0xBE00);  // Copy bits 16,14-10

                PrepNextSubinstruction();
                NextInstruction = false;
                Extend = false;
            }

            // Because writes to the write bus use binary OR,
            // we need to zero it out after every time pulse.
            WriteBus = 0;
            ++ControlPulseCount;
        }

        private void PrepNextSubinstruction()
        {
            // This function is going to be a mess. I could make a bit flag table but there would be so many redundant entries.
            byte regSQ16_10_Spliced = (byte)(DoubleSignBitDelete(RegisterSQ, true) >> 9); // Use only bits 16,14-10

            if (RegisterST == 2)
            {
                ISA.Subinstructions.STD2(this);
            }
            else if (Extend == false)
            {
                if (RegisterST == 0)
                {
                    switch (regSQ16_10_Spliced >> 3)
                    {
                        case 0:
                            ISA.Subinstructions.TC0(this);
                            break;
                        case 1:
                            switch ((regSQ16_10_Spliced >> 1) & 3)
                            {
                                case 0:
                                    ISA.Subinstructions.CCS0(this);
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    ISA.Subinstructions.TCF0(this);
                                    break;
                            }
                            break;
                        case 3:
                            ISA.Subinstructions.CA0(this);
                            break;
                        case 4:
                            ISA.Subinstructions.CS0(this);
                            break;
                        case 5:
                            switch ((regSQ16_10_Spliced >> 1) & 3)
                            {
                                case 0:
                                    //ISA.Subinstructions.NDX0(this);
                                    break;
                                case 1:
                                    //ISA.Subinstructions.DXCH0(this);
                                    break;
                                case 2:
                                    ISA.Subinstructions.TS0(this);
                                    break;
                                case 3:
                                    ISA.Subinstructions.XCH0(this);
                                    break;
                            }
                            break;
                        case 6:
                            ISA.Subinstructions.AD0(this);
                            break;
                    }
                }
                else if (RegisterST == 1)
                {
                    switch (regSQ16_10_Spliced >> 12)
                    {
                        case 0:
                            ISA.Subinstructions.GOJ1(this);
                            break;

                    }
                }
                else //if (RegisterST == 3)
                {

                }
            }
            else //if (Extend == true)
            {

            }
        }

        private ushort GetBankedErasableAddress()
        {
            return (ushort)((RegisterS & 0xFF) | (RegisterEB & 0x700));
        }

        private ushort DoubleSignBitCreate(ushort inVal)
        {
            inVal &= 0b0111111111111111;    // Mask out bit 16 just in case
            inVal |= (ushort)((inVal << 1) & 0b1000000000000000);   // Copy bit 15 into bit 16
            return inVal;
        }

        private ushort DoubleSignBitDelete(ushort inVal, bool maskBit16)
        {
            inVal &= 0b1011111111111111;    // Mask out the parity bit
            inVal |= (ushort)((inVal >> 1) & 0b0100000000000000);   // Copy the sign bit into bit 15
            if (maskBit16) inVal &= 0b0111111111111111;    // Mask out the old bit 16
            return inVal;
        }
    }
}
