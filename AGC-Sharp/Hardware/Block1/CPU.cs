using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AGC_Sharp.Bitmasks;

namespace AGC_Sharp.Hardware.Block1
{
    [Flags]
    internal enum CounterAction
    {
        None,
        Up,
        Down
    }

    internal enum CounterSlot
    {
        TIME2, TIME1, TIME3, TIME4, TIME5, TIME6,
        CDUX, CDUY, CDUZ,
        TRN, SHFT,
        PIPAX, PIPAY, PIPAZ,
        BMAGX, BMAGY, BMAGZ,
        INLINK,
        RNRAD,
        GYROD,
        CDUXD, CDUYD, CDUZD,
        TRUND, SHAFTD,
        THRSTD,
        EMSD,
        OTLNK,
        ALT
    }

    internal enum RuptSlot
    {
        GO,
        T6RUPT, T5RUPT, T3RUPT, T4RUPT,
        KEYRUPT1, KEYRUPT2,
        MARKRUPT,
        UPRUPT, DOWNRUPT,
        RADARRUPT,
        RUPT10L
    }

    internal partial class CPU
    {
        #region Central Registers
        private word A, LP, Q, Z, G, B, BNK, U, X, Y, IN0, IN1, IN2, IN3, OUT0, OUT1, OUT2, OUT3, OUT4;
        #endregion

        #region State Information
        private word WriteBus;
        private byte Timepulse;
        private bool ExplicitCarry;
        private byte ST, STNext;
        private byte BR;
        private word SQ;
        private word S, SBuffer;
        private bool NOEAC, PSEUDO ,MCRO, INKL, IIP, INHINT, Restart, PIFL;
        private bool DV;
        private byte DVStage;
        private bool FetchNextInstruction;
        public bool[] Interrupts;
        public CounterAction[] Counters;
        private bool NightWatchman;
        #endregion

        #region Emulator-related Data
        private AGC agcReference;
        private Subinstruction? currentSubinstruction = null;
        private Subinstruction? pendingSubinstruction = null;
        #endregion

        #region Functions
        public CPU(AGC agc)
        {
            agcReference = agc;

            FetchNextInstruction = true;
            Timepulse = 12;
            Interrupts = new bool[11];
            Counters = new CounterAction[22];

            // Hacks to test code execution
            Z = Octal(4000);
        }

        public word GetErasableAddress()
        {
            if (S == Octal(67)) NightWatchman = true;
            return S;
        }

        public word GetFixedAddress()
        {
            word absoluteAddress = S;

            // TODO: This code was adapted from Block II memory addressing, verify that's how Block I really does it.
            if (S >= Memory.MEM_FIXED_BANKED_START && S <= Memory.MEM_FIXED_BANKED_END)
            {
                absoluteAddress &= Octal(1777);
                absoluteAddress |= BNK; // Prepend the bank selection bits to address it
            }

            return absoluteAddress;
        }

        public void UpdateAdder()
        {
            int temp = X + Y;

            int carry = ExplicitCarry ? 1 : 0;  // Explicit carry
            if (!NOEAC)
            {
                carry |= ((temp >> 16) & 1);    // End-around carry if not inhibited
            }
            temp += carry;

            U = (word)temp;
        }

        public void PrintStateInfo()
        {
            StringBuilder stateInfo = new();
            stateInfo.AppendLine($"{currentSubinstruction?.Name} (T{Timepulse.ToString().PadLeft(2, '0')})");
            string[] line1 =
            {
                $"A: {ToOctal(A, 6)}",
                $"Z: {ToOctal(Z, 6)}",
                $"Q: {ToOctal(Q, 6)}",
                $"LP: {ToOctal(LP, 6)}",
                "\n"
            };
            string[] line2 =
            {
                $"B: {ToOctal(B, 6)}",
                $"SQ: {ToOctal(SQ, 1)}\t",
                $"ST: {ToOctal(ST, 1)}\t",
                $"BNK: {ToOctal(BNK, 6)}",
            };
            stateInfo.AppendJoin('\t', line1);
            stateInfo.AppendJoin('\t', line2);
            stateInfo.AppendLine();
            LogLine(stateInfo.ToString());
        }

        public void Tick()
        {
            PreTimepulseActions();

            // Actually execute the portion of the current subinstruction for the current timepulse.
            if (currentSubinstruction is not null)
                currentSubinstruction.Function(this);

            PostTimepulseActions();
        }

        private void PreTimepulseActions()
        {
            // At the start of every timepulse, clear MCRO.
            MCRO = false;


            // TODO: Before timepulse 1, do INKBT1
            if (Timepulse == 1)
            {
                if (INKL)
                {
                    // Remember what we wanted to do, so we can come back to it later.
                    pendingSubinstruction = currentSubinstruction;

                    foreach (var c in Counters)
                    {
                        if (c == CounterAction.Up)
                        {
                            // TODO: PINC subinstruction
                            break;
                        }
                        else if (c == CounterAction.Down)
                        {
                            // TODO: DINC subinstruction
                            break;
                        }
                    }
                }

                // Reset next-instruction flag(s)
                if (ST != 2)
                    FetchNextInstruction = false;
            }


            // Before T2, T5, T8, and T11, reset PIFL
            if (Timepulse == 2 || Timepulse == 5 || Timepulse == 8 || Timepulse == 11)
                PIFL = false;
        }

        private void PostTimepulseActions()
        {
            // Memory reads are done after T4
            if (Timepulse == 4)
            {
                // Determine whether we are targeting fixed or erasable memory.
            }

            if (Timepulse == 1)
            {
                PrintStateInfo();   // Print logging info for the timepulse or MCT.
            }


            WriteBus = 0;   // Clear the write lines


            // Perform cleanup and fetch the next subinstruction if needed after T12
            if (!DV && Timepulse == 12)
            {
                PrepForNextSubinstruction();
                ComputeNextSubinstruction();
                FinalCleanup();
            }

            if (Timepulse == 12)
                Timepulse = 1;
            else
                ++Timepulse;
        }

        private void PrepForNextSubinstruction()
        {
            // Push next stage to its pending value
            ST = STNext;
            STNext = 0;


            if (FetchNextInstruction)
            {
                // Check for pending counter requests
                bool prevINKL = INKL;
                INKL = false;
                foreach (var c in Counters)
                {
                    if (c != CounterAction.None && !PSEUDO)
                    {
                        INKL = true;
                        break;
                    }
                }


                // TODO: If no counters need servicing and we have a pending subinstruction, get back to it
                if (!INKL && prevINKL)
                {
                    // Do stuff here
                }


                // Check for pending interrupts
                bool ruptPending = false;
                foreach (var r in Interrupts)
                {
                    if (r)
                    {
                        ruptPending = true;
                        break;
                    }
                }


                // Before executing an interrupt, ensure that there is no overflow in A,
                // which would be lost if we rupt now.
                byte signBits = (byte)((A & BITMASK_15_16) >> 14);
                bool overflow = (signBits == 1 || signBits == 2);   // No overflow would be 0 or 3
                if (ruptPending && !INHINT && !IIP && !PSEUDO && !overflow)
                {
                    // TODO: Set next subinstruction to rupt instruction
                }
                else // Otherwise fetch the next instruction data and prep the CPU state
                {
                    SQ = (word)((B & BITMASK_10_14) >> 9);
                    SQ |= (word)((B & BITMASK_16) >> 10);
                }
            }
        }

        private void ComputeNextSubinstruction()
        {
            // Populate currentSubinstruction based on the contents of SQ and ST
            bool foundImplementation = false;
            foreach (var sub in ImplementedSubinstructions)
            {
                if (ST == sub.ST && (SQ & sub.SQMask) == sub.SQ)
                {
                    currentSubinstruction = sub;
                    foundImplementation = true;
                    break;
                }
            }

            // If no valid subinstruction was found, use STD2 to advance the program counter
            // and try again at the next memory location. This generally indicates a problem.
            if (!foundImplementation)
            {
                //LogLine("Unimplemented subinstruction, replacing with STD2.");

                var std2 = ImplementedSubinstructions[0] with { Name = "Unimplemented Subinstruction, replaced with STD2" };
                currentSubinstruction = std2;
                S = (word)(Z & BITMASK_1_12);
            }
        }

        private void FinalCleanup()
        {
            // TODO: Check for a pending GOJAM due to a hardware alarm, etc.
        }
        #endregion
    }
}
