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

        [Tooltip("The active room's camera bounds. Null means unbounded free-follow (today's behavior).")]
        [SerializeField] private CameraRoomBounds currentRoom;

        // Required for SmoothDamp to store the current speed of the camera
        private Vector3 _velocity = Vector3.zero;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        // We use LateUpdate instead of Update so the camera moves AFTER the player has finished moving for the frame.
        // This completely eliminates jitter.
        private void LateUpdate()
        {
            if (target == null) return;

            // 1. Figure out exactly where the camera WANTS to be
            Vector3 targetPosition = target.position + offset;

            // 2. Keep that position inside the active room, if one is set
            if (currentRoom != null)
                targetPosition = ClampToRoom(targetPosition, currentRoom.Bounds);

            // 3. Smoothly slide from our current position to the target position.
            // Switching rooms just changes the clamp above, so this same SmoothDamp
            // naturally glides toward the new room's bounds too — no separate tween.
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
        }

        /// <summary>Sets the active room the camera stays within. Pass null to go back to unbounded free-follow.</summary>
        public void SetRoomBounds(CameraRoomBounds room)
        {
            currentRoom = room;
        }

        private Vector3 ClampToRoom(Vector3 desired, Bounds room)
        {
            float halfHeight = _camera != null ? _camera.orthographicSize : 5f;
            float halfWidth = halfHeight * (_camera != null ? _camera.aspect : 16f / 9f);

            float minX = room.min.x + halfWidth;
            float maxX = room.max.x - halfWidth;
            float minY = room.min.y + halfHeight;
            float maxY = room.max.y - halfHeight;

            // If the room is smaller than the camera's view, center on it instead
            // of clamping into an inverted (min > max) range.
            float clampedX = minX <= maxX ? Mathf.Clamp(desired.x, minX, maxX) : room.center.x;
            float clampedY = minY <= maxY ? Mathf.Clamp(desired.y, minY, maxY) : room.center.y;

            return new Vector3(clampedX, clampedY, desired.z);
        }
    }
}