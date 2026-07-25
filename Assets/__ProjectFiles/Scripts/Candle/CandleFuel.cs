using R3;
using UnityEngine;

namespace Orpita.Candle
{
    /// <summary>
    /// The candle's survival countdown: a fixed pool of seconds that drains
    /// continuously, exposes its remaining time reactively, and announces the
    /// moment the flame goes out. Refills and candle loot write back through
    /// <see cref="Refill"/>/<see cref="AddSeconds"/>; the cost of special
    /// actions goes through <see cref="TryConsumeSeconds"/>. Hitting zero
    /// raises <see cref="Extinguished"/> — the actual player death is handled
    /// elsewhere, this class only announces the edge.
    ///
    /// <see cref="ICandle"/> is the stable seam interactables (refill stations,
    /// candle loot) code against, so this class can keep evolving without
    /// touching them.
    /// </summary>
    public sealed class CandleFuel : MonoBehaviour, ICandle
    {
        [Tooltip("Maximum fuel capacity in seconds.")]
        [SerializeField] private float maxSeconds = 15f;

        [Tooltip("Whether fuel drains each frame while the candle is lit.")]
        [SerializeField] private bool drainOverTime = true;

        [Tooltip("Candle seconds consumed per real second. 1 = real-time burn.")]
        [SerializeField, Min(0f)] private float burnRatePerSecond = 1f;

        private ReactiveProperty<float> _seconds;
        private ReactiveProperty<float> _normalized;

        // Edge signal for the moment the flame dies. Lazily created so a death
        // listener on another GameObject can subscribe from its OnEnable before
        // this component's Awake runs (cross-GameObject Awake/OnEnable order is
        // undefined).
        private Subject<Unit> _extinguished;

        // True once Extinguished has fired for the current burn-out; cleared the
        // moment fuel rises above 0 again so a subsequent burn-out re-fires.
        private bool _extinguishedFired;

        // Born lazily rather than in Awake, for the same cross-GameObject
        // ordering reason as _extinguished above. Seconds and normalized are
        // created together so a subscriber to either always observes a pair that
        // agrees about the same underlying fuel value.
        private void EnsureProperties()
        {
            if (_seconds == null)
            {
                _seconds = new ReactiveProperty<float>(maxSeconds);
                _normalized = new ReactiveProperty<float>(Normalize(maxSeconds));
            }
        }

        private ReactiveProperty<float> Fuel
        {
            get
            {
                EnsureProperties();
                return _seconds;
            }
        }

        /// <summary>Remaining fuel as a reactive stream (clamped to [0, <see cref="MaxSeconds"/>]).</summary>
        public ReadOnlyReactiveProperty<float> SecondsRemainingRx => Fuel;

        /// <summary>Remaining fuel normalized to [0, 1] (remaining / <see cref="MaxSeconds"/>).</summary>
        public ReadOnlyReactiveProperty<float> NormalizedRx
        {
            get
            {
                EnsureProperties();
                return _normalized;
            }
        }

        /// <summary>
        /// Edge-triggered signal that fires once when the candle transitions to
        /// 0 seconds. Re-arms the next time fuel rises above 0 (refill / wax),
        /// so a second burn-out fires again. Does NOT fire every frame at 0 and
        /// does NOT replay on subscribe — query <see cref="IsExtinguished"/>
        /// for the current state.
        /// </summary>
        public Observable<Unit> Extinguished => ExtinguishedSubject.AsObservable();

        private Subject<Unit> ExtinguishedSubject => _extinguished ??= new Subject<Unit>();

        /// <summary>True once the candle has burned down to 0 seconds.</summary>
        public bool IsExtinguished => SecondsRemaining <= 0f;

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

            // Per-frame drain is this candle's legitimate simulation loop (no
            // coroutine / Awaitable / R3 timer): scaled by burnRatePerSecond so
            // designers can retune burn speed without touching maxSeconds.
            value -= Time.deltaTime * burnRatePerSecond;
            SetFuel(value);
        }

        // Single write path for the fuel value. Clamps to [0, maxSeconds], keeps
        // both reactive properties in sync, and drives the Extinguished edge.
        // Every mutation (drain, refill, add, consume) routes through here so the
        // pair never drifts and the edge is always observed.
        private void SetFuel(float rawSeconds)
        {
            EnsureProperties();

            float clamped = Mathf.Clamp(rawSeconds, 0f, maxSeconds);

            // Co-located writes keep the two streams from diverging across frames;
            // seconds is authoritative, normalized mirrors it.
            _seconds.Value = clamped;
            _normalized.Value = Normalize(clamped);

            UpdateExtinguished(clamped);
        }

        // Fires Extinguished exactly once per visit to 0. The flag re-arms the
        // instant fuel goes above 0 again, so a refill-then-burn-out fires again.
        // OnNext is skipped when no subscriber has ever attached (_extinguished is
        // null) — the flag still tracks the edge for any later subscriber, which
        // should read IsExtinguished rather than expect a replay.
        private void UpdateExtinguished(float clamped)
        {
            if (clamped <= 0f)
            {
                if (_extinguishedFired)
                    return;
                _extinguishedFired = true;
                _extinguished?.OnNext(Unit.Default);
            }
            else
            {
                _extinguishedFired = false;
            }
        }

        private float Normalize(float seconds) => maxSeconds > 0f ? Mathf.Clamp01(seconds / maxSeconds) : 0f;

        /// <inheritdoc/>
        public void Refill() => SetFuel(maxSeconds);

        /// <inheritdoc/>
        public void AddSeconds(float seconds) => SetFuel(Fuel.Value + seconds);

        /// <summary>
        /// Spends <paramref name="seconds"/> of fuel if the candle can afford it.
        /// Returns false (changing nothing) for non-positive input or when there
        /// isn't that much fuel left. Cannot push fuel below 0 or resurrect an
        /// extinguished candle — the guard requires fuel &gt;= seconds &gt; 0.
        /// This is the Boost Flame cost hook.
        /// </summary>
        public bool TryConsumeSeconds(float seconds)
        {
            if (seconds <= 0f)
                return false;

            float current = Fuel.Value;
            if (current < seconds)
                return false;

            SetFuel(current - seconds);
            return true;
        }

        private void OnDestroy()
        {
            _seconds?.Dispose();
            _normalized?.Dispose();
            _extinguished?.Dispose();
        }
    }
}
