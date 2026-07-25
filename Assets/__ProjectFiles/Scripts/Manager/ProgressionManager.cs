using UnityEngine;
using Orpita.Inventory;
using System;

namespace Orpita.Core
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Owns Lockdown Mode state and effects. TriggerLockdown() delegates to it.")]
        [SerializeField] private LockdownManager lockdownManager;
        
        [Header("Phase Objects")]
        [Tooltip("The physical Diamond object in the Basement.")]
        [SerializeField] private GameObject diamondObject;
        
        [Header("Rules")]
        [SerializeField] private int fragmentsToTriggerDiamond = 3;

        private void OnEnable()
        {
            if (inventory != null)
                inventory.MapFragmentAdded += OnFragmentAdded;
                
            // Hide the Diamond at the start; it appears once enough fragments are collected.
            if (diamondObject != null) diamondObject.SetActive(false);
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

        // Kept as a thin delegate so existing scene wiring (DiamondPickup -> TriggerLockdown)
        // keeps working until scenes are re-wired to LockdownManager.Activate() directly.
        public void TriggerLockdown()
        {
            if (lockdownManager != null)
                lockdownManager.Activate();
            else
                Debug.LogWarning("ProgressionManager.TriggerLockdown called but no LockdownManager is assigned.", this);
        }
    }
}