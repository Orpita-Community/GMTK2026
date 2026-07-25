using UnityEngine;

namespace Orpita.Visuals
{
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("Drag your Player here.")]
        [SerializeField] private Transform target;

        [Tooltip("How long it takes the camera to catch up. Higher = slower/floatier.")]
        [SerializeField] private float smoothTime = 0.25f;

        [Tooltip("The offset from the player. Z must stay negative so the camera stays in front of the 2D map!")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        // Required for SmoothDamp to store the current speed of the camera
        private Vector3 _velocity = Vector3.zero;

        // We use LateUpdate instead of Update so the camera moves AFTER the player has finished moving for the frame.
        // This completely eliminates jitter.
        private void LateUpdate()
        {
            if (target == null) return;

            // 1. Figure out exactly where the camera WANTS to be
            Vector3 targetPosition = target.position + offset;

            // 2. Smoothly slide from our current position to the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
        }
    }
}