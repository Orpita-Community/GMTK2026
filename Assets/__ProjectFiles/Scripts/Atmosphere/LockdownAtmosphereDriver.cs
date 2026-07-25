using LitMotion;
using R3;
using Orpita.Core;
using UnityEngine;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Blends the artist-authored Lockdown grade onto the camera when Lockdown
    /// Mode begins, then leaves it in place for the rest of the run.
    ///
    /// <para>
    /// Lockdown is a one-shot, latching state (see
    /// <see cref="ILockdownState"/>): once it starts it never reverts. This
    /// driver therefore only has a blend-in path — there is no fade-out to
    /// design, tune, or accidentally leave half-run. The grade itself (cooler
    /// colour temperature, deeper vignette, etc.) lives entirely in the
    /// referenced volume profile, so this component moves a single weight float
    /// and artists can retune the look without touching code.
    /// </para>
    ///
    /// <para>
    /// <see cref="ILockdownState.IsActiveRx"/> replays its current value on
    /// subscribe. A component enabled after lockdown has already started (a
    /// GameObject toggled off/on, or an editor domain reload) would otherwise
    /// re-play the full fade every single time it re-enables — reading to the
    /// player as the grade glitching off and fading back on. The first emission
    /// is therefore snapped straight to the target weight; only a genuine
    /// false→true transition observed while live is tweened, per the "smooth
    /// transitions" acceptance criterion.
    /// </para>
    /// </summary>
    public sealed class LockdownAtmosphereDriver : VolumeBlendDriver
    {
        [Tooltip("Lockdown state source. Required.")]
        [SerializeField] private LockdownManager lockdown;

        [Tooltip("Seconds the grade takes to blend in when lockdown starts.")]
        [SerializeField, Min(0.01f)] private float blendSeconds = 1.5f;

        [Tooltip("Blend weight reached once lockdown is fully established.")]
        [SerializeField, Range(0f, 1f)] private float maxWeight = 1f;

        // Resolved from the serialized concrete reference in
        // TryResolveDependencies. Everything below resolution goes through the
        // interface so this driver depends on the lockdown abstraction, not the
        // manager's concrete side effects.
        private ILockdownState _lockdownState;

        private MotionHandle _motion;

        // Distinguishes the replayed first value (snap if active) from a real
        // transition observed while subscribed (tween). Reset per enable cycle
        // since SubscribeDriver runs on every OnEnable.
        private bool _sawInitialValue;

        /// <inheritdoc/>
        protected override bool TryResolveDependencies()
        {
            if (!RequireReference(lockdown, nameof(LockdownManager)))
                return false;

            _lockdownState = lockdown;
            return true;
        }

        /// <inheritdoc/>
        protected override void SubscribeDriver(CompositeDisposable subscriptions)
        {
            _sawInitialValue = false;
            _lockdownState.IsActiveRx.Subscribe(OnLockdownChanged).AddTo(subscriptions);
        }

        /// <inheritdoc/>
        protected override void OnDriverDisable()
        {
            // The base zeroes Weight immediately after OnDriverDisable returns,
            // so this only needs to cancel the in-flight tween.
            CancelMotion();
        }

        private void OnLockdownChanged(bool active)
        {
            if (!_sawInitialValue)
            {
                // Replayed current value. If lockdown is already live, snap to
                // the target so a re-enable doesn't replay the fade; if not,
                // the weight is already 0 (base reset on enable) so do nothing.
                _sawInitialValue = true;
                if (active)
                {
                    CancelMotion();
                    Weight = maxWeight;
                }
                return;
            }

            // Lockdown latches: the only live transition is false→true, so a
            // late 'false' never carries meaning and is ignored (the base
            // already zeroes the weight on disable).
            if (!active)
                return;

            CancelMotion();
            _motion = LMotion.Create(Weight, maxWeight, blendSeconds)
                .WithEase(Ease.OutQuad)
                .Bind(w => Weight = w);
        }

        private void CancelMotion()
        {
            if (_motion.IsActive())
                _motion.TryCancel();
            _motion = default;
        }
    }
}
