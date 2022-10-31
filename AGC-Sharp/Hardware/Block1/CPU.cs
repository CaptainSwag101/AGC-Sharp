using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        private byte CurrentTimepulse;
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
        #endregion

        #region Functions
        public CPU(AGC agc)
        {
            agcReference = agc;

            FetchNextInstruction = true;
            CurrentTimepulse = 12;
            Interrupts = new bool[11];
            Counters = new CounterAction[22];
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

        }

        public void Tick()
        {
            PrintStateInfo();

            WriteBus = 0;   // Clear the write lines
        }
        #endregion
    }
}
