using System.Collections;
using UnityEngine;
using Orpita.Player;

namespace Orpita.Interaction
{
    public class Stairwell : MonoBehaviour
    {
        [Tooltip("Drag the Stairwell object from the floor ABOVE this one here.")]
        [SerializeField] private Transform upperStair;
        
        [Tooltip("Drag the Stairwell object from the floor BELOW this one here.")]
        [SerializeField] private Transform lowerStair;

        [Tooltip("How many seconds it takes to walk up/down the stairs.")]
        [SerializeField] private float climbDuration = 1.5f;

        private void OnTriggerStay2D(Collider2D other)
        {
            // 1. Grab the player's motor
            PlayerMotor motor = other.GetComponentInParent<PlayerMotor>();
            
            // 2. If we can't find it, or it is ALREADY turned off (meaning they are currently climbing), do nothing!
            if (motor == null || !motor.enabled) return;

            IMovementInput input = other.GetComponentInParent<IMovementInput>();
            if (input == null) return;

            float verticalInput = input.MoveDirection.y;

            if (verticalInput > 0.5f && upperStair != null) 
            {
                // Pass the motor into the Coroutine so we can control it safely
                StartCoroutine(ClimbStairs(other.transform, upperStair.position, motor));
            }
            else if (verticalInput < -0.5f && lowerStair != null) 
            {
                StartCoroutine(ClimbStairs(other.transform, lowerStair.position, motor));
            }
        }

        private IEnumerator ClimbStairs(Transform player, Vector3 destination, PlayerMotor motor)
        {
            Rigidbody2D rb = player.GetComponentInParent<Rigidbody2D>();

            // 1. Lock the player by turning off the motor and physics
            motor.enabled = false;
            if (rb != null) rb.simulated = false;

            Vector3 startPos = player.position;
            float timer = 0f;

            // 2. Smoothly slide up/down
            while (timer < climbDuration)
            {
                timer += Time.deltaTime;
                player.position = Vector3.Lerp(startPos, destination, timer / climbDuration);
                yield return null; 
            }

            // 3. Snap to the exact destination to prevent drifting
            player.position = destination;

            // 4. Turn the physics back on so they don't fall through the floor
            if (rb != null) rb.simulated = true;
            
            // 5. Wait a tiny fraction of a second BEFORE turning the motor back on.
            // This prevents them from instantly triggering the next stairwell if they are still holding 'W'
            yield return new WaitForSeconds(0.2f);
            
            motor.enabled = true;
        }
    }
}