using UnityEngine;
using Orpita.Inventory;
using Orpita.Items;
using Orpita.Interaction;
using Orpita.Audio;

namespace Orpita.Core
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("Phase Objects")]
        [Tooltip("The Exit Door object in Hallway 1.")]
        [SerializeField] private GameObject exitDoorObject;

        [Header("UI Feedback")]
        [Tooltip("The UI GameObject containing the 'Run & Find the Exit' text.")]
        [SerializeField] private GameObject lockdownWarningHUD;

        private void Start()
        {
            // Play the standard ambient loop when the scene loads
            AudioManager.Instance.PlayMusic("BGM_Normal");
        }        

        private void OnEnable()
        {
            // Hide endgame objects and warnings at the start
            if (exitDoorObject != null) exitDoorObject.SetActive(false);
            if (lockdownWarningHUD != null) lockdownWarningHUD.SetActive(false);
        }

        public void TriggerLockdown()
        {
            Debug.Log("LOCKDOWN MODE INITIATED!");

            AudioManager.Instance.PlayMusic("BGM_Lockdown");

            // 1. Reveal the Exit Door
            if (exitDoorObject != null) exitDoorObject.SetActive(true);

            // 2. Show the Urgent Warning Text
            if (lockdownWarningHUD != null) lockdownWarningHUD.SetActive(true);

            // 3. Destroy all Wax Pickups on the ground
            WaxPickup[] waxPickups = FindObjectsByType<WaxPickup>(FindObjectsSortMode.None);
            foreach (WaxPickup wax in waxPickups)
            {
                Destroy(wax.gameObject);
            }

            // 4. Completely shut down and darken all Refill Stations
            CandleRefillStation[] stations = FindObjectsByType<CandleRefillStation>(FindObjectsSortMode.None);
            foreach (CandleRefillStation station in stations)
            {
                station.ShutdownStation(); 
            }
        }
    }
}