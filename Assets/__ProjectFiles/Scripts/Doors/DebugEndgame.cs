using UnityEngine;
using Orpita.Inventory;
using UnityEngine.InputSystem;

namespace Orpita.DebugTools
{
    public class DebugEndgame : MonoBehaviour
    {
        [Tooltip("Drag the Player (or whatever holds the PlayerInventory) here.")]
        [SerializeField] private PlayerInventory inventory;

        private void Update()
        {
            // Press F2 to instantly add a map fragment
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                if (inventory != null)
                {
                    inventory.AddMapFragment();
                    Debug.Log("DEBUG: Spawned 1 Map Fragment.");
                }
                else
                {
                    Debug.LogWarning("DEBUG: PlayerInventory not assigned to DebugEndgame script!");
                }
            }
        }
    }
}