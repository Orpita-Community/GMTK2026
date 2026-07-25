using UnityEngine;
using Orpita.Core;
using Orpita.Player;

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class DiamondPickup : MonoBehaviour
    {
        [Tooltip("Lockdown Manager — preferred. Called directly when assigned.")]
        [SerializeField] private LockdownManager lockdownManager;

        [Tooltip("Fallback: used only when Lockdown Manager is not assigned (legacy scene wiring).")]
        [SerializeField] private ProgressionManager progressionManager;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Verify it is the player touching it by looking for their motor
            PlayerMotor player = other.GetComponentInParent<PlayerMotor>();
            
            if (player != null)
            {
                // Prefer the dedicated LockdownManager; fall back to ProgressionManager
                // so the currently-wired scene keeps working until the orchestrator
                // re-wires it to LockdownManager directly.
                if (lockdownManager != null)
                {
                    lockdownManager.Activate();
                }
                else if (progressionManager != null)
                {
                    progressionManager.TriggerLockdown();
                }
                else
                {
                    Debug.LogError("Diamond has no Lockdown Manager or Progression Manager assigned — lockdown cannot start!");
                }
                
                // Destroy the diamond after it is collected
                Destroy(gameObject);
            }
        }
    }
}