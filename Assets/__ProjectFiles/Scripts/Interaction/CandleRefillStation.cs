using System;
using UnityEngine;
using Orpita.Candle;
using Orpita.Audio;

namespace Orpita.Interaction
{
    public sealed class CandleRefillStation : InteractableBase
    {
        [Header("Station Settings")]
        [Tooltip("How many seconds before the station can be used again.")]
        [SerializeField] private float cooldownDuration = 30f;
        
        [Tooltip("The light object to disable while on cooldown or lockdown.")]
        [SerializeField] private GameObject stationLight;

        [Tooltip("The particle system to disable while on cooldown.")]
        [SerializeField] private ParticleSystem stationParticles; // <-- ADDED THIS

        private float _cooldownTimer;
        private bool _isOnCooldown;
        private bool _isShutDown; 

        public event Action Refilled;

        private void Update()
        {
            if (_isShutDown) return;

            if (_isOnCooldown)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    _isOnCooldown = false;
                    
                    if (stationLight != null) stationLight.SetActive(true);
                    
                    // Turn the particles back on
                    if (stationParticles != null) stationParticles.Play(); // <-- ADDED THIS
                }
            }
        }

        public override bool CanInteract(InteractionContext ctx)
        {
            if (_isOnCooldown || _isShutDown) return false;

            ICandle candle = ctx.Candle;
            return candle != null && candle.SecondsRemaining < candle.MaxSeconds;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            ctx.Candle?.Refill();
            Refilled?.Invoke();

            // If timer is half or above, fade out the ghost SFX
            if (ctx.Candle != null && ctx.Candle.SecondsRemaining >= (ctx.Candle.MaxSeconds / 2f))
            {
                AudioManager.Instance.FadeOutGhostSFX(1f);
            }

            // Play the refill sound
            AudioManager.Instance.PlaySFX("CandleRefill");

            _isOnCooldown = true;
            _cooldownTimer = cooldownDuration;
            
            if (stationLight != null) stationLight.SetActive(false);
            
            // Stop the particles from emitting
            if (stationParticles != null) stationParticles.Stop(); // <-- ADDED THIS
        }

        public void ShutdownStation()
        {
            _isShutDown = true;
            this.enabled = false;
            
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            if (stationLight != null) stationLight.SetActive(false);
            
            // Ensure particles stop if the station is locked down
            if (stationParticles != null) stationParticles.Stop(); // <-- ADDED THIS
        }
    }
}