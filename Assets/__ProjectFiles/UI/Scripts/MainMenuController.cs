using UnityEngine;
using UnityEngine.SceneManagement;

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

        private void Start()
        {
            // Ensure we start on the main menu and the credits are hidden
            ShowMainMenu();
        }

        public void PlayGame()
        {
            // Ensure your cutscene scene is added in File -> Build Settings!
            SceneManager.LoadScene(cutsceneSceneName);
        }

        public void OpenCredits()
        {
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void CloseCredits()
        {
            ShowMainMenu();
        }

        public void QuitGame()
        {
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