using System;
using R3;
using UnityEngine;

namespace Orpita.Candle
{
    /// <summary>
    /// The candle: the player's health, vision, and timer in one. Burns
    /// continuously at a fixed rate regardless of player movement. Wax
    /// pickups, refill stations, and abilities like Boost Flame mutate the
    /// remaining time through the <see cref="ICandle"/> seam. Reaching zero
    /// fires <see cref="CandleDied"/>; other systems (light radius, audio,
    /// VFX, UI, danger stingers) react by subscribing to the exposed
    /// reactive state and events rather than polling this class.
    /// </summary>
    public sealed class CandleFuel : MonoBehaviour, ICandle
    {
        [Tooltip("Maximum fuel capacity in seconds.")]
        [SerializeField] private float maxSeconds = 15f;

        [Tooltip("Whether fuel drains each frame while the candle is lit.")]
        [SerializeField] private bool drainOverTime = true;

        [Tooltip("Seconds-remaining thresholds (descending) that raise DangerThresholdReached once each as the candle burns down. Re-arm on refill.")]
        [SerializeField] private int[] dangerThresholdsSeconds = { 5, 3 };

        private ReactiveProperty<float> _seconds;
        private ReactiveProperty<float> _normalized;
        private int _nextThresholdIndex;

        // Created lazily rather than in Awake: a UI view on another GameObject can
        // subscribe from its OnEnable before this component's Awake would have run,
        // and cross-GameObject Awake/OnEnable order is undefined.
        private ReactiveProperty<float> Fuel => _seconds ??= new ReactiveProperty<float>(maxSeconds);
        private ReactiveProperty<float> Normalized => _normalized ??= new ReactiveProperty<float>(1f);

        /// <summary>Remaining fuel as a reactive stream (clamped to [0, <see cref="MaxSeconds"/>]).</summary>
        public ReadOnlyReactiveProperty<float> SecondsRemainingRx => Fuel;

        /// <summary>Remaining fuel normalized to [0, 1], for UI meters and other systems.</summary>
        public ReadOnlyReactiveProperty<float> NormalizedRx => Normalized;

        /// <inheritdoc/>
        public float SecondsRemaining => Fuel.Value;

        /// <inheritdoc/>
        public float MaxSeconds => maxSeconds;

        /// <summary>True once the candle has burned out. Sticky until refilled/added back above zero.</summary>
        public bool IsDead { get; private set; }

        /// <summary>Raised once per threshold as remaining seconds crosses it going down.</summary>
        public event Action<int> DangerThresholdReached;

        /// <summary>Raised exactly once when remaining seconds transitions from &gt;0 to 0.</summary>
        public event Action CandleDied;

        private void Awake()
        {
            RearmThresholds(Fuel.Value);
        }

        private void Update()
        {
            if (!drainOverTime || IsDead)
                return;

            // Per-frame drain is this class's simulation loop; unconditional so
            // walking never changes burn rate.
            SetSecondsClamped(Fuel.Value - Time.deltaTime);
        }

        /// <inheritdoc/>
        public void Refill()
        {
            SetSecondsClamped(maxSeconds);
        }

        /// <inheritdoc/>
        public void AddSeconds(float seconds)
        {
            SetSecondsClamped(Fuel.Value + seconds);
        }

        /// <inheritdoc/>
        public void ConsumeTime(float seconds)
        {
            if (seconds < 0f)
            {
                Debug.LogWarning($"{nameof(ConsumeTime)} called with a negative amount ({seconds}); clamping to 0.", this);
                seconds = 0f;
            }

            SetSecondsClamped(Fuel.Value - seconds);
        }

        private void SetSecondsClamped(float newValue)
        {
            float oldValue = Fuel.Value;
            float clamped = Mathf.Clamp(newValue, 0f, maxSeconds);

            if (clamped > oldValue)
                RearmThresholds(clamped);

            Fuel.Value = clamped;
            Normalized.Value = maxSeconds > 0f ? clamped / maxSeconds : 0f;

            if (clamped < oldValue)
                FireCrossedThresholds(clamped);

            if (clamped > 0f)
            {
                IsDead = false;
            }
            else if (oldValue > 0f)
            {
                IsDead = true;
                CandleDied?.Invoke();
            }
        }

        private void FireCrossedThresholds(float clampedSeconds)
        {
            // Thresholds are sorted descending; walk forward from the next unfired
            // one and stop as soon as one hasn't been reached yet.
            while (_nextThresholdIndex < dangerThresholdsSeconds.Length &&
                   clampedSeconds <= dangerThresholdsSeconds[_nextThresholdIndex])
            {
                DangerThresholdReached?.Invoke(dangerThresholdsSeconds[_nextThresholdIndex]);
                _nextThresholdIndex++;
            }
        }

        private void RearmThresholds(float seconds)
        {
            _nextThresholdIndex = 0;
            while (_nextThresholdIndex < dangerThresholdsSeconds.Length &&
                   seconds <= dangerThresholdsSeconds[_nextThresholdIndex])
            {
                _nextThresholdIndex++;
            }
        }

        private void OnDestroy()
        {
            _seconds?.Dispose();
            _normalized?.Dispose();
        }
    }
}
