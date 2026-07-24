using UnityEngine;
using Orpita.Candle;

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class WaxPickup : MonoBehaviour
    {
        [Tooltip("How many seconds to add to the candle when picked up.")]
        [SerializeField] private float fuelAmount = 3f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Look for the ICandle component on the object that walked into us
            ICandle candle = other.GetComponentInParent<ICandle>();
            
            if (candle != null)
            {
                // Give the player the fuel
                candle.AddSeconds(fuelAmount);
                
                // Destroy the wax piece so it can't be picked up again
                Destroy(gameObject);
            }
        }
    }
}