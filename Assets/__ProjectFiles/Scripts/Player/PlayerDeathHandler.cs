using R3;
using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Reacts to <see cref="PlayerHealth.Died"/> by switching the player into a
    /// death state: movement and input are disabled and a single log is emitted.
    /// Kept separate from <see cref="PlayerHealth"/> so health stays pure state;
    /// the death screen and other death side effects are owned by systems that
    /// subscribe to the same <see cref="PlayerHealth.Died"/> stream directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        [Tooltip("The player's health. Required.")]
        [SerializeField] private PlayerHealth health;

        [Tooltip("Movement component to disable on death. Resolved from this GameObject if left empty.")]
        [SerializeField] private PlayerMotor motor;

        [Tooltip("Input component to disable on death. Resolved from this GameObject if left empty.")]
        [SerializeField] private MonoBehaviour inputReader;

        private CompositeDisposable _subs;

        private void OnEnable()
        {
            if (health == null)
            {
                Debug.LogError($"{nameof(PlayerDeathHandler)} on '{name}' requires a {nameof(PlayerHealth)} reference.", this);
                enabled = false;
                return;
            }

            // Resolve siblings from the same GameObject when not wired explicitly,
            // matching how PlayerMotor resolves its input source.
            motor ??= GetComponent<PlayerMotor>();
            inputReader ??= GetComponent<PlayerInputReader>();

            _subs = new CompositeDisposable();
            health.Died.Subscribe(OnDied).AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
        }

        private void OnDied(Unit _)
        {
            if (motor != null)
                motor.enabled = false;
            if (inputReader != null)
                inputReader.enabled = false;

            Debug.Log($"[PlayerDeathHandler] Player died on '{name}'.", this);
        }
    }
}
