using System;

namespace Orpita.Player
{
    /// <summary>
    /// Abstraction over the interaction input source. Drives the hold-to-interact
    /// channel from press/release events so the manager can distinguish a tap from
    /// a sustained hold (and own the channel timing) without polling the Input
    /// System directly.
    /// </summary>
    public interface IInteractionInput
    {
        /// <summary>Raised on the frame the interact button is pressed.</summary>
        event Action InteractStarted;

        /// <summary>Raised on the frame the interact button is released.</summary>
        event Action InteractCanceled;

        /// <summary>True while the interact button is held (between Started and Canceled).</summary>
        bool InteractHeld { get; }
    }
}
