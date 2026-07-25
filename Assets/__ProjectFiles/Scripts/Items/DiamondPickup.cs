using UnityEngine;
using Orpita.Core;
using Orpita.Player;

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class DiamondPickup : MonoBehaviour
    {
        [Tooltip("Link the ProgressionManager from the scene here.")]
        [SerializeField] private ProgressionManager progressionManager;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Verify it is the player touching it by looking for their motor
            PlayerMotor player = other.GetComponentInParent<PlayerMotor>();
            
            if (player != null)
            {
                if (progressionManager != null)
                {
                    progressionManager.TriggerLockdown();
                }
                else
                {
                    Debug.LogError("Diamond doesn't have a reference to the ProgressionManager!");
                }
                
                // Destroy the diamond after it is collected
                Destroy(gameObject);
            }
        }
    }
}