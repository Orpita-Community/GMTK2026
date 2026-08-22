using System.Collections;
using UnityEngine;
using Orpita.Inventory;

namespace Orpita.Visuals
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Normal Camera Settings")]
        [Tooltip("Drag your Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("How long it takes the camera to catch up. Higher = slower/floatier.")]
        [SerializeField] private float smoothTime = 0.25f;

        [Tooltip("The offset from the player. Z must stay negative!")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Cinematic Sequence Settings")]
        [Tooltip("Link the PlayerInventory here to listen for fragments.")]
        [SerializeField] private PlayerInventory playerInventory;
        
        [Tooltip("Where the camera should look during the cinematic.")]
        [SerializeField] private Transform treasureLocation;

        [Tooltip("The Door GameObject to enable.")]
        [SerializeField] private GameObject exitDoor;

        [Tooltip("The Light GameObject to turn on.")]
        [SerializeField] private GameObject basementLight;

        [Tooltip("How many seconds to stare at the treasure room.")]
        [SerializeField] private float cinematicFocusDuration = 2.5f;

        [Tooltip("How smooth/slow the camera pans during the cinematic.")]
        [SerializeField] private float cinematicSmoothTime = 0.8f;

        private Vector3 _velocity = Vector3.zero;
        private bool _isCinematicPlaying = false;
        private bool _hasPlayedCinematic = false;
        private Transform _cinematicTarget;

        private void OnEnable()
        {
            if (playerInventory != null)
            {
                playerInventory.MapFragmentAdded += OnMapFragmentAdded;
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.MapFragmentAdded -= OnMapFragmentAdded;
            }
        }

        private void OnMapFragmentAdded(int totalFragments)
        {
            if (totalFragments >= 3 && !_hasPlayedCinematic)
            {
                StartCoroutine(PlayTreasureRevealCinematic());
            }
        }

        private IEnumerator PlayTreasureRevealCinematic()
        {
            _hasPlayedCinematic = true;
            _isCinematicPlaying = true;
            _cinematicTarget = treasureLocation;

            Time.timeScale = 0f;

            if (exitDoor != null) exitDoor.SetActive(true);
            if (basementLight != null) basementLight.SetActive(true);

            yield return new WaitForSecondsRealtime(cinematicFocusDuration);

            _cinematicTarget = target;

            yield return new WaitForSecondsRealtime(cinematicSmoothTime * 3f);

            Time.timeScale = 1f;
            _isCinematicPlaying = false;
        }

        private void LateUpdate()
        {
            Transform currentTarget = _isCinematicPlaying ? _cinematicTarget : target;
            if (currentTarget == null) return;

            Vector3 targetPosition = currentTarget.position + offset;
            float currentSmoothTime = _isCinematicPlaying ? cinematicSmoothTime : smoothTime;

            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref _velocity, 
                currentSmoothTime, 
                Mathf.Infinity, 
                Time.unscaledDeltaTime
            );
        }
    }
}