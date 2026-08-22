using System;
using UnityEngine;

namespace Orpita.Items
{
    /// <summary>Kinds of reward a loot yield can have.</summary>
    public enum LootKind
    {
        Nothing,
        Key,
        Candle
    }

    /// <summary>
    /// Immutable result of a loot yield. Use the static factories instead of the
    /// constructor so callers cannot assemble invalid combinations.
    /// </summary>
    public readonly struct LootReward
    {
        /// <summary>Which kind of reward this is.</summary>
        public readonly LootKind Kind;

        /// <summary>The key awarded. Non-null only when <see cref="Kind"/> == <see cref="LootKind.Key"/>.</summary>
        public readonly KeyDefinition Key;

        /// <summary>Fuel seconds awarded. Used only when <see cref="Kind"/> == <see cref="LootKind.Candle"/>.</summary>
        public readonly float CandleSeconds;

        private LootReward(LootKind kind, KeyDefinition key, float candleSeconds)
        {
            Kind = kind;
            Key = key;
            CandleSeconds = candleSeconds;
        }

        /// <summary>No reward.</summary>
        public static LootReward Nothing => new LootReward(LootKind.Nothing, null, 0f);

        /// <summary>A key reward.</summary>
        public static LootReward OfKey(KeyDefinition key) => new LootReward(LootKind.Key, key, 0f);

        /// <summary>A candle fuel reward.</summary>
        public static LootReward OfCandle(float seconds) => new LootReward(LootKind.Candle, null, seconds);
    }

    /// <summary>One sequential option in a <see cref="LootTable"/>.</summary>
    [Serializable]
    public struct LootEntry
    {
        public LootKind kind;

        /// <summary>Key awarded. Used only when <see cref="kind"/> == <see cref="LootKind.Key"/>.</summary>
        public KeyDefinition key;

        /// <summary>Fuel seconds awarded. Used only when <see cref="kind"/> == <see cref="LootKind.Candle"/> (e.g. 15).</summary>
        public float candleSeconds;
    }

    /// <summary>
    /// Sequential loot table. Yields a specific entry based on the interaction index.
    /// </summary>
    [CreateAssetMenu(menuName = "Orpita/Loot Table", fileName = "LootTable")]
    public sealed class LootTable : ScriptableObject
    {
        [SerializeField] private LootEntry[] entries;

        /// <summary>
        /// Retrieves the specific entry matching the interaction index.
        /// </summary>
        public LootReward GetRewardForInteraction(int interactionIndex)
        {
            if (entries == null || entries.Length == 0)
                return LootReward.Nothing;

            // Ensure we don't go out of bounds if the player can search more times than there are entries
            if (interactionIndex >= entries.Length)
                return LootReward.Nothing;

            return ToReward(entries[interactionIndex]);
        }

        private static LootReward ToReward(LootEntry entry)
        {
            switch (entry.kind)
            {
                case LootKind.Key: return LootReward.OfKey(entry.key);
                case LootKind.Candle: return LootReward.OfCandle(entry.candleSeconds);
                default: return LootReward.Nothing;
            }
        }
    }
}