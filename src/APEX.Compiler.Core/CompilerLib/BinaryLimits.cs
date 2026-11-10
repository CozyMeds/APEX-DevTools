using System;

namespace T7CompilerLib
{
    internal static class BinaryLimits
    {
        internal static byte ByteCount(int count, string description)
        {
            if (count < 0 || count > byte.MaxValue)
                throw new InvalidOperationException(description + " exceeds the bytecode limit of 255.");
            return (byte)count;
        }

        internal static ushort UShortCount(int count, string description)
        {
            if (count < 0 || count > ushort.MaxValue)
                throw new InvalidOperationException(description + " exceeds the bytecode limit of 65535.");
            return (ushort)count;
        }
    }
}
