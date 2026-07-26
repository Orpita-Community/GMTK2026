using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Orpita.Core
{
    /// <summary>
    /// Physically blocks the vault entrance until unlocked. Unlike
    /// <see cref="ExitDoor"/>/<see cref="FloorDoor"/> this isn't a hold-E
    /// interactable — <see cref="ProgressionManager"/> unlocks it automatically
    /// once enough Map Fragments are collected, and once unlocked it's simply
    /// walkable, no interaction required.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class VaultDoor : MonoBehaviour
    {
        [Tooltip("Solid (non-trigger) collider blocking the doorway. If empty, resolved from this GameObject.")]
        [SerializeField] private Collider2D blockingCollider;

        [Tooltip("The door's visual. If empty, resolved from this GameObject.")]
        [SerializeField] private SpriteRenderer doorSprite;

        [Tooltip("Sprite swapped in on unlock. Optional.")]
        [SerializeField] private Sprite openSprite;

        [Tooltip("Tint applied on unlock.")]
        [SerializeField] private Color openTint = Color.white;

        [Tooltip("Open-animation pop duration in seconds.")]
        [SerializeField] private float openPopSeconds = 0.4f;

        private MotionHandle _openHandle;

        /// <summary>True once this door has been unlocked.</summary>
        public bool IsUnlocked { get; private set; }

        /// <summary>Raised once when the door unlocks.</summary>
        public event Action Unlocked;

        private void Awake()
        {
            blockingCollider ??= GetComponent<Collider2D>();
            doorSprite ??= GetComponent<SpriteRenderer>();
        }

        /// <summary>Unlocks the door: disables the blocking collider and plays the open animation. No-op if already unlocked.</summary>
        public void Unlock()
        {
            if (IsUnlocked)
                return;

            IsUnlocked = true;

            if (blockingCollider != null)
                blockingCollider.enabled = false;

            if (doorSprite != null)
            {
                if (openSprite != null)
                    doorSprite.sprite = openSprite;

                doorSprite.color = openTint;

                _openHandle.TryCancel();
                _openHandle = LMotion.Create(doorSprite.transform.localScale, Vector3.one, openPopSeconds)
                    .WithEase(Ease.OutQuad)
                    .BindToLocalScale(doorSprite.transform);
            }

            Unlocked?.Invoke();
        }

        private void OnDestroy()
        {
            _openHandle.TryCancel();
        }
    }
}
