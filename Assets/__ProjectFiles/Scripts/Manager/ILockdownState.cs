using R3;

namespace Orpita.Core
{
    /// <summary>
    /// Read-only observable view of Lockdown Mode. Systems that need to react to
    /// the endgame starting (e.g. a post-processing colour-grade shift) subscribe
    /// to <see cref="IsActiveRx"/> without coupling to whatever side effects
    /// lockdown performs on the scene.
    /// </summary>
    public interface ILockdownState
    {
        /// <summary>True once Lockdown Mode has started. Stays true thereafter.</summary>
        bool IsActive { get; }

        /// <summary>
        /// Reactive stream of lockdown state. Safe to subscribe before the
        /// implementing component's <c>Awake</c> runs (the implementation uses
        /// the project's lazy-init pattern).
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsActiveRx { get; }
    }
}
