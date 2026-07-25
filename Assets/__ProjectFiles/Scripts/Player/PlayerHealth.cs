using System;
using System.Threading;
using R3;
using UnityEngine;

namespace Orpita.Player
{
    /// <summary>
    /// Owns the player's hit-point pool, damage application, the invulnerability
    /// (i-frame) window that follows a hit, and the death flag. Pure state — death
    /// side effects (disabling movement, death screen, etc.) are owned by separate
    /// components that subscribe to <see cref="Died"/>. Implements
    /// <see cref="IDamageState"/> so <see cref="Orpita.Interaction.InteractionManager"/>
    /// can suppress interaction during i-frames without depending on a concrete
    /// health implementation.
    /// <para>
    /// The i-frame window is driven by <see cref="Awaitable.WaitForSecondsAsync"/>
    /// over a component-lifetime <see cref="CancellationToken"/>, following the
    /// <c>LightTime</c> house pattern (no coroutine, no Update timer). Damage never
    /// touches the candle — health and candle fuel are independent systems.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageState
    {
        [Tooltip("Maximum hit points. Reaching zero kills the player.")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Tooltip("Seconds of invulnerability granted after a hit. Stops overlapping sources from double-dipping in the same frame.")]
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 1.5f;

        private ReactiveProperty<int> _health;

        // Lazily created, mirroring CandleFuel: a view on another GameObject may
        // subscribe from its OnEnable before this component's Awake runs (cross-GO
        // Awake/OnEnable order is undefined). Seeded with maxHealth so the earliest
        // subscriber observes the starting value.
        private ReactiveProperty<int> Health => _health ??= new ReactiveProperty<int>(maxHealth);

        // Fire-and-forget event streams. Created eagerly as readonly fields so
        // subscribers can attach before Awake; Subject<T> is-an Observable<T>, so
        // exposing them directly satisfies the (frozen) public surface.
        private readonly Subject<int> _damageTaken = new();
        private readonly Subject<Unit> _died = new();

        private CancellationTokenSource _lifetimeCts;
        private bool _isInvulnerable;
        private bool _isDead;

        /// <summary>Current hit points (clamped to [0, <see cref="MaxHealth"/>]) as a reactive stream.</summary>
        public ReadOnlyReactiveProperty<int> CurrentHealthRx => Health;

        public int MaxHealth => maxHealth;

        /// <summary>Length of the i-frame window in seconds. Read-only so visual systems (e.g. InvulnerabilityFlicker) stay in sync with the logic.</summary>
        public float InvulnerabilitySeconds => invulnerabilitySeconds;

        /// <inheritdoc/>
        public bool IsInvulnerable => _isInvulnerable;

        public bool IsDead => _isDead;

        /// <summary>Emits the damage amount actually applied on every hit that lands. A killing blow emits this <em>first</em>, then <see cref="Died"/>.</summary>
        public Observable<int> DamageTaken => _damageTaken;

        /// <summary>Emits exactly once when health reaches zero.</summary>
        public Observable<Unit> Died => _died;

        private void OnEnable()
        {
            // Token source lives for the enabled lifetime of the component, matching
            // LightTime.cs. Recreated on each enable so a disabled-then-enabled
            // player starts with a clean cancellation handle.
            _lifetimeCts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            // Cancel any in-flight i-frame window and release the token. The
            // invulnerable flag is reset so the interaction system never observes a
            // permanently-invulnerable player after disable.
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = null;
            _isInvulnerable = false;
        }

        private void OnDestroy()
        {
            _health?.Dispose();
            _damageTaken.Dispose();
            _died.Dispose();
        }

        /// <summary>
        /// Apply <paramref name="amount"/> damage if able. Ignored entirely when
        /// <paramref name="amount"/> &lt;= 0, while <see cref="IsInvulnerable"/>, or
        /// once <see cref="IsDead"/> — those guards are precisely what stop two
        /// overlapping ghosts in the same frame from double-dipping.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || _isInvulnerable || _isDead)
                return;

            int next = Mathf.Max(0, Health.Value - amount);
            Health.Value = next;

            // Damage event fires before the death event (acceptance criterion: a
            // killing blow emits both, damage first).
            _damageTaken.OnNext(amount);

            if (next == 0)
            {
                _isDead = true;
                _died.OnNext(Unit.Default);
                // No i-frame window on a killing blow: the player is dead and
                // further calls are blocked by _isDead, so there is no double-dip
                // risk to guard against and no point launching an async window.
                return;
            }

            StartInvulnerabilityWindow();
        }

        private void StartInvulnerabilityWindow()
        {
            if (_lifetimeCts == null)
                return; // Component not enabled (being torn down) — nothing to time.

            // Synchronously claim invulnerability BEFORE the await so a second
            // source hitting later this same frame sees the flag and bails out in
            // TakeDamage's guard.
            _isInvulnerable = true;
            RunInvulnerabilityWindowAsync(_lifetimeCts.Token);
        }

        // async void is the sanctioned house pattern (see LightTime): the only
        // legitimate use is fire-and-forget event-driven async with a lifetime
        // CancellationToken and an OperationCanceledException guard. Never touch
        // component state after cancellation except via OnDisable's reset.
        private async void RunInvulnerabilityWindowAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(invulnerabilitySeconds, cancellationToken);
                _isInvulnerable = false;
            }
            catch (OperationCanceledException)
            {
                // Component disabled/destroyed mid-window, or play mode exited.
                // The flag is reset in OnDisable; nothing to do here.
            }
        }
    }
}
