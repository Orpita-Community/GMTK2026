using UnityEngine;

namespace Orpita.VFX
{
    public class ParticleFlipper : MonoBehaviour
    {
        [Tooltip("Drag your Player's SpriteRenderer here.")]
        [SerializeField] private SpriteRenderer playerSpriteRenderer;

        private float _startXOffset;

        private void Start()
        {
            // Remember exactly how far from the center the particles started
            _startXOffset = Mathf.Abs(transform.localPosition.x);
        }

        private void Update()
        {
            if (playerSpriteRenderer == null) return;

            // If the sprite is flipped left, make the X position negative. 
            // If facing right, make it positive.
            float targetX = playerSpriteRenderer.flipX ? -_startXOffset : _startXOffset;
            
            // Apply the new position
            transform.localPosition = new Vector3(
                targetX, 
                transform.localPosition.y, 
                transform.localPosition.z
            );
        }
    }
}