using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using Orpita.Interaction;
using Orpita.Items;

namespace Orpita.Core
{
    /// <summary>Where a <see cref="Door"/> currently is in its lifecycle.</summary>
    public enum DoorState
    {
        Locked,
        Closed,
        Open
    }

    /// <summary>
    /// A generic room-to-room door: closed doors open with a single E-press,
    /// locked doors need either a matching key (consumed on interact, opens
    /// immediately) or an external <see cref="Unlock"/> call from a progression
    /// event (which only clears the lock — the player still separately opens
    /// it). Its blocking <see cref="Collider2D"/> is solid, not a trigger, so it
    /// physically blocks anything with a collider while Closed/Locked — including
    /// a future ghost, with no extra code needed for that.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Door : InteractableBase
    {
        [SerializeField] private DoorState initialState = DoorState.Closed;

        [Tooltip("Solid collider blocking the doorway while Closed/Locked. If empty, resolved from this GameObject.")]
        [SerializeField] private Collider2D blockingCollider;

        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

        [SerializeField] private Color closedTint = Color.white;

        [Tooltip("Tint applied while Locked, so it reads as distinct from Closed without new art.")]
        [SerializeField] private Color lockedTint = new Color(0.7f, 0.35f, 0.35f);

        [Tooltip("Optional key required to unlock by interacting directly. Leave empty for event-only unlock.")]
        [SerializeField] private KeyDefinition requiredKey;

        [Tooltip("Open-animation pop duration in seconds.")]
        [SerializeField] private float openPopSeconds = 0.3f;

        private MotionHandle _openHandle;

        /// <summary>The door's current state.</summary>
        public DoorState State { get; private set; }

        /// <summary>Raised whenever the door's state changes.</summary>
        public event Action<DoorState> StateChanged;

        /// <summary>Raised when an external progression event clears the lock (Locked → Closed).</summary>
        public event Action Unlocked;

        // Doors open on a single press, not a hold channel.
        private void Reset() => interactionDuration = 0f;

        private void Awake()
        {
            blockingCollider ??= GetComponent<Collider2D>();
            ApplyState(initialState);
        }

        /// <inheritdoc/>
        public override bool CanInteract(InteractionContext ctx)
        {
            return State switch
            {
                DoorState.Open => false,
                DoorState.Closed => true,
                DoorState.Locked => requiredKey != null && ctx.Inventory.HasMatchingKey(requiredKey),
                _ => false
            };
        }

        /// <inheritdoc/>
        public override void OnInteract(InteractionContext ctx)
        {
            if (State == DoorState.Locked)
            {
                // Only reachable here with a matching key (see CanInteract) — a
                // locked door you hold the key for just opens, one action.
                ctx.Inventory.ConsumeKey();
            }

            Open();
        }

        /// <inheritdoc/>
        public override void SetHighlighted(bool highlighted)
        {
            if (State == DoorState.Open)
                return;

            base.SetHighlighted(highlighted);
        }

        /// <summary>Clears the lock (Locked → Closed) from an external progression event. No-op otherwise.</summary>
        public void Unlock()
        {
            if (State != DoorState.Locked)
                return;

            ApplyState(DoorState.Closed);
            Unlocked?.Invoke();
        }

        private void Open()
        {
            ApplyState(DoorState.Open);
        }

        private void ApplyState(DoorState state)
        {
            State = state;

            if (blockingCollider != null)
                blockingCollider.enabled = state != DoorState.Open;

            if (highlightTarget != null)
            {
                switch (state)
                {
                    case DoorState.Locked:
                        highlightTarget.sprite = closedSprite != null ? closedSprite : highlightTarget.sprite;
                        highlightTarget.color = lockedTint;
                        break;
                    case DoorState.Closed:
                        highlightTarget.sprite = closedSprite != null ? closedSprite : highlightTarget.sprite;
                        highlightTarget.color = closedTint;
                        break;
                    case DoorState.Open:
                        if (openSprite != null)
                            highlightTarget.sprite = openSprite;
                        highlightTarget.color = closedTint;

                        _openHandle.TryCancel();
                        _openHandle = LMotion.Create(highlightTarget.transform.localScale, Vector3.one, openPopSeconds)
                            .WithEase(Ease.OutQuad)
                            .BindToLocalScale(highlightTarget.transform);
                        break;
                }
            }

            StateChanged?.Invoke(state);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _openHandle.TryCancel();
        }
    }
}
