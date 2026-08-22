using System;
using R3;
using UnityEngine;
using Orpita.Candle;
using Orpita.Audio;

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
        private bool _isBleeding; 

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
            
            if (currentSeconds <= halfFuel && !_isBleeding)
            {
                _isBleeding = true;
                _spriteRenderer.sprite = bleedSprite;
                
                // Play the creepy sound on the dedicated ghost channel
                AudioManager.Instance.PlayGhostSFX("PortraitBleed"); 
            }
            else if (currentSeconds > halfFuel && _isBleeding)
            {
                _isBleeding = false;
                _spriteRenderer.sprite = normalSprite;
                
                // Fade out the ghost SFX smoothly over 1.5 seconds
                AudioManager.Instance.FadeOutGhostSFX(1.5f);
            }
        }
    }
}