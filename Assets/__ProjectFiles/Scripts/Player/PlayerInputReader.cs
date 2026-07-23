using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Reads the Player/Move action from the generated Input System asset and
    /// exposes it through <see cref="IMovementInput"/>. Owns the lifetime of the
    /// action collection so nothing else has to know about Input System plumbing.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour, IMovementInput
    {
        private InputSystem_Actions _actions;

        public Vector2 MoveDirection { get; private set; }

        private void Awake()
        {
            _actions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _actions.Player.Enable();
            _actions.Player.Move.performed += OnMove;
            _actions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _actions.Player.Move.performed -= OnMove;
            _actions.Player.Move.canceled -= OnMove;
            _actions.Player.Disable();
            MoveDirection = Vector2.zero;
        }

        private void OnDestroy()
        {
            _actions?.Dispose();
        }

        private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            MoveDirection = ctx.ReadValue<Vector2>();
        }
    }
}
