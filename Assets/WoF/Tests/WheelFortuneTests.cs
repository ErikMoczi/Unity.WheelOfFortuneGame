using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using WoF.Core;
using WoF.Core.Utils;
using WoF.Core.Utils.Exceptions;

namespace WoF.Tests
{
    public sealed class WheelFortuneTests
    {
        private static IEnumerable<object> TestCases()
        {
            yield return new object[]
            {
                new PercentageBetSize(5),
                10000,
                10000u
            };
            yield return new object[]
            {
                new FixedBetSize(100),
                10000,
                10000u
            };
        }

        [Test, TestCaseSource(nameof(TestCases)), Category("Statistics")]
        public void GenerateStatistics(IBetSize betSize, int maximumSpins, uint startingMoney)
        {
            var wheelChances = new[]
            {
                new WheelChance(0, 0.8f),
                new WheelChance(1, 0.08f),
                new WheelChance(10, 0.05f),
                new WheelChance(25, 0.03f),
                new WheelChance(50, 0.03f),
                new WheelChance(100, 0.01f),
            };

            var betPossibilities = new BigInteger[]
            {
                10, 20, 30, 50, 100, 200, 300, 500, 1000, 2000, 3000, 5000, 10000, 20000, 30000, 50000, 100000, 200000,
                300000, 1000000
            };

            var wheelFortune = new WheelFortune(wheelChances, startingMoney);
            var dataOutput = new StringBuilder();
            while (wheelFortune.TotalSpins < maximumSpins)
            {
                var currentBet = Common.FindNearest(
                    betSize.PickBet(wheelFortune.CurrentMoney),
                    betPossibilities
                );
                var currentMoney = wheelFortune.CurrentMoney;

                try
                {
                    var chosenWheelChanceIndex = wheelFortune.Run(currentBet);
                    var wheelChance = wheelFortune.GetWheelChances(chosenWheelChanceIndex);
                    dataOutput.AppendLine(
                        $"{wheelFortune.TotalSpins}, {currentMoney}, {currentBet}, {wheelChance.Factor * currentBet}, {wheelChance.Factor}"
                    );
                }
                catch (NotEnoughMoneyException)
                {
                    break;
                }
            }

            CreateStreamingAssetsFolder();
            CreatePerformanceTestRunJson(dataOutput, betSize.ToString());
        }

        private static void CreatePerformanceTestRunJson(StringBuilder text, string name)
        {
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, $"{name}.csv"), text.ToString());
            UnityEditor.AssetDatabase.Refresh();
        }

        private static void CreateStreamingAssetsFolder()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }
        }
    }
}