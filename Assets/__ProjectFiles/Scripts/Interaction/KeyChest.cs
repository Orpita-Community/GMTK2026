using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    /// <summary>
    /// A chest locked by a specific key. Holds the player in a 1-second stationary
    /// channel while the correct key is held; wrong or absent key blocks the channel
    /// and raises <see cref="InteractionManager.InteractionBlocked"/> for feedback.
    /// Unlocking consumes the key, grants a Map Fragment, and destroys the chest.
    /// </summary>
    public sealed class KeyChest : InteractableBase
    {
        [SerializeField] private KeyDefinition requiredKey;

        private bool _opened;

        /// <summary>True once this chest has been successfully unlocked.</summary>
        public bool IsOpened => _opened;

        /// <summary>The key asset this chest requires.</summary>
        public KeyDefinition RequiredKey => requiredKey;

        /// <summary>Raised once when the chest is unlocked.</summary>
        public event Action Opened;

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx) =>
            !_opened && ctx.Inventory.HasMatchingKey(requiredKey);

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            ctx.Inventory.ConsumeKey();
            ctx.Inventory.AddMapFragment();
            _opened = true;
            
            Opened?.Invoke();

            // Destroy the chest's GameObject immediately after unlocking
            Destroy(gameObject);
        }

        /// <inheritdoc/>
        public override void SetHighlighted(bool highlighted)
        {
            if (_opened)
                return;

            base.SetHighlighted(highlighted);
        }
    }
}