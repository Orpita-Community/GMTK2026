using R3;
using TMPro;
using UnityEngine;
using Orpita.Player;

namespace Orpita.UI
{
    /// <summary>
    /// Placeholder health readout in the style of <see cref="PlayerHudView"/>:
    /// subscribes to <see cref="PlayerHealth.CurrentHealthRx"/> and renders a
    /// heart string (♥ for remaining, ♡ for lost). Pure presentation — no
    /// gameplay logic. The string rebuilds only when the value changes (i.e. on
    /// damage), never per frame.
    /// </summary>
    public sealed class PlayerHealthView : MonoBehaviour
    {
        [Tooltip("Health state to render.")]
        [SerializeField] private PlayerHealth health;

        [Tooltip("Text element for the heart readout. If left empty the view does nothing.")]
        [SerializeField] private TMP_Text text;

        private CompositeDisposable _subs;

        private void OnEnable()
        {
            if (health == null)
            {
                enabled = false;
                return;
            }

            _subs = new CompositeDisposable();
            health.CurrentHealthRx.Subscribe(OnHealthChanged).AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
        }

        private void OnHealthChanged(int current)
        {
            if (text == null)
                return;

            text.text = BuildHearts(current, health.MaxHealth);
        }

        private static string BuildHearts(int current, int max)
        {
            // Pool is tiny (max ~3); a single char[] -> string is the cheapest
            // non-per-frame allocation and only runs on damage.
            var chars = new char[max];
            for (int i = 0; i < max; i++)
                chars[i] = i < current ? '♥' : '♡';
            return new string(chars);
        }
    }
}
