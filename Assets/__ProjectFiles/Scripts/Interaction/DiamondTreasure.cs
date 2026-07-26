using System;
using System.Collections;
using UnityEngine;
using Orpita.Interaction;
using Orpita.Inventory;
using Orpita.Player;
using Orpita.Audio;


namespace Orpita.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DiamondTreasure : InteractableBase
    {
        [Header("Dependencies")]
        [Tooltip("Link the PlayerInventory from the scene here so the chest can check it on approach.")]
        [SerializeField] private PlayerInventory playerInventory;

        [Header("Unlock Condition")]
        [Tooltip("The number of map fragments required to open this treasure.")]
        [SerializeField] private int requiredFragments = 3;

        [Header("Visuals")]
        [Tooltip("The sprite to display when the treasure is opened.")]
        [SerializeField] private Sprite openSprite;

        [Header("Spawning Config")]
        [Tooltip("The physical Diamond prefab to spawn.")]
        [SerializeField] private GameObject diamondPrefab;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private float dropYOffset = -1.0f;

        [Header("UI Feedback")]
        [Tooltip("The Canvas Group on the warning UI GameObject.")]
        [SerializeField] private CanvasGroup warningCanvasGroup;
        [Tooltip("How long the fade in/out takes.")]
        [SerializeField] private float fadeDuration = 0.5f;
        [Tooltip("How long the warning stays fully visible on screen.")]
        [SerializeField] private float warningHoldTime = 1.5f;

        private bool _opened;
        private SpriteRenderer _spriteRenderer;
        private Coroutine _warningCoroutine;

        public event Action Opened;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Ensure the warning is invisible at the start
            if (warningCanvasGroup != null)
            {
                warningCanvasGroup.alpha = 0f;
                warningCanvasGroup.gameObject.SetActive(false);
            }
        }

        public override bool CanInteract(InteractionContext ctx)
        {
            // The 'E' prompt is strictly blocked and invisible until they have all fragments.
            return !_opened && ctx.Inventory.MapFragmentCount >= requiredFragments;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            // Safety double-lock
            if (ctx.Inventory.MapFragmentCount < requiredFragments) return;

            _opened = true;

            AudioManager.Instance.PlaySFX("FinalTreasure");

            // 1. Swap the sprite to the open state
            if (openSprite != null)
            {
                _spriteRenderer.sprite = openSprite;
            }

            // 2. Spawn the diamond and animate it popping out
            if (diamondPrefab != null)
            {
                GameObject spawnedDiamond = Instantiate(diamondPrefab, transform.position + spawnOffset, Quaternion.identity);
                StartCoroutine(AnimatePopOut(spawnedDiamond.transform));
            }

            Opened?.Invoke();
        }

        public override void SetHighlighted(bool highlighted)
        {
            if (_opened) return;
            base.SetHighlighted(highlighted);
        }

        // --- NEW: Proximity Detection for the Warning Text ---
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_opened || playerInventory == null) return;

            // Verify it is the player touching it by looking for their motor[cite: 4]
            PlayerMotor player = other.GetComponentInParent<PlayerMotor>();
            
            if (player != null)
            {
                // If they walked up but don't have enough fragments, flash the warning
                if (playerInventory.MapFragmentCount < requiredFragments)
                {
                    if (warningCanvasGroup != null)
                    {
                        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
                        _warningCoroutine = StartCoroutine(FadeWarningRoutine());
                    }
                }
            }
        }

        private IEnumerator FadeWarningRoutine()
        {
            warningCanvasGroup.gameObject.SetActive(true);

            // 1. Fade In
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                warningCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            warningCanvasGroup.alpha = 1f;

            // 2. Hold on screen
            yield return new WaitForSeconds(warningHoldTime);

            // 3. Fade Out
            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                warningCanvasGroup.alpha = 1f - (timer / fadeDuration);
                yield return null;
            }
            
            // Cleanup
            warningCanvasGroup.alpha = 0f;
            warningCanvasGroup.gameObject.SetActive(false);
        }

        /// <summary>
        /// Animates the diamond jumping out of the chest and landing on the floor.
        /// </summary>
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