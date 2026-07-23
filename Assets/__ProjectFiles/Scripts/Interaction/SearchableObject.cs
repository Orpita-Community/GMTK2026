using System;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    /// <summary>
    /// Furniture the player can search for loot. When <c>singleUse</c> is set,
    /// it can only be searched once. Loot comes from a <see cref="LootTable"/>:
    /// a Key attempts a pickup (blocked if the player already holds a key — the
    /// single-key constraint is enforced inside the inventory), a Candle adds
    /// fuel, Nothing yields no reward.
    /// </summary>
    public sealed class SearchableObject : InteractableBase
    {
        [SerializeField] private LootTable lootTable;

        [Tooltip("If true, this object can only be searched once.")]
        [SerializeField] private bool singleUse = true;

        private bool _searched;

        /// <summary>Raised with the rolled reward when a search completes.</summary>
        public event Action<LootReward> Searched;

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx) => !(singleUse && _searched);

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            LootReward reward = lootTable != null ? lootTable.Roll() : LootReward.Nothing;

            switch (reward.Kind)
            {
                case LootKind.Key:
                    // TryPickUpKey returns false (and fires its own blocked
                    // feedback event) when the player already holds a key, so the
                    // single-key constraint is enforced here rather than by dropping
                    // or swapping.
                    ctx.Inventory.TryPickUpKey(reward.Key);
                    break;
                case LootKind.Candle:
                    ctx.Candle?.AddSeconds(reward.CandleSeconds);
                    break;
                case LootKind.Nothing:
                    break;
            }

            _searched = true;
            Searched?.Invoke(reward);
        }
    }
}
