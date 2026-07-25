using UnityEngine;
using Orpita.Items;

namespace Orpita.Interaction
{
    public sealed class KeyPickup : InteractableBase
    {
        [Tooltip("The specific key asset this object grants.")]
        [SerializeField] private KeyDefinition keyAsset;

        // The furniture will call this right after spawning the key
        public void Initialize(KeyDefinition key)
        {
            keyAsset = key;
        }

        public override bool CanInteract(InteractionContext ctx)
        {
            return true;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            if (keyAsset == null) return;

            if (ctx.Inventory.TryPickUpKey(keyAsset))
            {
                Destroy(gameObject);
            }
        }
    }
}