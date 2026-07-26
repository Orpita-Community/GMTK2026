using System;
using System.Threading;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Orpita.Core;

namespace Orpita.UI
{
    /// <summary>
    /// Plays the win beat once the player escapes: a brief full-screen flash
    /// ("light flooding in"), then the win screen with play time and Map
    /// Fragments collected. Subscribes to <see cref="ProgressionManager.GameWon"/>.
    /// </summary>
    public sealed class WinSequenceController : MonoBehaviour
    {
        [SerializeField] private ProgressionManager progressionManager;
        [SerializeField] private CanvasGroup flashOverlay;
        [SerializeField] private CanvasGroup winScreen;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text fragmentsText;

        [Tooltip("Flash fade in/out duration in seconds.")]
        [SerializeField] private float flashSeconds = 0.5f;

        [Tooltip("How long the flash holds at full brightness before the win screen appears.")]
        [SerializeField] private float holdSeconds = 1f;

        [Tooltip("Scene to load from the win screen's Main Menu button.")]
        [SerializeField] private string mainMenuSceneName = "Cutscene";

        private MotionHandle _flashHandle;
        private MotionHandle _winScreenHandle;
        private CancellationTokenSource _sequenceCts;

        private void OnEnable()
        {
            if (progressionManager == null)
            {
                Debug.LogError($"{nameof(WinSequenceController)} on '{name}' requires a {nameof(ProgressionManager)} reference.", this);
                enabled = false;
                return;
            }

            if (flashOverlay != null)
                flashOverlay.alpha = 0f;

            if (winScreen != null)
            {
                winScreen.alpha = 0f;
                winScreen.gameObject.SetActive(false);
            }

            progressionManager.GameWon += OnGameWon;
        }

        private void OnDisable()
        {
            if (progressionManager != null)
                progressionManager.GameWon -= OnGameWon;

            CancelSequence();
            _flashHandle.TryCancel();
            _winScreenHandle.TryCancel();
        }

        private void OnDestroy()
        {
            _sequenceCts?.Dispose();
        }

        private void OnGameWon(WinStats stats)
        {
            CancelSequence();
            _sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            PlaySequenceAsync(stats, _sequenceCts.Token);
        }

        private async void PlaySequenceAsync(WinStats stats, CancellationToken ct)
        {
            try
            {
                FadeFlashTo(1f);
                await Awaitable.WaitForSecondsAsync(flashSeconds + holdSeconds, ct);

                ApplyStats(stats);

                if (winScreen != null)
                {
                    winScreen.gameObject.SetActive(true);
                    _winScreenHandle.TryCancel();
                    _winScreenHandle = LMotion.Create(0f, 1f, flashSeconds)
                        .Bind(this, (a, self) => self.winScreen.alpha = a);
                }

                FadeFlashTo(0f);
            }
            catch (OperationCanceledException) { }
        }

        private void FadeFlashTo(float alpha)
        {
            if (flashOverlay == null)
                return;

            _flashHandle.TryCancel();
            _flashHandle = LMotion.Create(flashOverlay.alpha, alpha, flashSeconds)
                .Bind(this, (a, self) => self.flashOverlay.alpha = a);
        }

        private void ApplyStats(WinStats stats)
        {
            if (timeText != null)
            {
                int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(stats.PlayTimeSeconds));
                timeText.text = $"Time: {totalSeconds / 60:00}:{totalSeconds % 60:00}";
            }

            if (fragmentsText != null)
                fragmentsText.text = $"Map Fragments: {stats.Fragments} / 4";
        }

        private void CancelSequence()
        {
            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();
            _sequenceCts = null;
        }

        /// <summary>Wired to the win screen's Main Menu button.</summary>
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
