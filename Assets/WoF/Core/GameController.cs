using System;
using System.Text;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WoF.Core.Utils;
using WoF.Core.Utils.Exceptions;
using WoF.Core.Utils.Extensions;

namespace WoF.Core
{
    public sealed class GameController : MonoBehaviour
    {
        #region SerializeField

#pragma warning disable 649
        [SerializeField] private uint startingMoney = 1000;
        [SerializeField, Range(1, 5)] private int numberOfSpins = 3;
        [SerializeField, Range(1, 10)] private float maxLerpRotationTime = 4f;

        [SerializeField] private WheelChance[] wheelChances =
        {
            new WheelChance(1, 0.01f),
            new WheelChance(5, 0.07f),
            new WheelChance(2, 0.02f),
            new WheelChance(8, 0.15f),
            new WheelChance(9, 0.22f),
            new WheelChance(3, 0.03f),
            new WheelChance(6, 0.09f),
            new WheelChance(7, 0.11f),
            new WheelChance(4, 0.05f),
            new WheelChance(10, 0.25f)
        };

        [SerializeField] private BetManager betManager;
        [SerializeField] private Transform spinWheel;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button lessBetButton;
        [SerializeField] private Button moreBetButton;
        [SerializeField] private Text currentMoneyText;
        [SerializeField] private Text gainMoneyText;
        [SerializeField] private Text currentBetText;
        [SerializeField] private Text winChancesText;
        [SerializeField] private Text betWarningText;
#pragma warning restore 649

        #endregion

        private const float FontSizeDecrease = 0.03f;
        private const float AngleTolerance = 0.0001f;
        private const int MaxAngle = 360;

        private WheelFortune _wheelFortune;
        private float _currentLerpRotationTime;
        private float[] _sectorAngles;
        private float _finalAngle;
        private float _startAngle;
        private WheelChance[] _wheelChancesSorted;

        private int _chosenBet;
        private int _chosenWheelChanceIndex;

        private readonly ReactiveProperty<int> _currentBet = new ReactiveProperty<int>();
        private readonly ReactiveProperty<bool> _runningState = new ReactiveProperty<bool>();

        private void Start()
        {
            #region LocalFunctions

            void ObservableSubscribe()
            {
                void SpinButton()
                {
                    spinButton.onClick.AsObservable().Subscribe(_ => { TurnWheel(); });
                }

                void CurrentBet()
                {
                    var observable = _currentBet.AsObservable();
                    observable.SubscribeToText(currentBetText);
                    observable.Subscribe(_ =>
                    {
                        var winChances = new StringBuilder();
                        var fontSize = winChancesText.fontSize;
                        for (int i = _wheelChancesSorted.Length - 1, j = 0; i >= 0; i--, j++)
                        {
                            winChances.AppendLine(
                                $"<size={fontSize - fontSize * j * FontSizeDecrease}>{i + 1}. ${betManager.CurrentBet * _wheelChancesSorted[i].Factor} ({_wheelChancesSorted[i].Probability * 100}%)</size>");
                        }

                        winChancesText.text = winChances.ToString();
                    });
                }

                void BetButtons()
                {
                    var observableMoreBetButton = moreBetButton.onClick.AsObservable();
                    var observableLessBetButton = lessBetButton.onClick.AsObservable();
                    observableMoreBetButton.Subscribe(_ =>
                    {
                        if (betManager.Next())
                        {
                            _currentBet.Value = betManager.CurrentBet;
                        }
                    });
                    observableLessBetButton.Subscribe(_ =>
                    {
                        if (betManager.Previous())
                        {
                            _currentBet.Value = betManager.CurrentBet;
                        }
                    });
                    Observable.Merge(
                        observableMoreBetButton,
                        observableLessBetButton
                    ).Subscribe(_ => { SetInteractableBetButtons(); });
                }

                void RunningState()
                {
                    _runningState.Subscribe(state =>
                    {
                        if (state)
                        {
                            gainMoneyText.text = string.Empty;
                            spinButton.interactable = false;
                        }
                        else
                        {
                            currentMoneyText.text = _wheelFortune.CurrentMoney.MetricPrefix();
                            var gain = _wheelFortune.WheelChances[_chosenWheelChanceIndex].Factor * _chosenBet ;
                            gainMoneyText.text = $"{(gain >= 0 ? "+" : "-")}{gain}";
                            spinButton.interactable = true;
                        }
                    });
                }

                void ExitGame()
                {
                    Observable.EveryUpdate()
                        .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                        .Subscribe(_ =>
                        {
#if UNITY_EDITOR
                            EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                        });
                }

                SpinButton();
                CurrentBet();
                BetButtons();
                RunningState();
                ExitGame();
            }

            #endregion

#if !UNITY_EDITOR
        OnValidate();
#endif

            ObservableSubscribe();
        }

        private void Update()
        {
            #region LocalFunctions

            void CheckFinished()
            {
                if (
                    _currentLerpRotationTime > maxLerpRotationTime ||
                    Math.Abs(spinWheel.eulerAngles.z - _finalAngle) < AngleTolerance
                )
                {
                    _currentLerpRotationTime = maxLerpRotationTime;
                    _startAngle = _finalAngle % MaxAngle;
                    EndSpinning();
                }
            }

            void RotateWheel()
            {
                var angle = Mathf.Lerp(
                    _startAngle,
                    _finalAngle,
                    Common.WheelSpeed(_currentLerpRotationTime / maxLerpRotationTime)
                );
                spinWheel.eulerAngles = new Vector3(0, 0, angle);
            }

            #endregion

            if (!_runningState.Value)
            {
                return;
            }

            _currentLerpRotationTime += Time.deltaTime;
            CheckFinished();
            RotateWheel();
        }

        private void OnDestroy()
        {
            _currentBet.Dispose();
            _runningState.Dispose();
        }

        private void OnValidate()
        {
            #region LocalFunctions

            void InitSectorAngles()
            {
                _sectorAngles = new float[wheelChances.Length];
                for (var i = 0; i < wheelChances.Length; i++)
                {
                    _sectorAngles[i] = (MaxAngle / wheelChances.Length * i);
                }
            }

            void InitWheelChancesSorted()
            {
                _wheelChancesSorted = new WheelChance[_wheelFortune.NumberOfWheelChances];
                Array.Copy(_wheelFortune.WheelChances, _wheelChancesSorted, _wheelFortune.NumberOfWheelChances);
                Array.Sort(
                    _wheelChancesSorted,
                    (wheelChanceLeft, wheelChanceRight) => wheelChanceLeft.Factor.CompareTo(wheelChanceRight.Factor)
                );
            }

            #endregion

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif

            betManager.Init();
            _wheelFortune = new WheelFortune(wheelChances, startingMoney);

            InitSectorAngles();
            InitWheelChancesSorted();
            SetInteractableBetButtons();

            _currentBet.Value = betManager.CurrentBet;
        }

        private void TurnWheel()
        {
            if (betWarningText.enabled)
            {
                betWarningText.enabled = false;
            }

            _currentLerpRotationTime = 0f;
            _chosenBet = _currentBet.Value;
            try
            {
                _chosenWheelChanceIndex = _wheelFortune.Run(_chosenBet);
            }
            catch (NotEnoughMoneyException)
            {
                betWarningText.enabled = true;
                return;
            }

            _finalAngle = -(numberOfSpins * MaxAngle + _sectorAngles[_chosenWheelChanceIndex]);
            StartSpinning();
        }

        private void StartSpinning()
        {
            _runningState.Value = true;
        }

        private void EndSpinning()
        {
            _runningState.Value = false;
        }

        private void SetInteractableBetButtons()
        {
            moreBetButton.interactable = betManager.CheckNext();
            lessBetButton.interactable = betManager.CheckPrevious();
        }
    }
}