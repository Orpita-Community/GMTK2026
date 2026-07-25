using UnityEngine;
using Orpita.Candle;

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class WaxPickup : MonoBehaviour
    {
        [Tooltip("How many seconds to add to the candle when picked up.")]
        [SerializeField] private float fuelAmount = 3f;

        // The furniture will call this right after spawning the wax
        public void Initialize(float amount)
        {
            fuelAmount = amount;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ICandle candle = other.GetComponentInParent<ICandle>();
            if (candle != null)
            {
                candle.AddSeconds(fuelAmount);
                Destroy(gameObject);
            }
        }
    }
}