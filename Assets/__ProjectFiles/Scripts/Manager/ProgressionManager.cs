using UnityEngine;
using Orpita.Inventory;
using Orpita.Items;
using Orpita.Interaction;
using System;

namespace Orpita.Core
{
    /// <summary>Stats shown on the win screen at the moment the player escapes.</summary>
    public readonly struct WinStats
    {
        public readonly float PlayTimeSeconds;
        public readonly int Fragments;

        public WinStats(float playTimeSeconds, int fragments)
        {
            PlayTimeSeconds = playTimeSeconds;
            Fragments = fragments;
        }
    }

    public class ProgressionManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerInventory inventory;
        
        [Header("Phase Objects")]
        [Tooltip("The physical Diamond object in the Basement.")]
        [SerializeField] private GameObject diamondObject;
        
        [Tooltip("The Exit Door object in Hallway 1.")]
        [SerializeField] private GameObject exitDoorObject;

        [Tooltip("The door blocking the vault entrance.")]
        [SerializeField] private VaultDoor vaultDoor;

        [Header("Rules")]
        [SerializeField] private int fragmentsToTriggerDiamond = 3;

        [SerializeField] private int fragmentsToUnlockVault = 4;

        /// <summary>True once Lockdown Mode has been triggered.</summary>
        public bool IsLockdownActive { get; private set; }

        /// <summary>True once the player has won (reached the Exit Door with the Treasure).</summary>
        public bool IsGameWon { get; private set; }

        /// <summary>Raised once when Lockdown Mode starts.</summary>
        public event Action LockdownStarted;

        /// <summary>Raised once when the player wins, with the stats to show.</summary>
        public event Action<WinStats> GameWon;

        private float _runStartTime;

        private void Awake()
        {
            _runStartTime = Time.time;
        }

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

            // Unlock the vault door once all fragments are collected
            if (total >= fragmentsToUnlockVault && vaultDoor != null)
            {
                vaultDoor.Unlock();
                Debug.Log("All Fragments Collected! The Vault door has unlocked.");
            }
        }

        public void TriggerLockdown()
        {
            if (IsLockdownActive)
                return;

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

            IsLockdownActive = true;
            LockdownStarted?.Invoke();
        }

        public void TriggerWin()
        {
            if (IsGameWon)
                return;

            IsGameWon = true;
            Debug.Log("YOU WIN! The player escaped with the Treasure.");

            int fragments = inventory != null ? inventory.MapFragments : 0;
            GameWon?.Invoke(new WinStats(Time.time - _runStartTime, fragments));
        }
    }
}