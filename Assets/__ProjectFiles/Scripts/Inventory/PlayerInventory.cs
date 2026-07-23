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

        // Created lazily rather than in Awake: a view on another GameObject (the UI
        // canvas) can subscribe from its OnEnable before this component's Awake runs,
        // and cross-GameObject Awake/OnEnable order is undefined.
        private ReactiveProperty<KeyDefinition> KeyProp => _currentKey ??= new ReactiveProperty<KeyDefinition>(null);
        private ReactiveProperty<int> FragmentsProp => _mapFragments ??= new ReactiveProperty<int>(0);

        /// <summary>The currently held key, or null. Read-only reactive view.</summary>
        public ReadOnlyReactiveProperty<KeyDefinition> CurrentKeyRx => KeyProp;

        /// <summary>Total Map Fragments collected. Read-only reactive view.</summary>
        public ReadOnlyReactiveProperty<int> MapFragmentsRx => FragmentsProp;

        /// <summary>True while a key is currently held.</summary>
        public bool HasKey => KeyProp.Value != null;

        /// <summary>The currently held key, or null.</summary>
        public KeyDefinition CurrentKey => KeyProp.Value;

        /// <summary>Total Map Fragments collected.</summary>
        public int MapFragments => FragmentsProp.Value;

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

            KeyProp.Value = key;
            KeyPickedUp?.Invoke(key);
            return true;
        }

        /// <summary>True iff the player currently holds the specific required key.</summary>
        public bool HasMatchingKey(KeyDefinition required)
        {
            return required != null && KeyProp.Value == required;
        }

        /// <summary>
        /// Consume the currently held key. No-op (and raises nothing) if none is held.
        /// </summary>
        public void ConsumeKey()
        {
            KeyDefinition previous = KeyProp.Value;
            if (previous == null)
                return;

            KeyProp.Value = null;
            KeyConsumed?.Invoke(previous);
        }

        /// <summary>Grant one Map Fragment; raises <see cref="MapFragmentAdded"/> with the new total.</summary>
        public void AddMapFragment()
        {
            int next = FragmentsProp.Value + 1;
            FragmentsProp.Value = next;
            MapFragmentAdded?.Invoke(next);
        }

        private void OnDestroy()
        {
            _currentKey?.Dispose();
            _mapFragments?.Dispose();
        }
    }
}
