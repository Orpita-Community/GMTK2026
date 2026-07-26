using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Orpita.Candle;

namespace Orpita.Lighting
{
    /// <summary>
    /// Drives the player's candle light radius from remaining candle fuel: the
    /// radius shrinks smoothly toward zero as the candle burns down (mapped
    /// through an ease-in-out curve for a dramatic shrink near empty), and can
    /// be temporarily overridden for a boost effect. Other systems (ghost AI)
    /// query <see cref="IsPositionInRadius"/> rather than polling raw values.
    /// </summary>
    public sealed class LightRadiusManager : MonoBehaviour
    {
        [Tooltip("The candle light carried by the player. If empty, resolved from a child at Awake.")]
        [SerializeField] private Light2D playerLight;

        [Tooltip("The candle fuel source. If empty, resolved from a child at Awake.")]
        [SerializeField] private CandleFuel candle;

        [Tooltip("Maps normalized fuel [0,1] to a radius fraction [0,1] of the full baseline radius.")]
        [SerializeField] private AnimationCurve normalizedToRadiusCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("SmoothDamp time (seconds) for the radius to catch up to its target.")]
        [SerializeField, Min(0f)] private float smoothTime = 0.3f;

        [Tooltip("Multiplier applied to the full baseline radius while boosted.")]
        [SerializeField, Min(1f)] private float boostRadiusMultiplier = 3f;

        private float _maxRadius;
        private float _currentRadius;
        private float _radiusVelocity;
        private float _normalizedFuel = 1f;
        private bool _boosted;

        private ReactiveProperty<float> _currentRadiusRx;
        private CompositeDisposable _subs;

        private ReactiveProperty<float> CurrentRadiusRxProperty => _currentRadiusRx ??= new ReactiveProperty<float>(0f);

        /// <summary>The light's current outer radius.</summary>
        public float CurrentRadius => _currentRadius;

        /// <summary>Reactive stream of <see cref="CurrentRadius"/>, for systems that want change notification.</summary>
        public ReadOnlyReactiveProperty<float> CurrentRadiusRx => CurrentRadiusRxProperty;

        private void Awake()
        {
            playerLight ??= GetComponentInChildren<Light2D>();
            candle ??= GetComponentInChildren<CandleFuel>();

            if (playerLight == null || candle == null)
            {
                Debug.LogError(
                    $"{nameof(LightRadiusManager)} on '{name}' requires a {nameof(Light2D)} and a {nameof(CandleFuel)} in children.",
                    this);
                enabled = false;
                return;
            }

            _maxRadius = playerLight.pointLightOuterRadius;
            _currentRadius = _maxRadius;
        }

        private void OnEnable()
        {
            _subs = new CompositeDisposable();

            if (candle != null)
                candle.NormalizedRx.Subscribe(v => _normalizedFuel = v).AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
        }

        private void Update()
        {
            float target = _boosted
                ? _maxRadius * boostRadiusMultiplier
                : _maxRadius * normalizedToRadiusCurve.Evaluate(_normalizedFuel);

            _currentRadius = Mathf.SmoothDamp(_currentRadius, target, ref _radiusVelocity, smoothTime);
            playerLight.pointLightOuterRadius = _currentRadius;
            CurrentRadiusRxProperty.Value = _currentRadius;
        }

        /// <summary>Expands the target radius to the boosted radius. No current caller — a future ability hooks in here.</summary>
        public void BeginBoost() => _boosted = true;

        /// <summary>Resumes the candle-driven target radius (eases back down, no snap).</summary>
        public void EndBoost() => _boosted = false;

        /// <summary>True while <paramref name="position"/> is within the current light radius (simple distance check, no occlusion).</summary>
        public bool IsPositionInRadius(Vector2 position)
        {
            return Vector2.Distance(playerLight.transform.position, position) <= _currentRadius;
        }

        private void OnDestroy()
        {
            _currentRadiusRx?.Dispose();
        }
    }
}
