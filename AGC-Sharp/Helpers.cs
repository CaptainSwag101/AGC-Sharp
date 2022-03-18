using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp
{
    internal static class Helpers
    {
        public static ushort Bit15To16(ushort inVal)
        {
            inVal &= 0b0111111111111111;    // Mask out bit 16 just in case
            inVal |= (ushort)((inVal << 1) & 0b1000000000000000);   // Copy bit 15 into bit 16
            return inVal;
        }

        public static ushort Bit16To15(ushort inVal, bool clearBit16)
        {
            inVal &= 0b1011111111111111;    // Mask out the parity bit
            inVal |= (ushort)((inVal >> 1) & 0b0100000000000000);   // Copy the sign bit into bit 15
            if (clearBit16) inVal &= 0b0111111111111111;    // Mask out the old bit 16
            return inVal;
        }
    }
}
