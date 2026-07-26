using System;
using R3;
using UnityEngine;
using Orpita.Candle;
using Orpita.Audio; // REQUIRED FOR AUDIO

namespace Orpita.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PortraitBleedView : MonoBehaviour
    {
        [SerializeField] private CandleFuel candle;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite bleedSprite;

        private SpriteRenderer _spriteRenderer;
        private IDisposable _subscription;
        private bool _isBleeding; // Tracks state to play sound only once

        private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

        private void OnEnable()
        {
            if (candle != null) _subscription = candle.SecondsRemainingRx.Subscribe(OnFuelChanged);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnFuelChanged(float currentSeconds)
        {
            float halfFuel = candle.MaxSeconds / 2f;
            
            // If fuel is low and it hasn't started bleeding yet
            if (currentSeconds <= halfFuel && !_isBleeding)
            {
                _isBleeding = true;
                _spriteRenderer.sprite = bleedSprite;
                
                // Play the creepy sound!
                AudioManager.Instance.PlaySFX("PortraitBleed"); 
            }
            // If they refill the candle, reset the portrait
            else if (currentSeconds > halfFuel && _isBleeding)
            {
                _isBleeding = false;
                _spriteRenderer.sprite = normalSprite;
            }
        }
    }
}