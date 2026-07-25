using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Deals contact damage to the player on trigger overlap. This is the hook
    /// ghosts and other hazards will use; it owns no timing of its own because the
    /// i-frame window on <see cref="PlayerHealth"/> already prevents repeat hits
    /// while a source stays overlapped. Follows the WaxPickup trigger pattern
    /// (<c>GetComponentInParent</c> on the other collider).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class ContactDamageDealer : MonoBehaviour
    {
        [Tooltip("Damage applied on each contact.")]
        [SerializeField, Min(1)] private int damage = 1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            DealDamage(other);
        }

        // Stay lets a lingering overlap hurt the player again the instant i-frames
        // expire, without this component needing its own timer.
        private void OnTriggerStay2D(Collider2D other)
        {
            DealDamage(other);
        }

        private void DealDamage(Collider2D other)
        {
            // The player hierarchy exposes PlayerHealth on an ancestor of the
            // overlapping collider, matching how WaxPickup resolves ICandle.
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }
}
