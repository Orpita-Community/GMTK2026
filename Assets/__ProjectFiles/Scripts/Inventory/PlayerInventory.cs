using System;
using R3;
using UnityEngine;
using Orpita.Items;
using Orpita.Audio;

namespace Orpita.Inventory
{
    /// <summary>
    /// Single-key player inventory plus a Map Fragment counter[cite: 8]. Holds at most one
    /// key: a second pickup is PREVENTED (not swapped or dropped) and raises
    /// <see cref="KeyPickupBlocked"/> for feedback[cite: 8]. Key state and the fragment
    /// count are exposed as reactive streams; mutations raise plain events[cite: 8].
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        private ReactiveProperty<KeyDefinition> _currentKey;
        private ReactiveProperty<int> _mapFragments;

        // Created lazily rather than in Awake: a view on another GameObject (the UI
        // canvas) can subscribe from its OnEnable before this component's Awake runs,
        // and cross-GameObject Awake/OnEnable order is undefined[cite: 8].
        private ReactiveProperty<KeyDefinition> KeyProp => _currentKey ??= new ReactiveProperty<KeyDefinition>(null);
        private ReactiveProperty<int> FragmentsProp => _mapFragments ??= new ReactiveProperty<int>(0);

        /// <summary>The currently held key, or null. Read-only reactive view[cite: 8].</summary>
        public ReadOnlyReactiveProperty<KeyDefinition> CurrentKeyRx => KeyProp;

        /// <summary>Total Map Fragments collected. Read-only reactive view[cite: 8].</summary>
        public ReadOnlyReactiveProperty<int> MapFragmentsRx => FragmentsProp;

        /// <summary>True while a key is currently held[cite: 8].</summary>
        public bool HasKey => KeyProp.Value != null;

        /// <summary>The currently held key, or null[cite: 8].</summary>
        public KeyDefinition CurrentKey => KeyProp.Value;

        /// <summary>Total Map Fragments collected[cite: 8].</summary>
        public int MapFragments => FragmentsProp.Value;

        /// <summary>Total Map Fragments collected (Added to support DiamondTreasure.cs).</summary>
        public int MapFragmentCount => FragmentsProp.Value;

        /// <summary>Raised when a key is successfully picked up[cite: 8].</summary>
        public event Action<KeyDefinition> KeyPickedUp;

        /// <summary>Raised when a key pickup is blocked because one is already held[cite: 8].</summary>
        public event Action<KeyDefinition> KeyPickupBlocked;

        /// <summary>Raised when the held key is consumed (e.g. unlocking a chest)[cite: 8].</summary>
        public event Action<KeyDefinition> KeyConsumed;

        /// <summary>Raised when a Map Fragment is added; payload is the new total[cite: 8].</summary>
        public event Action<int> MapFragmentAdded;

        /// <summary>
        /// Attempt to pick up a key. If one is already held, the pickup is
        /// prevented, <see cref="KeyPickupBlocked"/> is raised, and this returns false[cite: 8].
        /// </summary>
        public bool TryPickUpKey(KeyDefinition key)
        {
            if (HasKey)
            {
                KeyPickupBlocked?.Invoke(key);
                return false;
            }

            KeyProp.Value = key;
            
            // --- ADD THIS EXACT LINE ---
            AudioManager.Instance.PlaySFX("KeyPickup");
            
            KeyPickedUp?.Invoke(key);
            return true;
        }

        /// <summary>True iff the player currently holds the specific required key[cite: 8].</summary>
        public bool HasMatchingKey(KeyDefinition required)
        {
            return required != null && KeyProp.Value == required;
        }

        /// <summary>
        /// Consume the currently held key. No-op (and raises nothing) if none is held[cite: 8].
        /// </summary>
        public void ConsumeKey()
        {
            KeyDefinition previous = KeyProp.Value;
            if (previous == null)
                return;

            KeyProp.Value = null;
            KeyConsumed?.Invoke(previous);
        }

        /// <summary>Grant one Map Fragment; raises <see cref="MapFragmentAdded"/> with the new total[cite: 8].</summary>
        public void AddMapFragment()
        {
            int next = FragmentsProp.Value + 1;
            FragmentsProp.Value = next;

            // Audio Logic: Check if it's the final piece
            if (next == 3)
            {
                AudioManager.Instance.PlaySFX("MapCompleted");
            }
            else
            {
                AudioManager.Instance.PlaySFX("MapPickup");
            }

            MapFragmentAdded?.Invoke(next);
        }

        private void OnDestroy()
        {
            _currentKey?.Dispose();
            _mapFragments?.Dispose();
        }
    }
}