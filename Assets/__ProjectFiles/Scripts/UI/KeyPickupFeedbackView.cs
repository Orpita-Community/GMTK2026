using System;
using System.Threading;
using LitMotion;
using TMPro;
using UnityEngine;
using Orpita.Inventory;
using Orpita.Items;

namespace Orpita.UI
{
    /// <summary>
    /// Shows a brief "Already carrying a key" message whenever a key pickup is
    /// blocked because the player already holds one. Subscribes to
    /// <see cref="PlayerInventory.KeyPickupBlocked"/> and auto-dismisses after
    /// <see cref="displaySeconds"/> seconds.
    /// </summary>
    public sealed class KeyPickupFeedbackView : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private CanvasGroup feedbackGroup;

        [Tooltip("How long the message stays fully visible before fading out.")]
        [SerializeField] private float displaySeconds = 2f;

        [Tooltip("Fade in/out duration in seconds.")]
        [SerializeField] private float fadeSeconds = 0.2f;

        private MotionHandle _fadeHandle;
        private CancellationTokenSource _dismissCts;

        private void OnEnable()
        {
            if (inventory == null)
            {
                Debug.LogError(
                    $"{nameof(KeyPickupFeedbackView)} on '{name}' has no PlayerInventory assigned.", this);
                enabled = false;
                return;
            }

            if (feedbackGroup != null)
                feedbackGroup.alpha = 0f;

            inventory.KeyPickupBlocked += OnKeyPickupBlocked;
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.KeyPickupBlocked -= OnKeyPickupBlocked;

            CancelDismiss();
            _fadeHandle.TryCancel();
        }

        private void OnDestroy()
        {
            _dismissCts?.Dispose();
        }

        private void OnKeyPickupBlocked(KeyDefinition _)
        {
            ShowFeedback("Already carrying a key");
        }

        private void ShowFeedback(string message)
        {
            if (feedbackLabel != null)
                feedbackLabel.text = message;

            CancelDismiss();
            FadeTo(1f);

            _dismissCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            ScheduleDismissAsync(_dismissCts.Token);
        }

        private async void ScheduleDismissAsync(CancellationToken ct)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(displaySeconds, ct);
                FadeTo(0f);
            }
            catch (OperationCanceledException) { }
        }

        private void FadeTo(float alpha)
        {
            if (feedbackGroup == null)
                return;

            _fadeHandle.TryCancel();
            _fadeHandle = LMotion.Create(feedbackGroup.alpha, alpha, fadeSeconds)
                .Bind(this, (a, self) => self.feedbackGroup.alpha = a);
        }

        private void CancelDismiss()
        {
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = null;
        }
    }
}
