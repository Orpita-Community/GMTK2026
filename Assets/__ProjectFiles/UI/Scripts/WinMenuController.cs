using UnityEngine;
using UnityEngine.SceneManagement;

namespace Orpita.UI
{
    public class WinMenuController : MonoBehaviour
    {
        [Header("Scene Loading")]
        [Tooltip("The exact name of your Main Menu scene.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        /// <summary>
        /// Hook this to the OnClick event of your 'Main Menu' button.
        /// </summary>
        public void GoToMainMenu()
        {
            // Always reset time before loading a new scene!
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Quit' button.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Game Quit!");
            Application.Quit();
        }
    }
}