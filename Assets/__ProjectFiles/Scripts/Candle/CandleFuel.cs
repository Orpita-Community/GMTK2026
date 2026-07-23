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

        // Created lazily rather than in Awake: a UI view on another GameObject can
        // subscribe from its OnEnable before this component's Awake would have run,
        // and cross-GameObject Awake/OnEnable order is undefined.
        private ReactiveProperty<float> Fuel => _seconds ??= new ReactiveProperty<float>(maxSeconds);

        /// <summary>Remaining fuel as a reactive stream (clamped to [0, <see cref="MaxSeconds"/>]).</summary>
        public ReadOnlyReactiveProperty<float> SecondsRemainingRx => Fuel;

        /// <inheritdoc/>
        public float SecondsRemaining => Fuel.Value;

        /// <inheritdoc/>
        public float MaxSeconds => maxSeconds;

        private void Update()
        {
            if (!drainOverTime)
                return;

            float value = Fuel.Value;
            if (value <= 0f)
                return;

            // Per-frame drain is this placeholder's legitimate simulation loop.
            value -= Time.deltaTime;
            if (value < 0f)
                value = 0f;
            Fuel.Value = value;
        }

        /// <inheritdoc/>
        public void Refill()
        {
            Fuel.Value = maxSeconds;
        }

        /// <inheritdoc/>
        public void AddSeconds(float seconds)
        {
            Fuel.Value = Mathf.Clamp(Fuel.Value + seconds, 0f, maxSeconds);
        }

        private void OnDestroy()
        {
            _seconds?.Dispose();
        }
    }
}
