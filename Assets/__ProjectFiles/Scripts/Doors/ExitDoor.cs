using UnityEngine;
using Orpita.Interaction;

namespace Orpita.Core
{
    public class ExitDoor : InteractableBase
    {
        // Set the hold-E interaction duration in the inspector (e.g., 2 seconds to build final tension)

        [Tooltip("Link the ProgressionManager from the scene here.")]
        [SerializeField] private ProgressionManager progressionManager;

        public override bool CanInteract(InteractionContext ctx)
        {
            // Only interactable if the player is alive (has candle fuel) and
            // actually has the Treasure (lockdown only ever starts via the
            // Diamond pickup, so this doubles as the "has Treasure" check).
            return ctx.Candle != null && ctx.Candle.SecondsRemaining > 0f
                && progressionManager != null && progressionManager.IsLockdownActive;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            // The Win State!
            Debug.Log("YOU SURVIVED! MANSION ESCAPED!");

            progressionManager?.TriggerWin();

            // Prevent further interaction
            gameObject.SetActive(false);
        }
    }
}