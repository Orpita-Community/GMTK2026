using System;
using R3;
using UnityEngine;
using Orpita.Player;
using Orpita.Inventory;
using Orpita.Candle;

namespace Orpita.Interaction
{
    /// <summary>
    /// The interaction brain. Resolves player dependencies, owns the stationary
    /// channel (hold-E near an interactable), tracks the current proximity target,
    /// and enforces the interaction rules: no interacting while moving or during
    /// damage i-frames, per-interactable channel durations, and a post-interaction
    /// cooldown to prevent spam. Fully event/reactive driven — no input polling in
    /// <see cref="Update"/> (only spatial nearest-upkeep and channel progression).
    /// </summary>
    [RequireComponent(typeof(InteractionDetector))]
    public sealed class InteractionManager : MonoBehaviour
    {
        [Tooltip("Move-input magnitude above this counts as 'moving' and blocks interaction.")]
        [SerializeField] private float moveThreshold = 0.05f;

        [Tooltip("Cooldown after a completed interaction, in seconds.")]
        [SerializeField] private float cooldownSeconds = 0.35f;

        private IInteractionInput _interactionInput;
        private IMovementInput _movementInput;
        private InteractionDetector _detector;
        private PlayerInventory _inventory;
        private ICandle _candle;   // optional — null when the player has no candle
        private IDamageState _damage; // optional — null treated as never invulnerable

        private InteractionContext _context;

        private readonly ReactiveProperty<float> _channelProgress = new ReactiveProperty<float>(0f);
        private readonly ReactiveProperty<IInteractable> _currentTarget = new ReactiveProperty<IInteractable>(null);

        private IInteractable _channelTarget;
        private float _channelTimer;
        private bool _channeling;
        private float _cooldownUntil;

        /// <summary>Channel completion progress, 0..1. Read-only reactive view.</summary>
        public ReadOnlyReactiveProperty<float> ChannelProgress => _channelProgress;

        /// <summary>The interactable currently highlighted as the proximity target (may be null).</summary>
        public ReadOnlyReactiveProperty<IInteractable> CurrentTarget => _currentTarget;

        /// <summary>Raised when an interaction channel completes and OnInteract fires.</summary>
        public event Action<IInteractable> InteractionCompleted;

        /// <summary>Raised when an in-progress channel is canceled.</summary>
        public event Action<IInteractable> InteractionCanceled;

        /// <summary>Raised when an interaction is blocked (moving, i-frames, or CanInteract false).</summary>
        public event Action<IInteractable> InteractionBlocked;

        // True while the player's move input exceeds the stationary threshold.
        private bool IsMoving => _movementInput.MoveDirection.sqrMagnitude > moveThreshold * moveThreshold;

        // True while the player is in damage invulnerability frames (never if no damage source).
        private bool IsInvulnerable => _damage != null && _damage.IsInvulnerable;

        private void Awake()
        {
            _interactionInput = GetComponent<IInteractionInput>();
            _movementInput = GetComponent<IMovementInput>();
            _detector = GetComponent<InteractionDetector>();
            _inventory = GetComponent<PlayerInventory>();
            _candle = GetComponentInChildren<ICandle>();
            _damage = GetComponent<IDamageState>();

            if (_interactionInput == null || _movementInput == null || _detector == null || _inventory == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionManager)} on '{name}' is missing required dependencies " +
                    $"(needs IInteractionInput, IMovementInput, InteractionDetector and PlayerInventory on this GameObject).",
                    this);
                enabled = false;
            }
        }

        private void Start()
        {
            // Build once: refs on the player GameObject are stable for the scene.
            _context = new InteractionContext(gameObject, _inventory, _candle);
        }

        private void OnEnable()
        {
            if (_interactionInput == null)
                return;

            _interactionInput.InteractStarted += OnInteractStarted;
            _interactionInput.InteractCanceled += OnInteractCanceled;
        }

        private void OnDisable()
        {
            if (_interactionInput == null)
                return;

            _interactionInput.InteractStarted -= OnInteractStarted;
            _interactionInput.InteractCanceled -= OnInteractCanceled;

            CancelChannel();
        }

        private void Update()
        {
            UpdateCurrentTarget();

            if (_channeling)
                TickChannel();
        }

        private void OnDestroy()
        {
            _channelProgress?.Dispose();
            _currentTarget?.Dispose();
        }

        private void UpdateCurrentTarget()
        {
            IInteractable nearest = _detector.GetNearest(transform.position);

            if (_channeling)
            {
                // Keep the channel target highlighted regardless of spatial drift.
                if (_currentTarget.Value != _channelTarget)
                    _currentTarget.Value = _channelTarget;
                return;
            }

            if (nearest != _currentTarget.Value)
            {
                _currentTarget.Value?.SetHighlighted(false);
                _currentTarget.Value = nearest;
                nearest?.SetHighlighted(true);
            }
        }

        private void TickChannel()
        {
            if (_channelTarget == null || _channelTarget.Transform == null)
            {
                CancelChannel();
                return;
            }

            if (!_interactionInput.InteractHeld || IsMoving || IsInvulnerable || !_channelTarget.CanInteract(_context))
            {
                CancelChannel();
                return;
            }

            _channelTimer += Time.deltaTime;

            float duration = _channelTarget.InteractionDuration;
            _channelProgress.Value = duration > 0f ? Mathf.Clamp01(_channelTimer / duration) : 1f;

            if (_channelTimer >= duration)
                CompleteInteraction(_channelTarget);
        }

        private void OnInteractStarted()
        {
            TryBeginInteraction();
        }

        private void OnInteractCanceled()
        {
            CancelChannel();
        }

        private void TryBeginInteraction()
        {
            if (Time.time < _cooldownUntil || _channeling)
                return;

            IInteractable target = _detector.GetNearest(transform.position);
            if (target == null)
                return;

            if (IsMoving || IsInvulnerable || !target.CanInteract(_context))
            {
                InteractionBlocked?.Invoke(target);
                return;
            }

            if (target.InteractionDuration <= 0f)
            {
                CompleteInteraction(target);
                return;
            }

            _channelTarget = target;
            _channelTimer = 0f;
            _channeling = true;
            _channelProgress.Value = 0f;
        }

        private void CompleteInteraction(IInteractable target)
        {
            target.OnInteract(_context);
            _cooldownUntil = Time.time + cooldownSeconds;

            ClearChannelState();

            InteractionCompleted?.Invoke(target);
        }

        private void CancelChannel()
        {
            if (!_channeling)
                return;

            IInteractable target = _channelTarget;
            ClearChannelState();
            InteractionCanceled?.Invoke(target);
        }

        private void ClearChannelState()
        {
            _channeling = false;
            _channelTarget = null;
            _channelTimer = 0f;
            _channelProgress.Value = 0f;
        }
    }
}
