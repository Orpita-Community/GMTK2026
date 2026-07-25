using System;
using System.Collections;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SearchableObject : InteractableBase
    {
        [SerializeField] private LootTable lootTable;
        
        [Tooltip("How many times the player can search this object.")]
        [SerializeField] private int maxInteractions = 1;

        [Header("Spawning Config")]
        [SerializeField] private KeyPickup keyPickupPrefab;
        [SerializeField] private WaxPickup waxPickupPrefab;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private float dropYOffset = -1.0f;

        [Header("Visuals")]
        [Tooltip("Sprites change in order of interactions. Search 1 = Sprite 0, Search 2 = Sprite 1, etc.")]
        [SerializeField] private Sprite[] searchedSprites;

        private int _interactionCount = 0; // Tracks how many times it has been searched
        private SpriteRenderer _spriteRenderer;

        public event Action<LootReward> Searched;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Only allow interaction if we haven't hit the max limit
        public override bool CanInteract(InteractionContext ctx) => _interactionCount < maxInteractions;

        public override void OnInteract(InteractionContext ctx)
        {
            LootReward reward = lootTable != null ? lootTable.Roll() : LootReward.Nothing;

            switch (reward.Kind)
            {
                case LootKind.Key:
                    if (keyPickupPrefab != null)
                    {
                        KeyPickup spawnedKey = Instantiate(keyPickupPrefab, transform.position + spawnOffset, Quaternion.identity);
                        spawnedKey.Initialize(reward.Key);
                        StartCoroutine(AnimatePopOut(spawnedKey.transform));
                    }
                    break;

                case LootKind.Candle:
                    if (waxPickupPrefab != null)
                    {
                        WaxPickup spawnedWax = Instantiate(waxPickupPrefab, transform.position + spawnOffset, Quaternion.identity);
                        spawnedWax.Initialize(reward.CandleSeconds);
                        StartCoroutine(AnimatePopOut(spawnedWax.transform));
                    }
                    break;

                case LootKind.Nothing:
                    break;
            }

            // Increase the search counter
            _interactionCount++;

            // --- SPRITE SWAP LOGIC (Sequential) ---
            if (searchedSprites != null && searchedSprites.Length > 0)
            {
                // We subtract 1 from interactionCount so the 1st search maps to index 0.
                // Mathf.Clamp acts as a safety net: if maxInteractions is higher than your sprite list, 
                // it just stays on the last available sprite instead of throwing an error.
                int spriteIndex = Mathf.Clamp(_interactionCount - 1, 0, searchedSprites.Length - 1);
                _spriteRenderer.sprite = searchedSprites[spriteIndex];
            }

            Searched?.Invoke(reward);
        }

        private IEnumerator AnimatePopOut(Transform itemTransform)
        {
            Vector3 startPos = itemTransform.position;
            
            float randomDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float randomX = UnityEngine.Random.Range(1.5f, 2.5f) * randomDirection;
            
            Vector3 endPos = startPos + new Vector3(randomX, dropYOffset, 0f);
            
            float duration = 0.5f; 
            float timer = 0f;

            while (timer < duration)
            {
                if (itemTransform == null) yield break;

                timer += Time.deltaTime;
                float progress = timer / duration;

                float currentX = Mathf.Lerp(startPos.x, endPos.x, progress);

                float jumpHeight = 1.5f;
                float arc = 4f * jumpHeight * (progress - progress * progress);
                float currentY = Mathf.Lerp(startPos.y, endPos.y, progress) + arc;

                itemTransform.position = new Vector3(currentX, currentY, 0f);
                
                yield return null;
            }

            if (itemTransform != null)
            {
                itemTransform.position = endPos;
            }
        }
    }
}