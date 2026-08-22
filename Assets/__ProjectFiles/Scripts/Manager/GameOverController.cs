using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;
using Orpita.Candle;
using Orpita.Audio; // <-- ADDED THIS

namespace Orpita.Core
{
    public class GameOverController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CandleFuel candle;
        
        [Header("UI Setup")]
        [SerializeField] private GameObject blackScreen;
        [SerializeField] private GameObject gameOverButtonsPanel;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private float delayBeforeButtons = 2.5f;

        private IDisposable _subscription;
        private bool _isDead;

        private void OnEnable()
        {
            if (blackScreen != null) blackScreen.SetActive(false);
            if (gameOverButtonsPanel != null) gameOverButtonsPanel.SetActive(false);
            
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

            if (fuel <= 0f)
            {
                _isDead = true;
                StartCoroutine(DeathSequence());
            }
        }

        private IEnumerator DeathSequence()
        {
            // Stop background music and play ghost sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlaySFX("GhostDeath"); // <-- MAKE SURE TO MATCH YOUR SFX NAME
            }

            // 1. FREEZE TIME IMMEDIATELY
            Time.timeScale = 0f;

            // 2. SNAP TO BLACK SCREEN
            if (blackScreen != null) blackScreen.SetActive(true);

            // 3. WAIT IN THE DARK 
            yield return new WaitForSecondsRealtime(delayBeforeButtons);

            // 4. SHOW THE BUTTONS
            if (gameOverButtonsPanel != null) gameOverButtonsPanel.SetActive(true);
        }

        public void TryAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void MainMenu()
        {
            Time.timeScale = 1f; 
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}