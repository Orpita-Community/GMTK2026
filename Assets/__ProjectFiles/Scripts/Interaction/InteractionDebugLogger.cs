using System;
using R3;
using UnityEngine;
using Orpita.Inventory;
using Orpita.Items;

namespace Orpita.Interaction
{
    /// <summary>
    /// Development-only diagnostics: subscribes to every interaction signal
    /// (manager events, channel progress, inventory changes, and each
    /// interactable's own outcome event) and prints them to the Unity console.
    /// Attach to the Player; toggle <see cref="enableLogging"/> or remove for release.
    /// </summary>
    public sealed class InteractionDebugLogger : MonoBehaviour
    {
        private const string Tag = "[Interact] ";

        [SerializeField] private bool enableLogging = true;

        private InteractionManager _manager;
        private PlayerInventory _inventory;

        private CompositeDisposable _subs;
        private bool _channeling;

        private void Awake()
        {
            _manager = GetComponent<InteractionManager>();
            _inventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (!enableLogging)
                return;

            _subs = new CompositeDisposable();

            if (_manager != null)
            {
                _manager.CurrentTarget.Subscribe(OnTargetChanged).AddTo(_subs);
                _manager.ChannelProgress.Subscribe(OnProgress).AddTo(_subs);
                _manager.InteractionCompleted += OnCompleted;
                _manager.InteractionCanceled += OnCanceled;
                _manager.InteractionBlocked += OnBlocked;
            }

            if (_inventory != null)
            {
                _inventory.KeyPickedUp += OnKeyPickedUp;
                _inventory.KeyPickupBlocked += OnKeyBlocked;
                _inventory.KeyConsumed += OnKeyConsumed;
                _inventory.MapFragmentAdded += OnFragment;
            }

            HookInteractableOutcomes();

            Log("Logger ready. Walk up to an object and hold E.");
        }

        private void OnDisable()
        {
            if (_manager != null)
            {
                _manager.InteractionCompleted -= OnCompleted;
                _manager.InteractionCanceled -= OnCanceled;
                _manager.InteractionBlocked -= OnBlocked;
            }

            if (_inventory != null)
            {
                _inventory.KeyPickedUp -= OnKeyPickedUp;
                _inventory.KeyPickupBlocked -= OnKeyBlocked;
                _inventory.KeyConsumed -= OnKeyConsumed;
                _inventory.MapFragmentAdded -= OnFragment;
            }

            _subs?.Dispose();
            _subs = null;
            _channeling = false;
        }

        // --- manager ---
        private void OnTargetChanged(IInteractable target)
        {
            Log(target == null
                ? "Target: none"
                : $"Target: {target.Transform.name} ({target.Prompt})");
        }

        private void OnProgress(float progress)
        {
            if (progress > 0f && !_channeling)
            {
                _channeling = true;
                string name = _manager.CurrentTarget.CurrentValue?.Transform.name ?? "?";
                Log($"Channeling {name}...");
            }
            else if (progress <= 0f)
            {
                _channeling = false;
            }
        }

        private void OnCompleted(IInteractable t)
        {
            _channeling = false;
            Log($"COMPLETED: {t.Transform.name} ({t.Prompt})");
        }

        private void OnCanceled(IInteractable t)
        {
            _channeling = false;
            Log($"CANCELED: {(t != null ? t.Transform.name : "?")} (moved / released / interrupted)");
        }

        private void OnBlocked(IInteractable t)
        {
            Log($"BLOCKED: {(t != null ? t.Transform.name + " (" + t.Prompt + ")" : "?")} " +
                "(moving, i-frames, locked, or already used)");
        }

        // --- inventory ---
        private void OnKeyPickedUp(KeyDefinition k) => Log($"KEY picked up: {Name(k)}");
        private void OnKeyBlocked(KeyDefinition k) => Log($"KEY pickup BLOCKED (hands full): {Name(k)}");
        private void OnKeyConsumed(KeyDefinition k) => Log($"KEY consumed: {Name(k)}");
        private void OnFragment(int total) => Log($"MAP FRAGMENT collected. Total = {total}");

        // --- per-interactable outcomes (closures need explicit unsubscribe) ---
        private void HookInteractableOutcomes()
        {
            foreach (SearchableObject s in FindObjectsByType<SearchableObject>(FindObjectsSortMode.None))
            {
                SearchableObject local = s;
                Action<LootReward> h = r => OnSearched(local, r);
                local.Searched += h;
                _subs.Add(Disposable.Create(() => local.Searched -= h));
            }

            foreach (KeyChest c in FindObjectsByType<KeyChest>(FindObjectsSortMode.None))
            {
                KeyChest local = c;
                Action h = () => Log($"CHEST opened: {local.name}");
                local.Opened += h;
                _subs.Add(Disposable.Create(() => local.Opened -= h));
            }

            foreach (CandleRefillStation st in FindObjectsByType<CandleRefillStation>(FindObjectsSortMode.None))
            {
                CandleRefillStation local = st;
                Action h = () => Log($"CANDLE refilled at {local.name}");
                local.Refilled += h;
                _subs.Add(Disposable.Create(() => local.Refilled -= h));
            }
        }

        private void OnSearched(SearchableObject source, LootReward reward)
        {
            Log($"SEARCHED {source.name} -> {Describe(reward)}");
        }

        // --- helpers ---
        private static string Describe(LootReward r)
        {
            switch (r.Kind)
            {
                case LootKind.Key: return $"KEY ({Name(r.Key)})";
                case LootKind.Candle: return $"CANDLE +{r.CandleSeconds:0}s";
                default: return "nothing";
            }
        }

        private static string Name(KeyDefinition k) => k != null ? k.DisplayName : "null";

        private void Log(string message)
        {
            Debug.Log(Tag + message, this);
        }
    }
}
