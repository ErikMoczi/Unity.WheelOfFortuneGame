using System.Numerics;

namespace WoF.Tests
{
    public interface IBetSize
    {
        BigInteger PickBet(BigInteger currentMoney);
    }

    public sealed class PercentageBetSize : IBetSize
    {
        private readonly int _value;

        public PercentageBetSize(int value)
        {
            _value = value;
        }

        public BigInteger PickBet(BigInteger currentMoney)
        {
            return currentMoney * _value / 100;
        }

        public override string ToString()
        {
            return $"{nameof(PercentageBetSize)}{{{_value}}}";
        }
    }

    public sealed class FixedBetSize : IBetSize
    {
        private readonly int _value;

        public FixedBetSize(int value)
        {
            _value = value;
        }

        public BigInteger PickBet(BigInteger currentMoney)
        {
            return _value;
        }

        public override string ToString()
        {
            return $"{nameof(FixedBetSize)}{{{_value}}}";
        }
    }
}