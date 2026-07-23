using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Orpita.Candle;
using Orpita.Inventory;
using Orpita.Items;

namespace Orpita.UI
{
    /// <summary>
    /// Player resource HUD: candle fuel meter, carried key, and Map Fragment
    /// count. Pure presentation — it only subscribes to the reactive state on
    /// <see cref="PlayerInventory"/> and <see cref="CandleFuel"/>.
    /// </summary>
    public sealed class PlayerHudView : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private CandleFuel candle;

        [SerializeField] private Image candleFill;
        [SerializeField] private TMP_Text candleText;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private TMP_Text fragmentsText;

        private CompositeDisposable _subs;

        // Candle drains every frame; only rebuild the label when the displayed
        // tenth-of-a-second actually changes, to avoid a per-frame string alloc.
        private int _shownTenths = int.MinValue;

        private void OnEnable()
        {
            _subs = new CompositeDisposable();

            if (inventory != null)
            {
                inventory.CurrentKeyRx.Subscribe(OnKeyChanged).AddTo(_subs);
                inventory.MapFragmentsRx.Subscribe(OnFragmentsChanged).AddTo(_subs);
            }

            if (candle != null)
                candle.SecondsRemainingRx.Subscribe(OnCandleChanged).AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
            _shownTenths = int.MinValue;
        }

        private void OnKeyChanged(KeyDefinition key)
        {
            if (keyText != null)
                keyText.text = key != null ? $"Key: {key.DisplayName}" : "Key: —";
        }

        private void OnFragmentsChanged(int total)
        {
            if (fragmentsText != null)
                fragmentsText.text = $"Map Fragments: {total}";
        }

        private void OnCandleChanged(float seconds)
        {
            if (candleFill != null && candle != null && candle.MaxSeconds > 0f)
                candleFill.fillAmount = Mathf.Clamp01(seconds / candle.MaxSeconds);

            if (candleText == null)
                return;

            int tenths = Mathf.FloorToInt(seconds * 10f);
            if (tenths == _shownTenths)
                return;

            _shownTenths = tenths;
            candleText.text = $"Candle: {seconds:0.0}s";
        }
    }
}
