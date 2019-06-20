using System;
using System.Numerics;

namespace WoF.Core.Utils.Extensions
{
    public static class BigIntegerExtensions
    {
        private const int MaxPrefix = 7;
        private static readonly char[] IncPrefixes = {'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y'};

        public static string MetricPrefix(this BigInteger value)
        {
            if (value == BigInteger.Zero)
            {
                return value.ToString();
            }

            var degree = (int) Math.Floor(BigInteger.Log10(BigInteger.Abs(value)) / 3);
            var scaled = BigInteger.Divide(
                value,
                BigInteger.Pow(1000, degree + (degree > 0 ? -1 : 0))
            );

            if (degree == 0)
            {
                return scaled.ToString();
            }

            if (degree > MaxPrefix)
            {
                return $"{scaled}*10^{degree}";
            }

            return (float) scaled / 1000f + IncPrefixes[degree - 1].ToString();
        }
    }
}