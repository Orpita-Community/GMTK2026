using UnityEngine;
using Orpita.Inventory;
using Orpita.Candle;

namespace Orpita.Interaction
{
    /// <summary>
    /// Bundle of everything an interactable may act on. Constructed once by the
    /// <see cref="InteractionManager"/> and handed to each interactable, so
    /// interactables stay decoupled from player internals (no service locators,
    /// no FindObjectOfType). The candle may be null when the player has no candle.
    /// </summary>
    public sealed class InteractionContext
    {
        /// <summary>The player root GameObject.</summary>
        public GameObject Player { get; }

        /// <summary>The player's single-key inventory. Never null.</summary>
        public PlayerInventory Inventory { get; }

        /// <summary>The player's candle, or null if none is equipped.</summary>
        public ICandle Candle { get; }

        public InteractionContext(GameObject player, PlayerInventory inventory, ICandle candle)
        {
            Player = player;
            Inventory = inventory;
            Candle = candle;
        }
    }
}
