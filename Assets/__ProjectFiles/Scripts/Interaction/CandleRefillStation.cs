using System;
using Orpita.Candle;

namespace Orpita.Interaction
{
    /// <summary>
    /// Restores the player's candle to full. Only interactable when the player
    /// carries a candle that isn't already full. Reusable.
    /// </summary>
    public sealed class CandleRefillStation : InteractableBase
    {
        /// <summary>Raised when a refill completes.</summary>
        public event Action Refilled;

        // Base default is 1.5s; the candle spec calls for a 1s stationary refill.
        private void Reset() => interactionDuration = 1f;

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx)
        {
            ICandle candle = ctx.Candle;
            return candle != null && candle.SecondsRemaining < candle.MaxSeconds;
        }

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            ctx.Candle?.Refill();
            Refilled?.Invoke();
        }
    }
}
