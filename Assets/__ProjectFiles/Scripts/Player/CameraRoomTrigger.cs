using UnityEngine;
using Orpita.Player;

namespace Orpita.Visuals
{
    /// <summary>
    /// Marks a doorway threshold: when the player crosses it, tells
    /// <see cref="CameraFollow"/> to clamp toward this room's bounds. Deliberately
    /// decoupled from <see cref="Orpita.Core.Door"/> — a room transition is about
    /// crossing a threshold, not about a door object existing there (this works
    /// equally well at an already-open archway).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class CameraRoomTrigger : MonoBehaviour
    {
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private CameraRoomBounds roomBounds;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMotor>() != null)
                cameraFollow?.SetRoomBounds(roomBounds);
        }
    }
}
