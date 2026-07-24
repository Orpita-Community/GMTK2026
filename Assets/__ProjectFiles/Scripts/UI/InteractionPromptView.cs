using LitMotion;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Orpita.Interaction;

namespace Orpita.UI
{
    /// <summary>
    /// Shows the current interaction target's prompt ("[E] Search") and the
    /// hold-to-interact channel progress. Pure presentation: it only observes
    /// <see cref="InteractionManager"/>'s reactive state and never drives gameplay.
    /// </summary>
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private InteractionManager manager;
        [SerializeField] private CanvasGroup promptGroup;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private GameObject channelRoot;
        [SerializeField] private Image channelFill;

        [Tooltip("Prompt fade duration in seconds.")]
        [SerializeField] private float fadeSeconds = 0.12f;

        private CompositeDisposable _subs;
        private MotionHandle _fade;

        private void OnEnable()
        {
            if (manager == null)
            {
                Debug.LogError($"{nameof(InteractionPromptView)} on '{name}' has no InteractionManager assigned.", this);
                enabled = false;
                return;
            }

            if (promptGroup != null)
                promptGroup.alpha = 0f;

            if (channelRoot != null)
                channelRoot.SetActive(false);

            _subs = new CompositeDisposable();
            manager.CurrentTarget.Subscribe(OnTargetChanged).AddTo(_subs);
            manager.ChannelProgress.Subscribe(OnProgressChanged).AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
            _fade.TryCancel();
        }

        private void OnTargetChanged(IInteractable target)
        {
            if (target == null)
            {
                FadeTo(0f);
                return;
            }

            if (promptLabel != null)
                promptLabel.text = $"[E] {target.Prompt}";

            FadeTo(1f);
        }

        private void OnProgressChanged(float progress)
        {
            bool channeling = progress > 0f;

            if (channelRoot != null && channelRoot.activeSelf != channeling)
                channelRoot.SetActive(channeling);

            if (channelFill != null)
                channelFill.fillAmount = progress;
        }

        private void FadeTo(float alpha)
        {
            if (promptGroup == null)
                return;

            _fade.TryCancel();
            _fade = LMotion.Create(promptGroup.alpha, alpha, fadeSeconds)
                .Bind(this, (a, self) => self.promptGroup.alpha = a);
        }
    }
}
