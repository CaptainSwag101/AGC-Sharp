using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.Helpers;

namespace AGC_Sharp
{
    public delegate void ControlPulseFunc(Cpu cpu);

    public class Cpu
    {
        #region Registers
        /// <summary>
        /// Stage of the current instruction.
        /// </summary>
        public byte RegisterST { get; set; }
        /// <summary>
        /// Stage of the upcoming instruction.
        /// </summary>
        public byte RegisterST_Next { get; set; }
        /// <summary>
        /// Bit 1 of the branch register.
        /// </summary>
        public bool RegisterBR1 { get; set; }
        /// <summary>
        /// Bit 2 of the branch register.
        /// </summary>
        public bool RegisterBR2 { get; set; }
        /// <summary>
        /// Memory address being accessed by the current instruction.
        /// </summary>
        public ushort RegisterS { get; set; }
        /// <summary>
        /// Flip-flop for temporary storage of S when performing erasable memory writeback.
        /// </summary>
        public ushort RegisterS_Temp { get; set; }
        /// <summary>
        /// Accumulator register, stores the results of many common operations.
        /// </summary>
        public ushort RegisterA { get; set; }
        /// <summary>
        /// Buffer register, used as temporary storage and to hold the next instruction to be executed.
        /// </summary>
        public ushort RegisterB { get; set; }
        /// <summary>
        /// Memory buffer register, used to store values being read from or written to memory.
        /// </summary>
        public ushort RegisterG { get; set; }
        /// <summary>
        /// Low-order accumulator, used to store the lower half of double-precision calculations.
        /// </summary>
        public ushort RegisterL { get; set; }
        /// <summary>
        /// Return address for TC-related instructions or interrupts.
        /// </summary>
        public ushort RegisterQ { get; set; }
        /// <summary>
        /// Sequence register, holds the current instruction opcode.
        /// </summary>
        public byte RegisterSQ { get; set; }
        /// <summary>
        /// Program Counter, holds the address of the next instruction to be executed.
        /// </summary>
        public ushort RegisterZ { get; set; }
        /// <summary>
        /// Erasable memory bank selection register.
        /// </summary>
        public ushort RegisterEB { get; set; }
        /// <summary>
        /// Fixed memory bank selection register.
        /// </summary>
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
        private ushort _registerFB;
        /// <summary>
        /// Both memory bank selection register.
        /// </summary>
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
        /// <summary>
        /// Dictionary of currently-connected I/O channels. An absolute maximum of 512 channels are supported.
        /// </summary>
        public Dictionary<ushort, ushort> IOChannels { get; set; }
        #endregion

        #region Internal Data
        /// <summary>
        /// Adder component X.
        /// </summary>
        public ushort AdderX { get; set; }
        /// <summary>
        /// Adder component Y.
        /// </summary>
        public ushort AdderY { get; set; }
        /// <summary>
        /// Adder explicit carry bit.
        /// </summary>
        public bool AdderCarry { get; set; }
        /// <summary>
        /// Adder output.
        /// </summary>
        public ushort AdderOutput {
            get
            {
                // Basic addition step
                uint temp = (uint)AdderX + (uint)AdderY;

                // Handle carries
                uint carry = (uint)(AdderCarry ? 1 : 0);  // Explicit carry
                if (!NoEAC)
                    carry |= ((temp >> 16) & 1);  // End-around carry if not inhibited
                temp += carry;

                return (ushort)temp;
            }
        }
        /// <summary>
        /// Indicates to the CPU that the next instruction should be
        /// fetched from memory at the end of the current instruction.
        /// Set by NISQ control pulse, cleared upon next instruction fetch.
        /// </summary>
        public bool NextInstruction { get; set; }
        /// <summary>
        /// Indicates to the CPU that interrupts are temporarily disabled.
        /// Set by INHINT instruction, cleared by RELINT instruction.
        /// </summary>
        public bool InhibitInterrupts { get; set; }
        /// <summary>
        /// Indicates to the CPU that end-around carry should be inhibited in the adder.
        /// Set by NEACON control pulse, cleared by NEACOF control pulse.
        /// </summary>
        public bool NoEAC { get; set; }
        /// <summary>
        /// Special memory bit used during multiplication.
        /// </summary>
        public bool MCRO { get; set; }
        /// <summary>
        /// Indicates to the CPU that we are currently in a division sequence.
        /// Reset upon next instruction fetch, reinstated by each subsequent DV stage.
        /// </summary>
        public bool DVSequence { get; set; }
        /// <summary>
        /// Status byte containing an integer byte by which the DV grey-code
        /// is shifted in, in place of the ST register when fetching the next instruction
        /// if DVSequence is true. Reset by RSTSTG control pulse.
        /// </summary>
        public byte DVStage { get; set; }
        /// <summary>
        /// Indicates to the CPU that we are performing a SHINC operation.
        /// </summary>
        public bool ShincSequence { get; set; }
        /// <summary>
        /// Special status indicator, not yet implemented.
        /// </summary>
        public bool PIFL { get; set; }
        /// <summary>
        /// Indicates to the CPU that the current instruction is an extra-code instruction.
        /// Set to <see cref="Extend_Next"/> at the next instruction fetch.
        /// </summary>
        public bool Extend { get; set; }
        /// <summary>
        /// Indicates to the CPU that the next instruction is an extra-code instruction.
        /// Cleared at the next instruction fetch.
        /// </summary>
        public bool Extend_Next { get; set; }
        /// <summary>
        /// Internal bus used to store intra-CPU data transfer.
        /// </summary>
        public ushort WriteBus { get; set; }
        /// <summary>
        /// Special counter used to keep track of CPU activity to check for software hang-ups.
        /// </summary>
        public ulong NightWatchman { get; set; }
        #endregion

        #region Control Pulse Data
        /// <summary>
        /// The current buffer queue for potential control pulses called for
        /// by the current subinstruction. If there are multiple control pulse sequences available
        /// for the current time pulse, the branch register is used to select which one is chosen.
        /// </summary>
        public Queue<(byte PulseNum, List<ControlPulseFunc> PulseList)> ControlPulseQueue { get; set; }  // Control pulse activation number, function
        /// <summary>
        /// The current timing pulse count, a value between 1 and 12.
        /// Resets back to 1 after finishing pulse 12.
        /// </summary>
        private byte controlPulseNum { get; set; }    // Reset upon every new subinstruction
        #endregion

        /// <summary>
        /// Initializes the CPU and its control pulse queue.
        /// </summary>
        public Cpu()
        {
            ControlPulseQueue = new();
            controlPulseNum = 1;
            NightWatchman = 0;

            // Init the first two I/O channels so we can pass self-tests
            IOChannels = new()
            {
                { 9, 0 },
                { 10, 0 }
            };
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
            if (controlPulseNum == 1)
            {
                // TODO: Add INKL handling once we implement I/O!
                if (RegisterST != 2)
                {
                    NextInstruction = false;
                    Extend_Next = false;
                }
            }

            // On T2, T5, T8, or T11, reset PIFL
            if (controlPulseNum == 2 || controlPulseNum == 5 || controlPulseNum == 8 || controlPulseNum == 11)
            {
                PIFL = false;
            }

            // If there are control pulses left to be performed, perform them if it's the proper pulse number
            if (ControlPulseQueue.Count > 0)
            {
                // If we have multiple possible lists of control pulses to perform at the current time pulse,
                // copy them into a list and choose the correct one afterward.
                List<List<ControlPulseFunc>> branchPulseList = new();
                foreach (var ctrlPulseEntry in ControlPulseQueue)
                {
                    if (ctrlPulseEntry.PulseNum == controlPulseNum)
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
                        throw new InvalidDataException($"The current subinstruction at timepulse {controlPulseNum} has {branchPulseList.Count} possible branches, which is invalid.");

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
            if (controlPulseNum == 4)
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
            }

            // Only perform writeback if we performed an erasable read earlier
            if (controlPulseNum == 9 && RegisterS_Temp > 0)
            {
                // TODO: Theoretically we should be re-computing the parity bit because it is discarded when loading into G
                memory.WriteErasableWord(RegisterS_Temp, RegisterG, this);  // This is the only place we ever write to erasable memory!
                RegisterS_Temp = 0;
            }

#if DEBUG
            // Print state debug info at the end of each timepulse's execution,
            // before we clear the write bus or queue the next subinstruction.
            PrintCpuStateDebugInfo();
#endif

            // After executing pulse 12, reset the control pulse count.
            // Then, perform bookkeeping tasks to prepare to load the next instruction.
            if (controlPulseNum == 12)
            {
                controlPulseNum = 0;  // This will be incremented to 1 shortly hereafter
                RegisterST = RegisterST_Next;
                RegisterST_Next = 0;

                if (NextInstruction)
                {
                    RegisterSQ = (byte)CopyWordBits(RegisterB, 0, 10..14, 1..5, BitCopyMode.ClearNone); // Copy bits 14-10
                    RegisterSQ = (byte)CopyWordBits(RegisterB, RegisterSQ, 16..16, 6..6, BitCopyMode.ClearNone);    // Copy bit 16
                    Extend = Extend_Next;
                }

                // Get the new subinstruction to be executed now that we've updated the CPU
                // state accordingly, and queue up its control pulses.
                (string _, ISA.SubinstructionFunc subinstructionFunc) = GetCurrentSubinstruction();
                subinstructionFunc(this);

                // Reset DVSequence, it may be reinstated by the next DV subinstruction.
                // This is largely redundant as we need to reset DVSequence early to break
                // out of DV properly.
                DVSequence = false;
            }

            // Because writes to the write bus use binary OR, we need to zero it out after every time pulse.
            WriteBus = 0;

            // Increment the control pulse number
            ++controlPulseNum;
        }

        /// <summary>
        /// Determines the next subinstruction to be executed based on <see cref="RegisterSQ"/>
        /// and prepares the control pulses that are called for by that subinstruction.
        /// </summary>
        public (string Name, ISA.SubinstructionFunc Func) GetCurrentSubinstruction()
        {
            // Use either the ST register or the DVStage flip-flops to get the subinstruction,
            // depending on the current context.
            byte stage;
            if (DVSequence)
            {
                stage = (byte)(((0b111 << DVStage) >> 3) & 0b111);
            }
            else
            {
                stage = RegisterST;
            }

            return ISA.SubinstructionHelper.SubinstructionDictionary[(stage, Extend, RegisterSQ)];
        }

        /// <summary>
        /// Print debug information about the current CPU state.
        /// </summary>
        public void PrintCpuStateDebugInfo()
        {
            (string subinstructionName, ISA.SubinstructionFunc _) = GetCurrentSubinstruction();

            // Print subinstruction debug info
            Console.WriteLine();
            Console.WriteLine($"{subinstructionName} (T{controlPulseNum})");
            Console.WriteLine($"Z = {Convert.ToString(RegisterZ, 8)}, A = {Convert.ToString(RegisterA, 8)}, L = {Convert.ToString(RegisterL, 8)}, B = {Convert.ToString(RegisterB, 8)}, EXTEND = {Extend}, INHINT = {InhibitInterrupts}");
            Console.WriteLine($"S = {Convert.ToString(RegisterS, 8)}, G = {Convert.ToString(RegisterG, 8)}, Q = {Convert.ToString(RegisterQ, 8)}, SQ = {Convert.ToString(RegisterSQ, 8)}, WL = {Convert.ToString(WriteBus, 8)}, X = {Convert.ToString(AdderX, 8)}, Y = {Convert.ToString(AdderY, 8)}");
            Console.WriteLine($"EB = {Convert.ToString(RegisterEB >> 8, 8)}, FB = {Convert.ToString(RegisterFB >> 10, 8)}, BB = {Convert.ToString(RegisterBB, 8)}, ST = {Convert.ToString(RegisterST, 8)}, DVSequence = {DVSequence}, DVStage = {DVStage}, BR1 = {RegisterBR1}, BR2 = {RegisterBR2}");
        }
    }
}
