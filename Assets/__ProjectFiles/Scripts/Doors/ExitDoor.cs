using UnityEngine;
using Orpita.Interaction;

namespace Orpita.Core
{
    public class ExitDoor : InteractableBase
    {
        // Set the hold-E interaction duration in the inspector (e.g., 2 seconds to build final tension)
        
        public override bool CanInteract(InteractionContext ctx)
        {
            // Only interactable if the player is alive (has candle fuel)
            return ctx.Candle != null && ctx.Candle.SecondsRemaining > 0f;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            // The Win State!
            Debug.Log("YOU SURVIVED! MANSION ESCAPED!");
            
            // TODO: Trigger your Win UI Screen or Scene Load here
            
            // Prevent further interaction
            gameObject.SetActive(false); 
        }
    }
}