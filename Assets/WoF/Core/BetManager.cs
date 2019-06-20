using System;
using UnityEngine;
using WoF.Core.Utils;

namespace WoF.Core
{
    [Serializable]
    public sealed class BetManager
    {
#pragma warning disable 649
        [SerializeField] private int[] possibilities = {10, 20, 50, 100, 200, 500, 1000, 2000, 5000};
#pragma warning restore 649

        private int _currentIndex = -1;
        public int CurrentIndex => _currentIndex;

        public void Init()
        {
            _currentIndex = 0;
            Common.FailOutOfRangeError<BetManager>(0, possibilities.Length);
        }

        public int CurrentBet
        {
            get
            {
                Common.FailOutOfRangeError<BetManager>(_currentIndex, possibilities.Length);
                return possibilities[_currentIndex];
            }
        }

        public bool CheckPrevious()
        {
            if (_currentIndex - 1 < 0)
            {
                return false;
            }

            return true;
        }

        public bool CheckNext()
        {
            if (_currentIndex + 1 >= possibilities.Length)
            {
                return false;
            }

            return true;
        }

        public bool Next()
        {
            var state = CheckNext();
            if (state)
            {
                _currentIndex++;
            }

            return state;
        }

        public bool Previous()
        {
            var state = CheckPrevious();
            if (state)
            {
                _currentIndex--;
            }

            return state;
        }

        public int this[int index]
        {
            get
            {
                Common.FailOutOfRangeError<BetManager>(index, possibilities.Length);
                return possibilities[index];
            }
        }
    }
}