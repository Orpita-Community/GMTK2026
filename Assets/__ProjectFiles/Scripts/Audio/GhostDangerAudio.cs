using System;
using R3;
using UnityEngine;
using Orpita.Candle;
using Orpita.Audio;

namespace Orpita.Environment
{
    public class GhostDangerAudio : MonoBehaviour
    {
        [SerializeField] private CandleFuel candle;
        [SerializeField] private float dangerThreshold = 10f;

        private IDisposable _subscription;
        private bool _isPlayingDangerSound;
        private bool _isDead; // Ensures the death sound only fires once

        private void OnEnable()
        {
            if (candle != null) _subscription = candle.SecondsRemainingRx.Subscribe(OnFuelChanged);
        }

        private void OnDisable() => _subscription?.Dispose();

        private void OnFuelChanged(float currentSeconds)
        {
            if (_isDead) return; // Stop checking if the player is already dead

            // 1. THE DEATH TRIGGER (Timer runs out)
            if (currentSeconds <= 0f)
            {
                _isDead = true;
                AudioManager.Instance.PlaySFX("GhostDeath");
                return;
            }

            // 2. THE DANGER TRIGGER (Low Fuel)
            if (currentSeconds <= dangerThreshold && currentSeconds > 0f && !_isPlayingDangerSound)
            {
                _isPlayingDangerSound = true;
                AudioManager.Instance.PlaySFX("GhostDanger");
            }
            
            // 3. THE RESET (If they refill the candle)
            else if (currentSeconds > dangerThreshold && _isPlayingDangerSound)
            {
                _isPlayingDangerSound = false;
            }
        }
    }
}