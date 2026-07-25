using UnityEngine;
using Orpita.Interaction;

namespace Orpita.Core
{
    public class FloorDoor : InteractableBase
    {
        [Tooltip("The door object on the floor you want to teleport to.")]
        [SerializeField] private Transform destinationDoor;

        public override bool CanInteract(InteractionContext ctx)
        {
            // The player can only interact if a destination is successfully linked
            return destinationDoor != null;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            // Because we know the Inventory is attached to the root Player object, 
            // we can use it to grab the Player's transform.
            Transform playerTransform = ctx.Inventory.transform;
            
            // Instantly snap the player to the destination door
            playerTransform.position = destinationDoor.position;
        }
    }
}