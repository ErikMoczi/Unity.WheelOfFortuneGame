using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using WoF.Core.Utils;
using WoF.Core.Utils.Exceptions;
using Random = UnityEngine.Random;

namespace WoF.Core
{
    public sealed class WheelFortune
    {
        private readonly WheelChance[] _wheelChances;
        private readonly float _maxProbability;

        public BigInteger CurrentMoney { get; private set; }
        public BigInteger TotalSpins { get; private set; } = BigInteger.Zero;
        public WheelChance[] WheelChances => _wheelChances;
        public int NumberOfWheelChances => _wheelChances.Length;

        public WheelFortune([NotNull] WheelChance[] wheelChances, uint startingMoney)
        {
            CheckWheelChances(wheelChances);

            _wheelChances = wheelChances;
            CurrentMoney = startingMoney;
            _maxProbability = wheelChances.Sum(x => x.Probability);
        }

        public int Run(BigInteger betAmount)
        {
#if DEBUG
            if (betAmount < BigInteger.Zero)
            {
                throw new Exception($"{nameof(betAmount)} is less then {BigInteger.Zero}");
            }
#endif

            if (CurrentMoney - betAmount < BigInteger.Zero)
            {
                throw new NotEnoughMoneyException(betAmount);
            }

            var randomChance = Random.Range(0f, _maxProbability);
            for (var i = 0; i < _wheelChances.Length; i++)
            {
                if (randomChance <= _wheelChances[i].Probability)
                {
                    TotalSpins++;
                    CurrentMoney -= betAmount;
                    CurrentMoney += _wheelChances[i].Factor * betAmount;
                    return i;
                }

                randomChance -= _wheelChances[i].Probability;
            }

            throw new Exception("Unexpected problem with guessing probability");
        }

        public WheelChance GetWheelChances(int index)
        {
            Common.FailOutOfRangeError<WheelFortune>(index, _wheelChances.Length);
            return _wheelChances[index];
        }

        [Conditional("DEBUG")]
        private static void CheckWheelChances(WheelChance[] wheelChances)
        {
            if (wheelChances == null)
            {
                throw new ArgumentNullException(nameof(wheelChances));
            }

            if (wheelChances.Length == 0)
            {
                throw new Exception($"{nameof(wheelChances)} is empty");
            }
        }
    }
}