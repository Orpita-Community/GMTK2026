using UnityEngine;
using Orpita.Candle;
using Orpita.Audio;

namespace Orpita.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class WaxPickup : MonoBehaviour
    {
        [Tooltip("How many seconds to add to the candle when picked up.")]
        [SerializeField] private float fuelAmount = 3f;

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

                // If timer is half or above, fade out the ghost SFX
                if (candle.SecondsRemaining >= (candle.MaxSeconds / 2f))
                {
                    AudioManager.Instance.FadeOutGhostSFX(.5f);
                }
                
                // Play pickup sound AFTER starting the fade out
                AudioManager.Instance.PlaySFX("CandlePickup");
                Destroy(gameObject);
            }
        }
    }
}