using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Orpita.Interaction
{
    /// <summary>
    /// Base for all hold-to-interact objects. Provides shared interaction fields
    /// (channel duration, prompt) and a LitMotion scale highlight driven by the
    /// <see cref="InteractionManager"/> proximity upkeep. Subclasses implement
    /// <see cref="OnInteract"/> and may override <see cref="CanInteract"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Tooltip("Stationary channel duration in seconds; <= 0 means instant.")]
        [SerializeField] protected float interactionDuration = 1.5f;

        [Tooltip("Prompt shown to the player (e.g. Search, Refill, Unlock).")]
        [SerializeField] protected string prompt = "Interact";

        [Tooltip("SpriteRenderer whose Transform scales on highlight. If empty, this object's SpriteRenderer is used.")]
        [SerializeField] protected SpriteRenderer highlightTarget;

        [Tooltip("Scale multiplier applied while this is the current proximity target.")]
        [SerializeField] protected float highlightScale = 1.12f;

        [Tooltip("Highlight tween duration in seconds.")]
        [SerializeField] private float highlightSeconds = 0.15f;

        private MotionHandle _highlightHandle;

        /// <inheritdoc/>
        public Transform Transform => transform;

        /// <inheritdoc/>
        public float InteractionDuration => interactionDuration;

        /// <inheritdoc/>
        public string Prompt => prompt;

        /// <inheritdoc/>
        public virtual bool CanInteract(InteractionContext ctx) => true;

        /// <inheritdoc/>
        public abstract void OnInteract(InteractionContext ctx);

        /// <inheritdoc/>
        public virtual void SetHighlighted(bool highlighted)
        {
            if (highlightTarget == null)
                return;

            // TryCancel is safe on a default or already-finished handle; the
            // throwing Cancel() would throw on those (LitMotion throws
            // InvalidOperationException on invalid/finished handles).
            _highlightHandle.TryCancel();

            Vector3 targetScale = highlighted ? Vector3.one * highlightScale : Vector3.one;
            _highlightHandle = LMotion.Create(highlightTarget.transform.localScale, targetScale, highlightSeconds)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(highlightTarget.transform);
        }

        protected virtual void OnDestroy()
        {
            _highlightHandle.TryCancel();
        }
    }
}
