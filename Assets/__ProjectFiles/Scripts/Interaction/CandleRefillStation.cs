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

        private float _cooldownTimer;
        private bool _isOnCooldown;
        private bool _isShutDown; // Tracks if lockdown mode is active

        public event Action Refilled;

        private void Update()
        {
            // If the station was permanently shut down by the lockdown, stop running timers
            if (_isShutDown) return;

            if (_isOnCooldown)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    _isOnCooldown = false;
                    
                    if (stationLight != null) stationLight.SetActive(true);
                }
            }
        }

        public override bool CanInteract(InteractionContext ctx)
        {
            // Block interaction if cooling down OR if the lockdown has triggered
            if (_isOnCooldown || _isShutDown) return false;

            ICandle candle = ctx.Candle;
            return candle != null && candle.SecondsRemaining < candle.MaxSeconds;
        }

        public override void OnInteract(InteractionContext ctx)
        {
            AudioManager.Instance.PlaySFX("CandleRefill");
            ctx.Candle?.Refill();
            Refilled?.Invoke();

            _isOnCooldown = true;
            _cooldownTimer = cooldownDuration;
            
            if (stationLight != null) stationLight.SetActive(false);
        }

        // --- NEW METHOD CALLED BY THE PROGRESSION MANAGER ---
        public void ShutdownStation()
        {
            _isShutDown = true;
            this.enabled = false;
            
            // Turn off the interaction collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            // Turn off the light permanently
            if (stationLight != null) stationLight.SetActive(false);
        }
    }
}