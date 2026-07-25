using System;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Orpita.Candle
{
    /// <summary>
    /// Presents candle fuel as the player's carried <see cref="Light2D"/>: the flame
    /// shrinks, dims and reddens toward an ember as fuel runs out, plus a continuous
    /// flicker on top.
    ///
    /// Pure presentation — it never writes back to the candle. Fuel remains owned by
    /// <see cref="CandleFuel"/>; refills grow the light back for free.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light2D))]
    public sealed class CandleLightView : MonoBehaviour
    {
        [Tooltip("Candle whose remaining fuel drives this light.")]
        [SerializeField] private CandleFuel candle;

        [Header("Flame size")]
        [Tooltip("Outer radius at full fuel.")]
        [SerializeField, Min(0f)] private float fullOuterRadius = 3.4f;

        [Tooltip("Outer radius when the candle is spent.")]
        [SerializeField, Min(0f)] private float emptyOuterRadius = 0.9f;

        [Tooltip("Inner radius as a fraction of the current outer radius.")]
        [SerializeField, Range(0f, 1f)] private float innerRadiusRatio = 0.35f;

        [Header("Flame brightness and color")]
        [SerializeField, Min(0f)] private float fullIntensity = 1.15f;
        [SerializeField, Min(0f)] private float emptyIntensity = 0.12f;
        [SerializeField] private Color flameColor = new Color(1f, 0.82f, 0.55f);
        [SerializeField] private Color emberColor = new Color(1f, 0.48f, 0.22f);

        [Tooltip("Remaps normalized fuel before it drives size/brightness. Left = empty, right = full.")]
        [SerializeField] private AnimationCurve fuelResponse = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Flicker")]
        [SerializeField] private bool flickerEnabled = true;

        [Tooltip("Peak intensity variation, as a fraction of the current brightness.")]
        [SerializeField, Range(0f, 0.5f)] private float flickerIntensityAmount = 0.08f;

        [Tooltip("Peak radius variation, as a fraction of the current radius.")]
        [SerializeField, Range(0f, 0.5f)] private float flickerRadiusAmount = 0.05f;

        [SerializeField, Min(0.01f)] private float flickerSpeed = 6.5f;

        private Light2D _light;
        private IDisposable _subscription;

        // Fuel-derived values before flicker is layered on; refreshed only when fuel changes.
        private float _baseOuterRadius;
        private float _baseIntensity;

        // Offsets keep several candles in a scene from flickering in lockstep.
        private float _noiseOffsetIntensity;
        private float _noiseOffsetRadius;

        // Guards the frame between enabling and the first fuel notification.
        private bool _hasFuelState;

        private void OnEnable()
        {
            // Resolved here rather than in Awake: OnEnable can run before Awake after a
            // domain reload, and this component holds no state that needs earlier setup.
            _light = GetComponent<Light2D>();

            _noiseOffsetIntensity = UnityEngine.Random.value * 100f;
            _noiseOffsetRadius = UnityEngine.Random.value * 100f;

            if (candle == null)
            {
                Debug.LogError($"{nameof(CandleLightView)} on '{name}' requires a {nameof(CandleFuel)} reference.", this);
                enabled = false;
                return;
            }

            _subscription = candle.SecondsRemainingRx.Subscribe(OnFuelChanged);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
            _hasFuelState = false;
        }

        // Flicker is continuous per-frame noise, so it belongs in a frame callback while
        // the fuel response stays reactive. LateUpdate, not Update: the candle drains in
        // its own Update, and script execution order between the two is undefined — from
        // LateUpdate the flicker always lands on top of the current frame's fuel value
        // instead of being overwritten by it.
        private void LateUpdate()
        {
            if (!_hasFuelState)
                return;

            if (!flickerEnabled)
            {
                ApplyToLight(_baseOuterRadius, _baseIntensity);
                return;
            }

            float time = Time.time * flickerSpeed;
            float intensityNoise = Mathf.PerlinNoise(_noiseOffsetIntensity, time) * 2f - 1f;
            float radiusNoise = Mathf.PerlinNoise(_noiseOffsetRadius, time) * 2f - 1f;

            ApplyToLight(
                _baseOuterRadius * (1f + radiusNoise * flickerRadiusAmount),
                _baseIntensity * (1f + intensityNoise * flickerIntensityAmount));
        }

        private void OnFuelChanged(float seconds)
        {
            float maxSeconds = candle.MaxSeconds;
            float fuel01 = maxSeconds > 0f ? Mathf.Clamp01(seconds / maxSeconds) : 0f;
            float t = Mathf.Clamp01(fuelResponse.Evaluate(fuel01));

            _baseOuterRadius = Mathf.Lerp(emptyOuterRadius, fullOuterRadius, t);
            _baseIntensity = Mathf.Lerp(emptyIntensity, fullIntensity, t);
            _light.color = Color.Lerp(emberColor, flameColor, t);
            _hasFuelState = true;
        }

        private void ApplyToLight(float outerRadius, float intensity)
        {
            _light.pointLightOuterRadius = outerRadius;
            _light.pointLightInnerRadius = outerRadius * innerRadiusRatio;
            _light.intensity = intensity;
        }

        private void OnValidate()
        {
            if (emptyOuterRadius > fullOuterRadius)
                emptyOuterRadius = fullOuterRadius;

            if (emptyIntensity > fullIntensity)
                emptyIntensity = fullIntensity;
        }
    }
}
