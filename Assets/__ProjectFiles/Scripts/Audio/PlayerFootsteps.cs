using UnityEngine;

namespace Orpita.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private float movementThreshold = 0.1f;
        
        [Tooltip("How many seconds between each footstep sound? Increase this to slow down the steps!")]
        [SerializeField] private float timeBetweenSteps = 0.5f;

        private AudioSource _footstepSource;
        private float _stepTimer;

        private void Awake()
        {
            _footstepSource = GetComponent<AudioSource>();
            // Make sure the audio source itself is NOT set to loop anymore!
            _footstepSource.loop = false; 
        }

        private void Update()
        {
            bool isMoving = playerRigidbody.linearVelocity.magnitude > movementThreshold;

            if (isMoving)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    _footstepSource.PlayOneShot(_footstepSource.clip);
                    _stepTimer = timeBetweenSteps; // Reset the timer
                }
            }
            else
            {
                // Reset the timer when they stop so the first step happens instantly when they move again
                _stepTimer = 0f; 
            }
        }
    }
}