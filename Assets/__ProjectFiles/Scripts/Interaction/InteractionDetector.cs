using System.Collections.Generic;
using UnityEngine;

namespace Orpita.Interaction
{
    /// <summary>
    /// Maintains the live set of interactables within the player's proximity
    /// trigger and answers nearest-target queries. Keeps the detection range
    /// consistent: the trigger radius is forced to the serialized range so the
    /// <see cref="OnDrawGizmosSelected"/> circle and the actual detection volume
    /// always agree.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class InteractionDetector : MonoBehaviour
    {
        [Tooltip("Detection radius, kept in sync with the trigger collider radius.")]
        [SerializeField] private float range = 1.6f;

        [Tooltip("The proximity trigger. If left empty, a CircleCollider2D on this GameObject is used.")]
        [SerializeField] private CircleCollider2D triggerCollider;

        private readonly HashSet<IInteractable> _inRange = new HashSet<IInteractable>();

        /// <summary>Interactables currently inside the proximity trigger.</summary>
        public IReadOnlyCollection<IInteractable> InRange => _inRange;

        private void Awake()
        {
            ResolveCollider();
            ApplyColliderConfig();
        }

        private void OnValidate()
        {
            ApplyColliderConfig();
        }

        /// <summary>
        /// Returns the in-range interactable whose Transform is closest to
        /// <paramref name="from"/>; skips destroyed targets. Returns null if none.
        /// </summary>
        public IInteractable GetNearest(Vector2 from)
        {
            IInteractable nearest = null;
            float nearestSqr = float.MaxValue;

            foreach (IInteractable candidate in _inRange)
            {
                Transform t = candidate != null ? candidate.Transform : null;
                if (t == null)
                    continue;

                float sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null)
                _inRange.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null)
                _inRange.Remove(interactable);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, range);
        }

        private void ResolveCollider()
        {
            if (triggerCollider == null)
                triggerCollider = GetComponent<CircleCollider2D>();
        }

        private void ApplyColliderConfig()
        {
            if (triggerCollider == null)
                return;

            triggerCollider.isTrigger = true;
            triggerCollider.radius = range;
        }
    }
}
