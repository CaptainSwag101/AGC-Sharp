using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp
{
    internal class Memory
    {
        public enum MemoryInitState
        {
            BitsClear,
            BitsSet,
            Random
        }

        private const int ERASABLE_MEM_SIZE = 2048;
        private const int FIXED_MEM_SIZE = 36864;

        public ushort[] Erasable { get; private set; }
        public ushort[] Fixed { get; private set; }

        /// <summary>
        /// Initializes the emulated AGC's two sections of memory, with the specified data pattern.
        /// </summary>
        /// <param name="initState">The desired memory initialization pattern.</param>
        public Memory(MemoryInitState initState = MemoryInitState.BitsClear)
        {
            Erasable = new ushort[ERASABLE_MEM_SIZE];
            Fixed = new ushort[FIXED_MEM_SIZE];

            
            if (initState == MemoryInitState.BitsClear)
            {
                // C# already initializes arrays to zero, so do nothing.
            }
            else if (initState == MemoryInitState.BitsSet)
            {
                for (int i = 0; i < ERASABLE_MEM_SIZE; ++i)
                {
                    Erasable[i] = ushort.MaxValue;
                }
                for (int i = 0; i < FIXED_MEM_SIZE; ++i)
                {
                    Fixed[i] = ushort.MaxValue;
                }
            }
            else if (initState == MemoryInitState.Random)
            {
                Random memRand = new();

                for (int i = 0; i < ERASABLE_MEM_SIZE; ++i)
                {
                    Erasable[i] = (ushort)memRand.Next(ushort.MaxValue);
                }
                for (int i = 0; i < FIXED_MEM_SIZE; ++i)
                {
                    Fixed[i] = (ushort)memRand.Next(ushort.MaxValue);
                }
            }
        }

        public ushort ReadWord(ushort address, Cpu cpu)
        {
            // Sanity check
            if (address >= (0x0FFF))
            {
                throw new ArgumentOutOfRangeException(nameof(address),
                    $"The specified address is outside the bounds of total memory ({0x0FFF} words).");
            }

            if (address < 0x400)    // Erasable memory
            {
                if (address >= 0x300 && address <= 0x3FF)   // Banked erasable memory
                {
                    address &= 0x00FF;
                    address |= (ushort)(cpu.RegisterEB & 0x0F00);
                    return ReadErasable(address);
                }
                else
                {
                    return ReadErasable(address);
                }
            }
            else                    // Fixed memory
            {
                if (address >= 0x400 && address <= 0x7FF)   // Bankable fixed memory
                {
                    address &= 0x07FF;
                    address |= (ushort)((cpu.RegisterFB & 0x7C00) << 2);
                    return ReadFixed(address);
                }
                else
                {
                    return ReadFixed(address);
                }
            }
        }

        private ushort ReadErasable(ushort address)
        {
            ushort temp = Erasable[address];
            Erasable[address] = 0;  // Erasable reads are destructive, value is zeroed until writeback
            return temp;
        }

        private ushort ReadFixed(ushort address)
        {
            return Fixed[address];
        }

        public void WriteErasableWord(ushort address, ushort value)
        {
            Erasable[address] = value;
        }

        /// <summary>
        /// Writes a block of words into erasable memory.
        /// Generally used for loading a memory state at computer startup.
        /// </summary>
        /// <param name="wordArray">The array of words to be written into memory.</param>
        /// <param name="index">The zero-based starting index where the array will be placed.</param>
        public void WriteErasableBlock(ushort[] wordArray, int index = 0)
        {
            if (wordArray.Length > ERASABLE_MEM_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(wordArray),
                    $"The provided array is larger than the size of erasable memory ({ERASABLE_MEM_SIZE} words).");
            }
            else if (index + wordArray.Length > ERASABLE_MEM_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The provided index would prevent some or all of the array from fitting within erasable memory ({ERASABLE_MEM_SIZE} words).");
            }

            for (int i = 0; i < wordArray.Length; ++i)
            {
                Erasable[index + i] = wordArray[i];
            }
        }

        /// <summary>
        /// Writes a block of words into fixed memory.
        /// Generally used for loading a core rope program at computer startup.
        /// </summary>
        /// <param name="wordArray">The array of words to be written into memory.</param>
        /// <param name="index">The zero-based starting index where the array will be placed.</param>
        public void WriteFixedBlock(ushort[] wordArray, int index = 0)
        {
            if (wordArray.Length > FIXED_MEM_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(wordArray),
                    $"The provided array is larger than the size of fixed memory ({FIXED_MEM_SIZE} words).");
            }
            else if (index + wordArray.Length > FIXED_MEM_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The provided index would prevent some or all of the array from fitting within fixed memory ({FIXED_MEM_SIZE} words).");
            }

            for (int i = 0; i < wordArray.Length; ++i)
            {
                Fixed[index + i] = wordArray[i];
            }
        }
    }
}
