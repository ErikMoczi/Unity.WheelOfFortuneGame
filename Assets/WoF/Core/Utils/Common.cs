using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace WoF.Core.Utils
{
    public static class Common
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float WheelSpeed(float t)
        {
            return t * t * t * (t * (6f * t - 15f) + 10f);
        }

        public static BigInteger FindNearest(BigInteger number, BigInteger[] data)
        {
            var current = data[0];
            var difference = BigInteger.Abs(number - current);
            var index = data.Length;
            while (index > 1)
            {
                index--;
                var newDifference = BigInteger.Abs(number - data[index]);
                if (newDifference < difference)
                {
                    difference = newDifference;
                    current = data[index];
                }
            }

            return current;
        }

        [Conditional("DEBUG")]
        public static void FailOutOfRangeError<T>(int index, int max, int min = 0)
        {
            if (index < min || index >= max)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} used in {typeof(T)} is out of range of '{min}' - '{max}'."
                );
            }
        }
    }
}