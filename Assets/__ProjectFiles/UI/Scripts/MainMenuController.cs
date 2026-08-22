using UnityEngine;
using UnityEngine.SceneManagement;
using Orpita.Audio; // Added to access the AudioManager

namespace Orpita.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Loading")]
        [Tooltip("The exact name of the intro cutscene scene to load.")]
        [SerializeField] private string cutsceneSceneName = "IntroCutscene";

        [Header("UI Panels")]
        [Tooltip("The container holding the Play, Credits, and Quit buttons.")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Tooltip("The container holding the Scroll View and Back button.")]
        [SerializeField] private GameObject creditsPanel;

        [Header("Audio")]
        [Tooltip("Name of the SFX to play on hover.")]
        [SerializeField] private string hoverSoundName = "menu cursor move";
        
        [Tooltip("Name of the SFX to play on click/select.")]
        [SerializeField] private string selectSoundName = "menu cursor select";

        private void Start()
        {
            // Ensure we start on the main menu and the credits are hidden
            ShowMainMenu();
        }

        /// <summary>
        /// Call this via an EventTrigger (PointerEnter) on your UI buttons.
        /// </summary>
        public void PlayHoverSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hoverSoundName);
            }
        }

        /// <summary>
        /// Plays the select sound. Called automatically by the button methods below.
        /// </summary>
        private void PlaySelectSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(selectSoundName);
            }
        }

        public void PlayGame()
        {
            PlaySelectSound();
            // Ensure your cutscene scene is added in File -> Build Settings!
            SceneManager.LoadScene(cutsceneSceneName);
        }

        public void OpenCredits()
        {
            PlaySelectSound();
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void CloseCredits()
        {
            PlaySelectSound();
            ShowMainMenu();
        }

        public void QuitGame()
        {
            PlaySelectSound();
            Debug.Log("Quit Game Initiated!");
            Application.Quit();
        }

        private void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            creditsPanel.SetActive(false);
        }
    }
}