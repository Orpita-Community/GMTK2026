using UnityEngine;
using Orpita.Player;
using Orpita.Candle;

namespace Orpita.Visuals
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("The candle time at which the player looks terrified.")]
        [SerializeField] private float terrifiedThreshold = 5f;

        private Animator _animator;
        private SpriteRenderer _sprite;
        private IMovementInput _movement;
        private ICandle _candle;
        
        // Add a reference to the PlayerMotor
        private PlayerMotor _motor; 

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
            _movement = GetComponent<IMovementInput>();
            _candle = GetComponentInChildren<ICandle>();
            
            // Grab the motor component
            _motor = GetComponent<PlayerMotor>(); 
        }

        private void Update()
        {
            // 1. Check if the player is currently on the stairs
            // (We know they are on the stairs if the motor was disabled by the Stairwell script)
            bool isClimbing = _motor != null && !_motor.enabled;

            // 2. Handle Walking State
            // Force the walking animation to play if they are climbing, OR if they are providing movement input
            bool isWalking = isClimbing || _movement.MoveDirection.sqrMagnitude > 0.01f;
            _animator.SetBool("IsWalking", isWalking);

            // 3. Handle Terrified State (Checks if candle is at or below threshold)
            bool isTerrified = _candle != null && _candle.SecondsRemaining <= terrifiedThreshold;
            _animator.SetBool("IsTerrified", isTerrified);

            // 4. Flip the sprite left/right based on movement direction
            // We only want to flip the sprite if they are actually walking left/right, not while climbing
            if (!isClimbing && isWalking)
            {
                if (_movement.MoveDirection.x < 0)
                    _sprite.flipX = true; // Facing Left
                else if (_movement.MoveDirection.x > 0)
                    _sprite.flipX = false; // Facing Right
            }
        }
    }
}