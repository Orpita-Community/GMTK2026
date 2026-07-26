using UnityEngine;
using Orpita.Core;
using Orpita.Interaction; 

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class DiamondPickup : InteractableBase
    {
        // Notice we removed the [SerializeField]. We don't need the Inspector anymore!
        private ProgressionManager _progressionManager;

        private void Start()
        {
            // The millisecond this diamond spawns, it searches the entire scene 
            // for the ProgressionManager script and links itself automatically.
            _progressionManager = Object.FindFirstObjectByType<ProgressionManager>();

            if (_progressionManager == null)
            {
                Debug.LogError("The Diamond spawned, but it couldn't find a ProgressionManager in the scene!");
            }
        }

        public override bool CanInteract(InteractionContext ctx)
        {
            // The diamond is always interactable as long as it exists
            return true;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            if (_progressionManager != null)
            {
                _progressionManager.TriggerLockdown();
            }
            
            // Destroy the diamond after it is collected
            Destroy(gameObject);
        }
    }
}