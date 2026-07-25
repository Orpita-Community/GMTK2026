using System;
using System.Threading;
using LitMotion;
using TMPro;
using UnityEngine;
using Orpita.Interaction;
using Orpita.Inventory;

namespace Orpita.UI
{
    /// <summary>
    /// Shows a brief "Wrong key" or "Need a key" message whenever the player
    /// attempts to interact with a locked <see cref="KeyChest"/> without the
    /// correct key. Subscribes to <see cref="InteractionManager.InteractionBlocked"/>
    /// and determines the appropriate message by inspecting the player's inventory.
    /// </summary>
    public sealed class ChestFeedbackView : MonoBehaviour
    {
        [SerializeField] private InteractionManager manager;
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
            if (manager == null || inventory == null)
            {
                Debug.LogError(
                    $"{nameof(ChestFeedbackView)} on '{name}' is missing required references " +
                    $"(needs InteractionManager and PlayerInventory).", this);
                enabled = false;
                return;
            }

            if (feedbackGroup != null)
                feedbackGroup.alpha = 0f;

            manager.InteractionBlocked += OnInteractionBlocked;
        }

        private void OnDisable()
        {
            if (manager != null)
                manager.InteractionBlocked -= OnInteractionBlocked;

            CancelDismiss();
            _fadeHandle.TryCancel();
        }

        private void OnDestroy()
        {
            _dismissCts?.Dispose();
        }

        private void OnInteractionBlocked(IInteractable target)
        {
            if (target is not KeyChest chest || chest.IsOpened)
                return;

            string message = inventory.HasKey ? "Wrong key" : "Need a key";
            ShowFeedback(message);
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
