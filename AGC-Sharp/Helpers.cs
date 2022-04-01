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

        public enum BitCopyMode
        {
            ClearAll,
            ClearChanged,
            ClearNone
        }

        public static ushort CopyWordBits(ushort source, ushort destination, Range sourceBitRange, Range destBitRange, BitCopyMode mode)
        {
            // Sanity checks
            if ((sourceBitRange.End.Value - sourceBitRange.Start.Value) != (destBitRange.End.Value - destBitRange.Start.Value))
                throw new ArgumentOutOfRangeException(nameof(sourceBitRange), "Source and destination bit ranges are not the same length.");
            if (sourceBitRange.Start.Value < 1 || sourceBitRange.End.Value > 16)
                throw new ArgumentOutOfRangeException(nameof(sourceBitRange), "Source bit range is outside the bounds of a 16-bit word.");
            if (sourceBitRange.Start.Value > sourceBitRange.End.Value)
                throw new ArgumentOutOfRangeException(nameof(sourceBitRange), "Source bit range start is greater than end.");
            if (destBitRange.Start.Value < 1 || destBitRange.End.Value > 16)
                throw new ArgumentOutOfRangeException(nameof(destBitRange), "Destination bit range is outside the bounds of a 16-bit word.");
            if (destBitRange.Start.Value > destBitRange.End.Value)
                throw new ArgumentOutOfRangeException(nameof(destBitRange), "Destination bit range start is greater than end.");

            // Input destBitRange range is one-based indexing to match documentation,
            // convert it to a zero-based indexed range.
            Range sourceBitRangeZeroBase = new(sourceBitRange.Start.Value - 1, sourceBitRange.End.Value - 1);
            Range destBitRangeZeroBase = new(destBitRange.Start.Value - 1, destBitRange.End.Value - 1);

            // Shift source bits to destination base
            int shiftOffset = destBitRangeZeroBase.Start.Value - sourceBitRangeZeroBase.Start.Value;
            ushort shiftedBits;
            if (shiftOffset >= 0)
                shiftedBits = (ushort)(source << shiftOffset);              // Shift left
            else
                shiftedBits = (ushort)(source >> Math.Abs(shiftOffset));    // Shift right

            // Mask only the range we intend to copy
            ushort bitMask = 0;
            for (ushort bitNum = 0; bitNum < 16; ++bitNum)
            {
                if (bitNum >= destBitRangeZeroBase.Start.Value && bitNum <= destBitRangeZeroBase.End.Value)
                {
                    bitMask |= (ushort)(1 << bitNum);   // Allow this bit in our mask
                }
            }
            shiftedBits &= bitMask; // Clear out any source bits that aren't targeted

            // Adjust the destination word according to our BitCopyMode
            switch (mode)
            {
                // Clear all destination bits
                case BitCopyMode.ClearAll:
                    destination = 0;
                    break;
                // Bitwise AND with the inverse of our mask to clear target bits
                case BitCopyMode.ClearChanged:
                    destination &= (ushort)~bitMask;
                    break;
                // Do nothing
                case BitCopyMode.ClearNone:
                    break;
            }

            // Finally bitwise OR our shifted & masked target bits into the destination
            destination |= shiftedBits;

            return destination;
        }
    }
}
