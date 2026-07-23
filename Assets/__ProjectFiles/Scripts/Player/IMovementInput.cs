using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Abstraction over the source of movement input.
    /// Lets the motor stay decoupled from the concrete Input System wiring
    /// and enables substitution (AI, replay, tests) without touching movement.
    /// </summary>
    public interface IMovementInput
    {
        /// <summary>
        /// Desired move direction this frame. Components (x, y) are in the
        /// range [-1, 1]. Not guaranteed normalized (diagonals have magnitude
        /// up to ~1.41); the consumer is responsible for normalizing.
        /// </summary>
        Vector2 MoveDirection { get; }
    }
}
