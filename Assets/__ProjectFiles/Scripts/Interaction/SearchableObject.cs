using System;
using System.Collections;
using UnityEngine;
using Orpita.Items;
using Orpita.Audio;

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

        private int _interactionCount = 0; 
        private SpriteRenderer _spriteRenderer;

        public event Action<LootReward> Searched;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdatePromptText(); // Set the initial text
        }

        // Only allow interaction if we haven't hit the max limit[cite: 10]
        public override bool CanInteract(InteractionContext ctx) => _interactionCount < maxInteractions;

        public override void OnInteract(InteractionContext ctx)
        {
            if (_interactionCount >= maxInteractions) return; // Safety check

            LootReward reward = lootTable != null ? lootTable.GetRewardForInteraction(_interactionCount) : LootReward.Nothing;
            AudioManager.Instance.PlaySFX("DrawerOpen");

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

            // Sprite Swap Logic
            if (searchedSprites != null && searchedSprites.Length > 0)
            {
                int spriteIndex = Mathf.Clamp(_interactionCount - 1, 0, searchedSprites.Length - 1);
                _spriteRenderer.sprite = searchedSprites[spriteIndex];
            }

            Searched?.Invoke(reward);
            
            Collider2D col = GetComponent<Collider2D>();

            // --- THE DISAPPEARING ACT ---
            if (_interactionCount >= maxInteractions)
            {
                // If we are out of interactions, disable the collider!
                // This instantly removes the object from the player's detector, hiding the UI text entirely.
                if (col != null) col.enabled = false;
                
                // Disable the script so it permanently stops highlighting as well
                this.enabled = false;
            }
            else
            {
                // If there are more interactions left, update the text to "Interact again"
                UpdatePromptText();

                // Hack to instantly refresh the detector's UI while standing still.
                if (col != null)
                {
                    col.enabled = false;
                    col.enabled = true;
                }
            }
        }

        /// <summary>
        /// Updates the underlying inherited 'prompt' string based on our progress.[cite: 10]
        /// </summary>
        private void UpdatePromptText()
        {
            if (_interactionCount == 0)
            {
                prompt = "Interact";
            }
            else 
            {
                // We no longer need the "Already opened" text since the prompt vanishes completely!
                prompt = "Interact again"; 
            }
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