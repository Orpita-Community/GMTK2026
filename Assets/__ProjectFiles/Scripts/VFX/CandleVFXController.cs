using System;
using UnityEngine;
using R3;
using Orpita.Candle;

namespace Orpita.VFX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class CandleVFXController : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Link the Player's Candle object here.")]
        [SerializeField] private CandleFuel candle;
        
        [Header("Size Settings")]
        [Tooltip("The size of the particles when the candle is completely full.")]
        [SerializeField] private float maxSize = 0.5f;
        
        [Tooltip("The size of the particles right before the candle dies.")]
        [SerializeField] private float minSize = 0.05f;

        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _mainModule;
        private IDisposable _subscription;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            // We have to cache the main module to edit it at runtime
            _mainModule = _particleSystem.main; 
        }

        private void OnEnable()
        {
            if (candle != null)
            {
                _subscription = candle.SecondsRemainingRx.Subscribe(UpdateParticleSize);
            }
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
        }

        private void UpdateParticleSize(float currentFuel)
        {
            if (candle.MaxSeconds <= 0) return;

            // Get a percentage from 0 (empty) to 1 (full)
            float fuelPercent = currentFuel / candle.MaxSeconds;
            
            // Calculate the new size based on the percentage
            float newSize = Mathf.Lerp(minSize, maxSize, fuelPercent);
            
            // Apply it to the particle system's start size
            _mainModule.startSizeMultiplier = newSize;
        }
    }
}