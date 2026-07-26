using UnityEngine;
using UnityEngine.UI;
using Orpita.Inventory;

namespace Orpita.UI
{
    public class MapFragmentUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Link the Player or wherever the PlayerInventory script is located.")]
        [SerializeField] private PlayerInventory inventory;
        
        [Tooltip("The UI Image component that will display the map.")]
        [SerializeField] private Image mapImage;

        [Header("Map Sprites")]
        [Tooltip("Place sprites in exact order: 0 fragments, 1 fragment, 2 fragments, 3 fragments.")]
        [SerializeField] private Sprite[] fragmentSprites = new Sprite[4];

        private void OnEnable()
        {
            if (inventory != null)
            {
                // Subscribe to the event so the UI updates automatically when a fragment is added[cite: 6]
                inventory.MapFragmentAdded += UpdateMapUI;
                
                // Set the initial sprite when the game starts (usually 0)[cite: 6]
                UpdateMapUI(inventory.MapFragmentCount);
            }
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks[cite: 6]
            if (inventory != null)
            {
                inventory.MapFragmentAdded -= UpdateMapUI;
            }
        }

        private void UpdateMapUI(int currentFragments)
        {
            if (mapImage == null || fragmentSprites.Length == 0) return;

            // Clamp the value to ensure we don't look for an index outside of our array (e.g., if the player somehow gets 4 fragments)
            int index = Mathf.Clamp(currentFragments, 0, fragmentSprites.Length - 1);
            
            // Swap the sprite on the UI
            mapImage.sprite = fragmentSprites[index];
        }
    }
}