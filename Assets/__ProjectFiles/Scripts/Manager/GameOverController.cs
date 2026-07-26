using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;
using Orpita.Candle;

namespace Orpita.Core
{
    public class GameOverController : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Link the Player's Candle object here.")]
        [SerializeField] private CandleFuel candle;
        
        [Header("UI Setup")]
        [Tooltip("A UI Image that is completely black, covering the whole screen.")]
        [SerializeField] private GameObject blackScreen;
        
        [Tooltip("The UI Panel containing the Try Again and Main Menu buttons.")]
        [SerializeField] private GameObject gameOverButtonsPanel;
        
        [Tooltip("The exact name of your Main Menu scene.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Tooltip("How many seconds the screen stays black before the buttons appear.")]
        [SerializeField] private float delayBeforeButtons = 2.5f;

        private IDisposable _subscription;
        private bool _isDead;

        private void OnEnable()
        {
            // Ensure UI is hidden at the start of the game
            if (blackScreen != null) blackScreen.SetActive(false);
            if (gameOverButtonsPanel != null) gameOverButtonsPanel.SetActive(false);
            
            // Subscribe to the candle fuel tracker
            if (candle != null)
            {
                _subscription = candle.SecondsRemainingRx.Subscribe(OnFuelChanged);
            }
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
        }

        private void OnFuelChanged(float fuel)
        {
            if (_isDead) return;

            // When fuel hits 0, trigger the death sequence
            if (fuel <= 0f)
            {
                _isDead = true;
                StartCoroutine(DeathSequence());
            }
        }

        private IEnumerator DeathSequence()
        {
            // 1. FREEZE TIME IMMEDIATELY (stops the player, monsters, and physics)
            Time.timeScale = 0f;

            // 2. SNAP TO BLACK SCREEN
            if (blackScreen != null) blackScreen.SetActive(true);

            // 3. WAIT IN THE DARK 
            // (We MUST use WaitForSecondsRealtime because timeScale is 0!)
            yield return new WaitForSecondsRealtime(delayBeforeButtons);

            // 4. SHOW THE BUTTONS
            if (gameOverButtonsPanel != null) gameOverButtonsPanel.SetActive(true);
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Try Again' button.
        /// </summary>
        public void TryAgain()
        {
            Time.timeScale = 1f; // MUST reset time before loading!
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Hook this to the OnClick event of your 'Main Menu' button.
        /// </summary>
        public void MainMenu()
        {
            Time.timeScale = 1f; // MUST reset time before loading!
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}