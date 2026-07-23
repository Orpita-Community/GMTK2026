namespace Orpita.Player
{
    /// <summary>
    /// Exposes the player's damage state so the interaction system can suppress
    /// interaction during invulnerability (i-frames) without depending on a
    /// concrete health implementation. A future health system implements this;
    /// consumers query it null-safely (a null reference is treated as never
    /// invulnerable).
    /// </summary>
    public interface IDamageState
    {
        /// <summary>True while the player is in damage invulnerability frames.</summary>
        bool IsInvulnerable { get; }
    }
}
