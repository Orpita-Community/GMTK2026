using UnityEngine;

namespace Orpita.Interaction
{
    /// <summary>
    /// Anything the player can interact with via the stationary channel
    /// (hold-E) system. Implementations typically derive from
    /// <see cref="InteractableBase"/> for shared highlight behaviour, but this
    /// interface is the contract the <see cref="InteractionManager"/> and
    /// <see cref="InteractionDetector"/> work against.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Transform used for proximity/distance checks.</summary>
        Transform Transform { get; }

        /// <summary>Stationary channel duration in seconds; &lt;= 0 means instant.</summary>
        float InteractionDuration { get; }

        /// <summary>Prompt shown to the player (e.g. "Search", "Refill", "Unlock").</summary>
        string Prompt { get; }

        /// <summary>Is the interaction currently valid given the player's state?</summary>
        bool CanInteract(InteractionContext ctx);

        /// <summary>Invoked once when the channel completes successfully.</summary>
        void OnInteract(InteractionContext ctx);

        /// <summary>Visualize (or stop visualizing) as the current proximity target.</summary>
        void SetHighlighted(bool highlighted);
    }
}
