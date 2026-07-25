using R3;
using UnityEngine;
using Orpita.Items;
using Orpita.Interaction;

namespace Orpita.Core
{
    /// <summary>
    /// Owns Lockdown Mode: the reactive state flag plus the three one-shot
    /// endgame effects (reveal exit door, remove wax pickups, deactivate candle
    /// refill stations). Activation is idempotent and latches forever.
    /// Exposed reactively via <see cref="ILockdownState"/> so unrelated systems
    /// (post-processing, audio, UI) can react without coupling to the effects.
    /// </summary>
    public sealed class LockdownManager : MonoBehaviour, ILockdownState
    {
        [Header("Lockdown Effects")]
        [Tooltip("The Exit Door object in Hallway 1. Hidden until lockdown activates.")]
        [SerializeField] private GameObject exitDoorObject;

        private ReactiveProperty<bool> _isActive;

        // Lazily created rather than in Awake: a subscriber on another GameObject
        // (e.g. a post-processing volume's OnEnable) may subscribe before this
        // component's Awake runs, and cross-GameObject Awake/OnEnable order is
        // undefined. Mirrors the CandleFuel lazy-init pattern.
        private ReactiveProperty<bool> State => _isActive ??= new ReactiveProperty<bool>(false);

        /// <inheritdoc/>
        public bool IsActive => State.Value;

        /// <inheritdoc/>
        public ReadOnlyReactiveProperty<bool> IsActiveRx => State;

        private void OnEnable()
        {
            // Door must start invisible/non-interactable; Activate() reveals it.
            // Guarded by the latch: re-enabling this component after lockdown has
            // started must not hide the exit the player is currently running for.
            if (!State.Value && exitDoorObject != null) exitDoorObject.SetActive(false);
        }

        /// <summary>
        /// Starts lockdown. Idempotent: the first call flips the state and runs
        /// the effects; subsequent calls are silent no-ops.
        /// </summary>
        public void Activate()
        {
            if (State.Value)
                return;

            Debug.Log("LOCKDOWN MODE INITIATED!");

            // State flips FIRST so subscribers (colour grade, audio, UI) observe
            // the transition before any scene object they may reference vanishes.
            State.Value = true;

            if (exitDoorObject != null) exitDoorObject.SetActive(true);

            WaxPickup[] waxPickups = FindObjectsByType<WaxPickup>(FindObjectsSortMode.None);
            foreach (WaxPickup wax in waxPickups)
            {
                Destroy(wax.gameObject);
            }

            CandleRefillStation[] stations = FindObjectsByType<CandleRefillStation>(FindObjectsSortMode.None);
            foreach (CandleRefillStation station in stations)
            {
                // Disabling the GameObject removes it from the InteractionDetector entirely
                station.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _isActive?.Dispose();
        }
    }
}
