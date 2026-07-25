using LitMotion;
using R3;
using UnityEngine;
using Orpita.Player;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Pulses a dedicated damage-flash volume (a red vignette plus a warm color
    /// filter, authored by an artist in a profile asset) whenever the player
    /// takes a hit. The component moves only the volume's blend weight — the
    /// look itself lives entirely in the profile, so the grade can be retuned
    /// without touching code.
    ///
    /// <para>
    /// This is a separate volume rather than a tween on an existing grade's
    /// properties for two reasons: the flash must read over both the candle and
    /// lockdown grades, which it does by sitting at a higher priority in the
    /// volume stack; and a single weight tween is the cheapest possible way to
    /// drive a whole art-authored look.
    /// </para>
    ///
    /// <para>
    /// <see cref="peakWeight"/> is capped below 1 by default so the hit
    /// feedback never fully replaces the gameplay frame — the acceptance
    /// criterion says "effects never obscure critical gameplay visibility," and
    /// a full-weight red vignette would.
    /// </para>
    /// </summary>
    public sealed class DamageFlashDriver : VolumeBlendDriver
    {
        [Tooltip("Health component whose damage events trigger the flash. Required.")]
        [SerializeField] private PlayerHealth health;

        [Tooltip("Total duration of one flash, in seconds (rise and fall combined).")]
        [SerializeField, Min(0.02f)] private float flashSeconds = 0.2f;

        [Tooltip("Peak blend weight at the top of the flash. Below 1 keeps the hit readable.")]
        [SerializeField, Range(0f, 1f)] private float peakWeight = 0.85f;

        private MotionHandle _motion;

        protected override bool TryResolveDependencies()
            => RequireReference(health, nameof(PlayerHealth));

        protected override void SubscribeDriver(CompositeDisposable subscriptions)
        {
            health.DamageTaken.Subscribe(OnDamageTaken).AddTo(subscriptions);
        }

        // OnDriverDisable is the only release hook we get: the base class owns
        // OnEnable/OnDisable privately. Cancel the tween here; the base resets
        // Weight to 0 immediately afterward, so we intentionally do NOT touch
        // Weight in this method.
        protected override void OnDriverDisable()
        {
            CancelMotion();
        }

        private void OnDamageTaken(int _)
        {
            // Deliberate difference from InvulnerabilityFlicker: no IsDead guard
            // here. PlayerHealth raises DamageTaken before Died on a killing blow,
            // and the lethal hit is the one that most deserves feedback.
            // InvulnerabilityFlicker skips it only because a dead player's sprite
            // may already be hidden by the death handler — this driver animates a
            // screen-space volume, not the sprite, so the same concern does not
            // apply. Do not "fix" this by adding an IsDead check.
            CancelMotion();

            // Two yoyo loops of half the total duration: the tween climbs to
            // peakWeight on the first pass and returns to its start value (0) on
            // the second. Ending at 0 on natural completion means a finished
            // flash can never strand the volume lit, regardless of when
            // cancellation lands — the same invariant the base class enforces on
            // disable, here preserved by the tween's own shape.
            _motion = LMotion.Create(0f, peakWeight, flashSeconds * 0.5f)
                .WithLoops(2, LoopType.Yoyo)
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
