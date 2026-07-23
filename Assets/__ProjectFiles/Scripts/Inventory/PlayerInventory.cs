using System;
using R3;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Inventory
{
    /// <summary>
    /// Single-key player inventory plus a Map Fragment counter. Holds at most one
    /// key: a second pickup is PREVENTED (not swapped or dropped) and raises
    /// <see cref="KeyPickupBlocked"/> for feedback. Key state and the fragment
    /// count are exposed as reactive streams; mutations raise plain events.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        private ReactiveProperty<KeyDefinition> _currentKey;
        private ReactiveProperty<int> _mapFragments;

        private void Awake()
        {
            _currentKey = new ReactiveProperty<KeyDefinition>(null);
            _mapFragments = new ReactiveProperty<int>(0);
        }

        /// <summary>The currently held key, or null. Read-only reactive view.</summary>
        public ReadOnlyReactiveProperty<KeyDefinition> CurrentKeyRx => _currentKey;

        /// <summary>Total Map Fragments collected. Read-only reactive view.</summary>
        public ReadOnlyReactiveProperty<int> MapFragmentsRx => _mapFragments;

        /// <summary>True while a key is currently held.</summary>
        public bool HasKey => _currentKey.Value != null;

        /// <summary>The currently held key, or null.</summary>
        public KeyDefinition CurrentKey => _currentKey.Value;

        /// <summary>Total Map Fragments collected.</summary>
        public int MapFragments => _mapFragments.Value;

        /// <summary>Raised when a key is successfully picked up.</summary>
        public event Action<KeyDefinition> KeyPickedUp;

        /// <summary>Raised when a key pickup is blocked because one is already held.</summary>
        public event Action<KeyDefinition> KeyPickupBlocked;

        /// <summary>Raised when the held key is consumed (e.g. unlocking a chest).</summary>
        public event Action<KeyDefinition> KeyConsumed;

        /// <summary>Raised when a Map Fragment is added; payload is the new total.</summary>
        public event Action<int> MapFragmentAdded;

        /// <summary>
        /// Attempt to pick up a key. If one is already held, the pickup is
        /// prevented, <see cref="KeyPickupBlocked"/> is raised, and this returns false.
        /// </summary>
        public bool TryPickUpKey(KeyDefinition key)
        {
            if (HasKey)
            {
                KeyPickupBlocked?.Invoke(key);
                return false;
            }

            _currentKey.Value = key;
            KeyPickedUp?.Invoke(key);
            return true;
        }

        /// <summary>True iff the player currently holds the specific required key.</summary>
        public bool HasMatchingKey(KeyDefinition required)
        {
            return required != null && _currentKey.Value == required;
        }

        /// <summary>
        /// Consume the currently held key. No-op (and raises nothing) if none is held.
        /// </summary>
        public void ConsumeKey()
        {
            KeyDefinition previous = _currentKey.Value;
            if (previous == null)
                return;

            _currentKey.Value = null;
            KeyConsumed?.Invoke(previous);
        }

        /// <summary>Grant one Map Fragment; raises <see cref="MapFragmentAdded"/> with the new total.</summary>
        public void AddMapFragment()
        {
            int next = _mapFragments.Value + 1;
            _mapFragments.Value = next;
            MapFragmentAdded?.Invoke(next);
        }

        private void OnDestroy()
        {
            _currentKey?.Dispose();
            _mapFragments?.Dispose();
        }
    }
}
