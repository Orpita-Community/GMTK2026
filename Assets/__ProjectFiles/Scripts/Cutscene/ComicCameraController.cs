using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Orpaits.Cinematics
{
    [RequireComponent(typeof(Camera))]
    public class ComicCameraController : MonoBehaviour
    {
        [Serializable]
        public class ComicPanel
        {
            public Transform focusPoint;
            public float cameraZoom = 5f;
            public AudioClip panelAudio;
        }

        [Header("Comic Sequence")]
        [SerializeField] private ComicPanel[] sequence;

        [Header("Camera Feel")]
        [SerializeField] private float defaultSmoothTime = 0.3f;
        
        [Header("Transitions")]
        [SerializeField] [Tooltip("How long it takes to fade in from black when the scene starts")] 
        private float startFadeDuration = 2.0f;
        
        [SerializeField] [Tooltip("How long to hold the completely black screen at the end before loading the game")] 
        private float finalBlackHoldDuration = 1.5f;

        [SerializeField] [Tooltip("The Canvas Group on the Black Screen UI image")]
        private CanvasGroup fadeOverlay;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Controls")]
        [SerializeField] private InputActionReference advanceAction;

        [Header("Scene Loading")]
        [SerializeField] private string nextSceneName = "SampleScene";

        private Camera cam;
        private int currentIndex = -1;
        private bool isTransitioning = false;

        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 positionVelocity = Vector3.zero;
        private float zoomVelocity = 0f;
        private float currentSmoothTime;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            
            targetPosition = transform.position;
            targetZoom = cam.orthographicSize;
            currentSmoothTime = defaultSmoothTime;

            // Start completely black
            if (fadeOverlay != null) fadeOverlay.alpha = 1f;
        }

        private void OnEnable()
        {
            if (advanceAction != null)
            {
                advanceAction.action.Enable();
                advanceAction.action.performed += HandleAdvance;
            }
        }

        private void OnDisable()
        {
            if (advanceAction != null)
            {
                advanceAction.action.performed -= HandleAdvance;
                advanceAction.action.Disable();
            }
        }

        private async void Start()
        {
            if (bgmSource != null && bgmSource.clip != null)
            {
                bgmSource.loop = true;
                bgmSource.Play();
            }
            
            ShowNextPanel();
            
            // Kick off the fade-in effect right as the scene starts
            await FadeInStartAsync();
        }

        private void LateUpdate()
        {
            if (currentIndex < 0 || currentIndex >= sequence.Length) return;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, currentSmoothTime);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, currentSmoothTime);
        }

        private void HandleAdvance(InputAction.CallbackContext context)
        {
            if (isTransitioning) return;
            
            if (currentIndex >= sequence.Length - 1)
            {
                _ = PlayFinalTransitionAsync();
            }
            else
            {
                ShowNextPanel();
            }
        }

        private void ShowNextPanel()
        {
            currentIndex++;
            SetupPanelTarget(sequence[currentIndex]);
        }

        private void SetupPanelTarget(ComicPanel currentPanel)
        {
            if (currentPanel.focusPoint != null)
            {
                targetPosition = new Vector3(
                    currentPanel.focusPoint.position.x, 
                    currentPanel.focusPoint.position.y, 
                    transform.position.z
                );
                targetZoom = currentPanel.cameraZoom;
            }

            if (currentPanel.panelAudio != null && sfxSource != null)
            {
                sfxSource.Stop();
                sfxSource.PlayOneShot(currentPanel.panelAudio);
            }
        }

        private async Awaitable FadeInStartAsync()
        {
            if (fadeOverlay == null) return;

            float timer = 0f;
            while (timer < startFadeDuration)
            {
                timer += Time.deltaTime;
                
                // Smoothly fade from 1 (black) to 0 (clear)
                fadeOverlay.alpha = 1f - (timer / startFadeDuration);
                
                await Awaitable.NextFrameAsync();
            }
            
            // Ensure it is completely clear at the end
            fadeOverlay.alpha = 0f; 
        }

        private async Awaitable PlayFinalTransitionAsync()
        {
            isTransitioning = true;
            
            // 1. INSTANT BLACKOUT - Just like the power went out
            if (fadeOverlay != null) fadeOverlay.alpha = 1f;
            
            // 2. Cut the music instantly to sell the effect
            if (bgmSource != null) bgmSource.Stop();

            // 3. Hold in the darkness for a moment to let the player process the scare
            await Awaitable.WaitForSecondsAsync(finalBlackHoldDuration);

            // 4. Load the game
            SceneManager.LoadScene(nextSceneName);
        }
    }
}