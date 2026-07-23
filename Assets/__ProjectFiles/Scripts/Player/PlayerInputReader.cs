using System;
using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Reads the Player map from the generated Input System asset and exposes it
    /// through <see cref="IMovementInput"/> (Move) and <see cref="IInteractionInput"/>
    /// (Interact). Owns the lifetime of the action collection so nothing else has
    /// to know about Input System plumbing.
    /// <para>
    /// Interaction is driven from the Interact action's started/canceled events: we
    /// track the held flag ourselves and surface press/release as plain events,
    /// letting the <c>InteractionManager</c> own the stationary channel timing.
    /// </para>
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour, IMovementInput, IInteractionInput
    {
        private InputSystem_Actions _actions;

        public Vector2 MoveDirection { get; private set; }

        public bool InteractHeld { get; private set; }

        public event Action InteractStarted;
        public event Action InteractCanceled;

        private void Awake()
        {
            _actions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            // On an edit-mode domain reload Unity can call OnEnable before Awake,
            // so ensure the action collection exists before touching it.
            _actions ??= new InputSystem_Actions();

            _actions.Player.Enable();

            _actions.Player.Move.performed += OnMove;
            _actions.Player.Move.canceled += OnMove;

            _actions.Player.Interact.started += OnInteractStarted;
            _actions.Player.Interact.canceled += OnInteractCanceled;
        }

        private void OnDisable()
        {
            if (_actions == null)
                return;

            _actions.Player.Move.performed -= OnMove;
            _actions.Player.Move.canceled -= OnMove;

            _actions.Player.Interact.started -= OnInteractStarted;
            _actions.Player.Interact.canceled -= OnInteractCanceled;

            _actions.Player.Disable();

            MoveDirection = Vector2.zero;
            InteractHeld = false;
        }

        private void OnDestroy()
        {
            _actions?.Dispose();
        }

        private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            MoveDirection = ctx.ReadValue<Vector2>();
        }

        private void OnInteractStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            InteractHeld = true;
            InteractStarted?.Invoke();
        }

        private void OnInteractCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            InteractHeld = false;
            InteractCanceled?.Invoke();
        }
    }
}
