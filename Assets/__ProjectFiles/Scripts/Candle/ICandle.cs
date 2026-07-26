namespace Orpita.Candle
{
    /// <summary>
    /// Abstraction over the candle fuel mechanic. Lets interactables (refill
    /// stations, candle loot) modify candle time without coupling to a concrete
    /// implementation. The real candle system may replace the placeholder
    /// implementer; this interface is the stable seam between systems.
    /// </summary>
    public interface ICandle
    {
        /// <summary>Seconds of fuel remaining, in [0, <see cref="MaxSeconds"/>].</summary>
        float SecondsRemaining { get; }

        /// <summary>Maximum fuel capacity in seconds.</summary>
        float MaxSeconds { get; }

        /// <summary>Refuel to full (<see cref="MaxSeconds"/>).</summary>
        void Refill();

        /// <summary>Add seconds, clamped to [0, <see cref="MaxSeconds"/>].</summary>
        void AddSeconds(float seconds);

        /// <summary>Spend seconds instantly (e.g. Boost Flame), clamped to [0, <see cref="MaxSeconds"/>].</summary>
        void ConsumeTime(float seconds);
    }
}
