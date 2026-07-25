using R3;
using UnityEngine;
using UnityEngine.Rendering;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Base for components that express a gameplay state as the blend weight of a
    /// post-processing <see cref="Volume"/>. The look itself stays entirely in the
    /// volume profile — subclasses only move one float — so artists can retune a
    /// grade without touching code.
    ///
    /// <para>
    /// The class exists to hold one invariant: a driven volume is at weight 0
    /// whenever its driver is not running. A stuck volume (a damage flash left at
    /// full weight after the player is destroyed, say) is invisible in the
    /// inspector but ruins every frame after it, so the reset is enforced here
    /// rather than left to each subclass to remember.
    /// </para>
    ///
    /// Subclasses implement <see cref="SubscribeDriver"/> to attach their state
    /// source; anything added to the supplied bag is disposed automatically.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Volume))]
    public abstract class VolumeBlendDriver : MonoBehaviour
    {
        private Volume _volume;
        private CompositeDisposable _subscriptions;

        /// <summary>
        /// Current blend weight of the driven volume, always in [0, 1]. Reads 0
        /// before the driver is enabled.
        /// </summary>
        protected float Weight
        {
            get => _volume != null ? _volume.weight : 0f;
            set
            {
                if (_volume != null)
                    _volume.weight = Mathf.Clamp01(value);
            }
        }

        private void OnEnable()
        {
            // Resolved here rather than in Awake: OnEnable can run before Awake
            // after a domain reload. Mirrors the PlayerInputReader guard.
            _volume = GetComponent<Volume>();
            Weight = 0f;

            _subscriptions = new CompositeDisposable();

            if (!TryResolveDependencies())
            {
                enabled = false;
                return;
            }

            SubscribeDriver(_subscriptions);
        }

        private void OnDisable()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            OnDriverDisable();

            // Last write wins: OnDriverDisable may cancel a tween that would
            // otherwise have left the volume mid-blend.
            Weight = 0f;
        }

        /// <summary>
        /// Validate serialized dependencies before subscribing. Return false to
        /// disable the component (log the reason first). Defaults to true for
        /// drivers with no required references.
        /// </summary>
        protected virtual bool TryResolveDependencies() => true;

        /// <summary>
        /// Attach to the gameplay state that drives this volume. Add every
        /// subscription to <paramref name="subscriptions"/>; the bag is disposed
        /// in <c>OnDisable</c>.
        /// </summary>
        protected abstract void SubscribeDriver(CompositeDisposable subscriptions);

        /// <summary>
        /// Release non-subscription resources — in practice, cancelling an
        /// in-flight tween. The weight reset is handled by the base class.
        /// </summary>
        protected virtual void OnDriverDisable() { }

        /// <summary>
        /// Shared missing-reference diagnostic, so every driver reports a wiring
        /// mistake the same way.
        /// </summary>
        protected bool RequireReference(Object reference, string referenceName)
        {
            if (reference != null)
                return true;

            Debug.LogError($"{GetType().Name} on '{name}' requires a {referenceName} reference.", this);
            return false;
        }
    }
}
