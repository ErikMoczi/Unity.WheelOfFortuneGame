using System;
using System.Numerics;

namespace WoF.Core.Utils.Exceptions
{
    public sealed class NotEnoughMoneyException : Exception
    {
        public NotEnoughMoneyException(BigInteger betAmount) : base(
            $"Not enough money {betAmount} to bet"
        )
        {
        }
    }
}