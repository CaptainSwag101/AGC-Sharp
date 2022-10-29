using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal partial class CPU
    {
        #region Central Registers
        private word A, LP, Q, Z, G, IN, OUT, B, BNK, U, X, Y;
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
        private bool[] Interrupts;
        private word[] Counters;
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
            Counters = new word[20];
        }

        public void GetErasableAddress()
        {

        }

        public void GetFixedAddress()
        {

        }

        public void UpdateAdder()
        {

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
