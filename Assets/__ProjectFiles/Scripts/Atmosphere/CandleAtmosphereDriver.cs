using System;
using Orpita.Candle;
using R3;
using UnityEngine;
using UnityEngine.Rendering;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Blends a secondary post-processing <see cref="Volume"/> in as the candle
    /// burns down, so the screen closes in and desaturates when the player is
    /// about to lose their light.
    ///
    /// The look itself lives entirely in the two volume profiles; this component
    /// only drives the blend weight, so artists can retune the low-fuel grade
    /// without touching code.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Volume))]
    public sealed class CandleAtmosphereDriver : MonoBehaviour
    {
        [Tooltip("Candle whose remaining fuel drives the blend.")]
        [SerializeField] private CandleFuel candle;

        [Tooltip("Normalized fuel level at which the low-candle look starts blending in.")]
        [SerializeField, Range(0f, 1f)] private float blendStart = 0.4f;

        [Tooltip("Blend weight reached when the candle is fully spent.")]
        [SerializeField, Range(0f, 1f)] private float maxWeight = 1f;

        private Volume _volume;
        private IDisposable _subscription;

        private void OnEnable()
        {
            // Resolved here rather than in Awake: OnEnable can run before Awake
            // after a domain reload, and this component owns no other state.
            _volume = GetComponent<Volume>();
            _volume.weight = 0f;

            if (candle == null)
            {
                Debug.LogError($"{nameof(CandleAtmosphereDriver)} on '{name}' requires a {nameof(CandleFuel)} reference.", this);
                return;
            }

            _subscription = candle.SecondsRemainingRx.Subscribe(OnFuelChanged);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;

            if (_volume != null)
                _volume.weight = 0f;
        }

        private void OnFuelChanged(float seconds)
        {
            float maxSeconds = candle.MaxSeconds;
            if (maxSeconds <= 0f)
                return;

            float fuel01 = Mathf.Clamp01(seconds / maxSeconds);
            float blend = blendStart > 0f
                ? Mathf.Clamp01(1f - fuel01 / blendStart)
                : (fuel01 <= 0f ? 1f : 0f);

            _volume.weight = blend * maxWeight;
        }
    }
}
