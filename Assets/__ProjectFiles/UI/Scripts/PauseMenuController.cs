using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Orpita.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI Setup")]
        [Tooltip("The parent GameObject containing all the pause menu buttons and background.")]
        [SerializeField] private GameObject pauseMenuPanel;

        [Header("Scene Loading")]
        [Tooltip("The exact name of your Main Menu scene.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Input Setup")]
        [Tooltip("The Input Action used to trigger the pause menu (e.g., Escape key).")]
        [SerializeField] private InputActionReference pauseAction;

        private bool _isPaused = false;

        private void Awake()
        {
            // Ensure the pause menu is hidden when the game starts
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.Enable();
                pauseAction.action.performed += HandlePauseInput;
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.performed -= HandlePauseInput;
                pauseAction.action.Disable();
            }
        }

        private void HandlePauseInput(InputAction.CallbackContext context)
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            _isPaused = true;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            
            // Freeze time
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Resume' button.
        /// </summary>
        public void ResumeGame()
        {
            _isPaused = false;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            
            // Unfreeze time
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Restart' button.
        /// </summary>
        public void RestartGame()
        {
            // Reset time and reload the current active scene
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Main Menu' button.
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}