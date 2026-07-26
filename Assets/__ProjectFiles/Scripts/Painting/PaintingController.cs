using System;
using R3;
using UnityEngine;
using Orpita.Candle;
using Orpita.Player;

namespace Orpita.Painting
{
    /// <summary>The Painted Ancestors' visible states as the candle burns down.</summary>
    public enum PaintingState
    {
        Safe,
        Watching,
        Reaching,
        Grabbing
    }

    /// <summary>
    /// A painting that reacts to the candle's remaining time: it tints/swaps to a
    /// damaged look as danger escalates, and if the candle dies while the player
    /// is standing in front of it, flips to a one-shot "Grabbing" look. Death
    /// itself is already handled by <see cref="CandleFuel"/>/<see cref="ICandle"/>
    /// (P5) — this component is purely the reactive presentation for that shared
    /// event, not a second death pathway.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PaintingController : MonoBehaviour
    {
        [Tooltip("Candle whose remaining time drives this painting's state.")]
        [SerializeField] private CandleFuel candle;

        [Tooltip("Sprite shown at Safe/Watching.")]
        [SerializeField] private Sprite calmSprite;

        [Tooltip("Sprite shown at Reaching/Grabbing.")]
        [SerializeField] private Sprite damagedSprite;

        [SerializeField] private Color safeTint = Color.white;
        [SerializeField] private Color watchingTint = new Color(0.85f, 0.8f, 0.8f);
        [SerializeField] private Color reachingTint = new Color(0.9f, 0.6f, 0.6f);
        [SerializeField] private Color grabbingTint = new Color(0.6f, 0.1f, 0.1f);

        [Tooltip("Below this many seconds remaining, the painting starts Watching.")]
        [SerializeField] private float watchingThresholdSeconds = 10f;

        [Tooltip("Below this many seconds remaining, the painting starts Reaching.")]
        [SerializeField] private float reachingThresholdSeconds = 5f;

        private SpriteRenderer _sprite;
        private Collider2D _triggerZone;
        private CompositeDisposable _subs;
        private bool _playerInZone;

        /// <summary>The painting's current state.</summary>
        public PaintingState State { get; private set; } = PaintingState.Safe;

        /// <summary>Raised whenever the painting's state changes.</summary>
        public event Action<PaintingState> StateChanged;

        /// <summary>Raised once if the candle dies while the player is in this painting's zone.</summary>
        public event Action Grabbed;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _triggerZone = GetComponent<Collider2D>();
            _triggerZone.isTrigger = true;
        }

        private void OnEnable()
        {
            _subs = new CompositeDisposable();

            if (candle == null)
            {
                Debug.LogError($"{nameof(PaintingController)} on '{name}' requires a {nameof(CandleFuel)} reference.", this);
                return;
            }

            candle.SecondsRemainingRx.Subscribe(OnSecondsChanged).AddTo(_subs);
            candle.CandleDied += OnCandleDied;

            ApplyState(PaintingState.Safe);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;

            if (candle != null)
                candle.CandleDied -= OnCandleDied;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMotor>() != null)
                _playerInZone = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMotor>() != null)
                _playerInZone = false;
        }

        private void OnSecondsChanged(float seconds)
        {
            if (State == PaintingState.Grabbing)
                return;

            PaintingState next = seconds < reachingThresholdSeconds ? PaintingState.Reaching
                : seconds < watchingThresholdSeconds ? PaintingState.Watching
                : PaintingState.Safe;

            if (next != State)
                ApplyState(next);
        }

        private void OnCandleDied()
        {
            if (State == PaintingState.Grabbing)
                return;

            if (_playerInZone)
            {
                ApplyState(PaintingState.Grabbing);
                Grabbed?.Invoke();
            }
        }

        private void ApplyState(PaintingState state)
        {
            State = state;

            switch (state)
            {
                case PaintingState.Safe:
                    if (calmSprite != null) _sprite.sprite = calmSprite;
                    _sprite.color = safeTint;
                    break;
                case PaintingState.Watching:
                    _sprite.color = watchingTint;
                    break;
                case PaintingState.Reaching:
                    if (damagedSprite != null) _sprite.sprite = damagedSprite;
                    _sprite.color = reachingTint;
                    break;
                case PaintingState.Grabbing:
                    if (damagedSprite != null) _sprite.sprite = damagedSprite;
                    _sprite.color = grabbingTint;
                    break;
            }

            StateChanged?.Invoke(state);
        }
    }
}
