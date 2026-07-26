using UnityEngine;

namespace Orpita.Visuals
{
    /// <summary>
    /// Defines one room's camera clamp rectangle, centered on this transform.
    /// Placed once per room; <see cref="CameraFollow"/> clamps toward whichever
    /// instance is currently active (set via <see cref="CameraRoomTrigger"/>).
    /// </summary>
    public sealed class CameraRoomBounds : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(20f, 12f);

        /// <summary>The room's world-space camera bounds.</summary>
        public Bounds Bounds => new Bounds(transform.position, new Vector3(size.x, size.y, 0f));

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }
    }
}
