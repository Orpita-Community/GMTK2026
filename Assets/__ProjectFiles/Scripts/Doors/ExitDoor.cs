using System.Collections;
using UnityEngine;
using Orpita.Interaction;

namespace Orpita.Core
{
    public class ExitDoor : InteractableBase
    {
        [Header("Cinematic Settings")]
        [Tooltip("The black screen Canvas Group to fade in.")]
        [SerializeField] private CanvasGroup fadeOverlay;
        
        [Tooltip("The Win UI panel to show after the fade.")]
        [SerializeField] private GameObject winUI;
        
        [Tooltip("How long the screen takes to fade to black.")]
        [SerializeField] private float fadeDuration = 1.5f;

        private bool _isEscaping;

        public override bool CanInteract(InteractionContext ctx)
        {
            // Only interactable if the player is alive and hasn't already triggered the door
            return !_isEscaping && ctx.Candle != null && ctx.Candle.SecondsRemaining > 0f;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            _isEscaping = true;
            
            // 1. FREEZE TIME IMMEDIATELY
            Time.timeScale = 0f;
            
            StartCoroutine(EscapeSequence());
        }

        private IEnumerator EscapeSequence()
        {
            // 2. Fade the screen to black smoothly using UNSCALED time
            if (fadeOverlay != null)
            {
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    // Because timeScale is 0, normal Time.deltaTime is 0. 
                    // We MUST use unscaledDeltaTime so the fade animation still plays!
                    timer += Time.unscaledDeltaTime;
                    fadeOverlay.alpha = timer / fadeDuration;
                    yield return null;
                }
                fadeOverlay.alpha = 1f;
            }

            // 3. Activate the Win UI once the screen is fully black[cite: 10]
            if (winUI != null)
            {
                winUI.SetActive(true);
            }

            // 4. Deactivate the door so it can't be interacted with again[cite: 10]
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // SAFETY NET: If the player clicks "Restart" or "Main Menu" on your Win UI,
            // we must ensure time goes back to normal for the next scene.
            Time.timeScale = 1f;
        }
    }
}