using UnityEngine;
using System.Collections; // Added to support Coroutines
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
        
        [Header("Lockdown Settings")]
        [Tooltip("How quickly the warning text flashes in and out.")]
        [SerializeField] private float textFadeSpeed = 5f;

        // Reference to control UI transparency
        private CanvasGroup warningCanvasGroup;

        private void Start()
        {
            // Play the standard ambient loop when the scene loads
            AudioManager.Instance.PlayMusic("BGM_Normal");

            // Automatically grab or attach a CanvasGroup to the HUD for fading
            if (lockdownWarningHUD != null)
            {
                warningCanvasGroup = lockdownWarningHUD.GetComponent<CanvasGroup>();
                if (warningCanvasGroup == null)
                {
                    warningCanvasGroup = lockdownWarningHUD.AddComponent<CanvasGroup>();
                }
            }
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

            // 2. Show the Urgent Warning Text and start flashing
            if (lockdownWarningHUD != null) 
            {
                lockdownWarningHUD.SetActive(true);
                
                // Start the fading effect
                if (warningCanvasGroup != null)
                {
                    StartCoroutine(FlashWarningText());
                }
            }

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
        
        // Coroutine to handle the rapid fade in and fade out
        private IEnumerator FlashWarningText()
        {
            while (true)
            {
                // Mathf.PingPong naturally bounces a value between 0 and 1 over time
                warningCanvasGroup.alpha = Mathf.PingPong(Time.time * textFadeSpeed, 1f);
                
                // Wait until the next frame before continuing the loop
                yield return null; 
            }
        }
    }
}