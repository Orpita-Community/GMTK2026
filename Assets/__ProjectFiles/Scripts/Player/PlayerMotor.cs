using System;
using UnityEngine;

namespace Orpita.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Tooltip("Units per second at full input.")]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("Optional explicit input source. If left empty, an IMovementInput is resolved from this GameObject.")]
        [SerializeField] private MonoBehaviour inputSource;

        private Rigidbody2D _body;
        private IMovementInput _input;
        private Vector2 _facing = Vector2.right; // Defaulting to right for side-scroller

        public Vector2 Facing => _facing;
        public event Action<Vector2> FacingChanged;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            ConfigureBody();

            _input = inputSource as IMovementInput ?? GetComponent<IMovementInput>();
            if (_input == null)
            {
                Debug.LogError($"{nameof(PlayerMotor)} on '{name}' has no IMovementInput source.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            // 1. Only grab the horizontal (X) input (A/D or Left/Right arrows)
            float horizontalInput = _input.MoveDirection.x;
            
            // 2. Apply horizontal movement, but keep the current Y velocity so gravity still works if added later
            _body.linearVelocity = new Vector2(horizontalInput * moveSpeed, _body.linearVelocity.y);
            
            UpdateFacing(horizontalInput);
        }

        private void UpdateFacing(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) < 0.0001f)
                return;

            Vector2 next = horizontalInput > 0 ? Vector2.right : Vector2.left;
            
            if (Vector2.Dot(next, _facing) < 0.9999f)
            {
                _facing = next;
                FacingChanged?.Invoke(_facing);
            }
        }

        private void ConfigureBody()
        {
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}