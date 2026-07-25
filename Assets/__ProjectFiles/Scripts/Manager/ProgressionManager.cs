using UnityEngine;
using Orpita.Inventory;
using Orpita.Items;
using Orpita.Interaction;
using System;

namespace Orpita.Core
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerInventory inventory;
        
        [Header("Phase Objects")]
        [Tooltip("The physical Diamond object in the Basement.")]
        [SerializeField] private GameObject diamondObject;
        
        [Tooltip("The Exit Door object in Hallway 1.")]
        [SerializeField] private GameObject exitDoorObject;
        
        [Header("Rules")]
        [SerializeField] private int fragmentsToTriggerDiamond = 3;

        private void OnEnable()
        {
            if (inventory != null)
                inventory.MapFragmentAdded += OnFragmentAdded;
                
            // Hide endgame objects at the start
            if (diamondObject != null) diamondObject.SetActive(false);
            if (exitDoorObject != null) exitDoorObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.MapFragmentAdded -= OnFragmentAdded;
        }

        private void OnFragmentAdded(int total)
        {
            // Spawn the Diamond when the 3rd fragment is collected
            if (total >= fragmentsToTriggerDiamond && diamondObject != null)
            {
                diamondObject.SetActive(true);
                Debug.Log("3 Fragments Collected! The Diamond has appeared in the Basement.");
            }
        }

        public void TriggerLockdown()
        {
            Debug.Log("LOCKDOWN MODE INITIATED!");

            // 1. Reveal the Exit Door in Hallway 1
            if (exitDoorObject != null) exitDoorObject.SetActive(true);

            // 2. Destroy all Wax Pickups on the ground
            WaxPickup[] waxPickups = FindObjectsByType<WaxPickup>(FindObjectsSortMode.None);
            foreach (WaxPickup wax in waxPickups)
            {
                Destroy(wax.gameObject);
            }

            // 3. Deactivate all Candle Refill Stations
            CandleRefillStation[] stations = FindObjectsByType<CandleRefillStation>(FindObjectsSortMode.None);
            foreach (CandleRefillStation station in stations)
            {
                // Disabling the GameObject removes it from the InteractionDetector entirely
                station.gameObject.SetActive(false); 
            }
        }
    }
}