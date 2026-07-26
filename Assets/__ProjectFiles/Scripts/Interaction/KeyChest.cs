using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    /// <summary>
    /// A chest locked by a specific key. Holds the player in a 1-second stationary
    /// channel while the correct key is held; wrong or absent key blocks the channel
    /// and raises <see cref="InteractionManager.InteractionBlocked"/> for feedback.
    /// Unlocking consumes the key, grants a Map Fragment, plays an open animation,
    /// and permanently disables further interaction.
    /// </summary>
    public sealed class KeyChest : InteractableBase
    {
        [SerializeField] private KeyDefinition requiredKey;
        [SerializeField] private Sprite unlockedSprite;

        [Tooltip("Tint applied once opened, so the chest reads as spent even without dedicated open-state art.")]
        [SerializeField] private Color openedTint = new Color(0.55f, 0.55f, 0.5f, 1f);

        [Tooltip("Open-animation pop duration in seconds.")]
        [SerializeField] private float openPopSeconds = 0.3f;

        private bool _opened;
        private MotionHandle _openHandle;

        /// <summary>True once this chest has been successfully unlocked.</summary>
        public bool IsOpened => _opened;

        /// <summary>The key asset this chest requires.</summary>
        public KeyDefinition RequiredKey => requiredKey;

        /// <summary>Raised once when the chest is unlocked.</summary>
        public event Action Opened;

        // Base default is 1.5s; the chest spec calls for a 1s stationary unlock.
        private void Reset() => interactionDuration = 1f;

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx) =>
            !_opened && ctx.Inventory.HasMatchingKey(requiredKey);

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            ctx.Inventory.ConsumeKey();
            ctx.Inventory.AddMapFragment();
            _opened = true;
            PlayOpenAnimation();
            Opened?.Invoke();
        }

        /// <inheritdoc/>
        public override void SetHighlighted(bool highlighted)
        {
            if (_opened)
                return;

            base.SetHighlighted(highlighted);
        }

        private void PlayOpenAnimation()
        {
            if (highlightTarget == null)
                return;

            if (unlockedSprite != null)
                highlightTarget.sprite = unlockedSprite;

            highlightTarget.color = openedTint;

            // Pop back to the base scale InteractableBase.SetHighlighted assumes
            // (Vector3.one), so the chest doesn't stay frozen at its highlighted
            // scale once SetHighlighted starts no-op'ing after _opened is set.
            _openHandle.TryCancel();
            _openHandle = LMotion.Create(highlightTarget.transform.localScale, Vector3.one, openPopSeconds)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(highlightTarget.transform);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _openHandle.TryCancel();
        }
    }
}
