using System;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    /// <summary>
    /// A chest locked by a specific key. Interactable only while closed and
    /// while the player holds the matching key. Unlocking consumes the key and
    /// grants a Map Fragment. A missing key keeps <see cref="CanInteract"/> false,
    /// so the manager reports the interaction as blocked (feedback) rather than
    /// starting a doomed channel.
    /// </summary>
    public sealed class KeyChest : InteractableBase
    {
        [SerializeField] private KeyDefinition requiredKey;

        private bool _opened;

        /// <summary>Raised once when the chest is unlocked.</summary>
        public event Action Opened;

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx)
        {
            return !_opened && ctx.Inventory.HasMatchingKey(requiredKey);
        }

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            ctx.Inventory.ConsumeKey();
            ctx.Inventory.AddMapFragment();
            _opened = true;
            Opened?.Invoke();
        }
    }
}
