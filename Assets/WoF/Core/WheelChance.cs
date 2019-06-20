using System;
using UnityEngine;

namespace WoF.Core
{
    [Serializable]
    public struct WheelChance
    {
        [SerializeField] private int factor;
        [SerializeField] private float probability;

        public WheelChance(int factor, float probability)
        {
            this.factor = factor;
            this.probability = probability;
        }

        public int Factor => factor;
        public float Probability => probability;
    }
}