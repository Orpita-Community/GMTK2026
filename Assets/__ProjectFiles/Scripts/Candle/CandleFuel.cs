using R3;
using UnityEngine;

namespace Orpita.Candle
{
    /// <summary>
    /// PLACEHOLDER candle fuel implementation. Drains remaining time over the
    /// frame loop so refills and candle loot are observable in-game while the
    /// real candle mechanic is still pending. The stable surface is the
    /// <see cref="ICandle"/> interface; the full candle system (a separate task)
    /// may replace this class wholesale without touching consumers.
    /// </summary>
    public sealed class CandleFuel : MonoBehaviour, ICandle
    {
        [Tooltip("Maximum fuel capacity in seconds.")]
        [SerializeField] private float maxSeconds = 15f;

        [Tooltip("Whether fuel drains each frame while the candle is lit.")]
        [SerializeField] private bool drainOverTime = true;

        private ReactiveProperty<float> _seconds;

        /// <summary>Remaining fuel as a reactive stream (clamped to [0, <see cref="MaxSeconds"/>]).</summary>
        public ReadOnlyReactiveProperty<float> SecondsRemainingRx => _seconds;

        /// <inheritdoc/>
        public float SecondsRemaining => _seconds.Value;

        /// <inheritdoc/>
        public float MaxSeconds => maxSeconds;

        private void Awake()
        {
            _seconds = new ReactiveProperty<float>(maxSeconds);
        }

        private void Update()
        {
            if (!drainOverTime)
                return;

            float value = _seconds.Value;
            if (value <= 0f)
                return;

            // Per-frame drain is this placeholder's legitimate simulation loop.
            value -= Time.deltaTime;
            if (value < 0f)
                value = 0f;
            _seconds.Value = value;
        }

        /// <inheritdoc/>
        public void Refill()
        {
            _seconds.Value = maxSeconds;
        }

        /// <inheritdoc/>
        public void AddSeconds(float seconds)
        {
            _seconds.Value = Mathf.Clamp(_seconds.Value + seconds, 0f, maxSeconds);
        }

        private void OnDestroy()
        {
            _seconds?.Dispose();
        }
    }
}
