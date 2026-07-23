using System;
using UnityEngine;

namespace Orpita.Items
{
    /// <summary>Kinds of reward a loot roll can yield.</summary>
    public enum LootKind
    {
        Nothing,
        Key,
        Candle
    }

    /// <summary>
    /// Immutable result of a loot roll. Use the static factories instead of the
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

    /// <summary>One weighted option in a <see cref="LootTable"/>.</summary>
    [Serializable]
    public struct LootEntry
    {
        public LootKind kind;

        [Min(0f)] public float weight;

        /// <summary>Key awarded. Used only when <see cref="kind"/> == <see cref="LootKind.Key"/>.</summary>
        public KeyDefinition key;

        /// <summary>Fuel seconds awarded. Used only when <see cref="kind"/> == <see cref="LootKind.Candle"/> (e.g. 15).</summary>
        public float candleSeconds;
    }

    /// <summary>
    /// Weighted random loot table. Rolls a single entry proportional to its
    /// weight (via <see cref="Random.value"/>) and converts it to the matching
    /// <see cref="LootReward"/>. An empty table, or one whose weights all sum to
    /// zero, yields <see cref="LootReward.Nothing"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Orpita/Loot Table", fileName = "LootTable")]
    public sealed class LootTable : ScriptableObject
    {
        [SerializeField] private LootEntry[] entries;

        /// <summary>
        /// Roll one entry proportional to its weight. Returns
        /// <see cref="LootReward.Nothing"/> if the table is empty or every weight
        /// is zero/negative.
        /// </summary>
        public LootReward Roll()
        {
            if (entries == null || entries.Length == 0)
                return LootReward.Nothing;

            float total = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                float w = entries[i].weight;
                if (w > 0f)
                    total += w;
            }

            if (total <= 0f)
                return LootReward.Nothing;

            float roll = UnityEngine.Random.value * total;
            float accumulated = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                float w = entries[i].weight;
                if (w <= 0f)
                    continue;

                accumulated += w;
                if (roll <= accumulated)
                    return ToReward(entries[i]);
            }

            // Floating-point tail guard: if rounding kept us above the last entry,
            // fall back to the last positive entry.
            for (int i = entries.Length - 1; i >= 0; i--)
            {
                if (entries[i].weight > 0f)
                    return ToReward(entries[i]);
            }

            return LootReward.Nothing;
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
