using System;
using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Physics-based top-down movement. Consumes an <see cref="IMovementInput"/>
    /// and drives a Rigidbody2D so collision against walls, furniture and doors
    /// is resolved by the physics engine. Movement is frame-rate independent
    /// (velocity is applied through the fixed-step physics loop) and diagonal
    /// speed is normalized so it never exceeds cardinal speed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Tooltip("Units per second at full input.")]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("Optional explicit input source. If left empty, an IMovementInput is resolved from this GameObject.")]
        [SerializeField] private MonoBehaviour inputSource;

        private Rigidbody2D _body;
        private IMovementInput _input;
        private Vector2 _facing = Vector2.down;

        /// <summary>
        /// Last non-zero movement direction (normalized). Intended for driving
        /// animation/aiming; updated only while the player is actually moving.
        /// </summary>
        public Vector2 Facing => _facing;

        /// <summary>Raised when the facing direction changes to a new octant.</summary>
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
            Vector2 direction = Vector2.ClampMagnitude(_input.MoveDirection, 1f);
            _body.linearVelocity = direction * moveSpeed;
            UpdateFacing(direction);
        }

        private void UpdateFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Vector2 next = direction.normalized;
            if (Vector2.Dot(next, _facing) > 0.9999f)
                return;

            _facing = next;
            FacingChanged?.Invoke(_facing);
        }

        private void ConfigureBody()
        {
            // Top-down: no gravity, physics-driven movement, and the body must
            // not tumble from collision torque.
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
