using Orpita.Candle;
using R3;
using UnityEngine;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Blends a post-processing volume in as the candle burns down. The look
    /// itself lives in the volume profile; this component only moves
    /// <see cref="VolumeBlendDriver.Weight"/>.
    ///
    /// <para>
    /// The blend window is in absolute seconds rather than a fraction of
    /// <c>maxSeconds</c> so the same component can be dropped onto two volume
    /// layers with two different windows and still mean what the design doc
    /// says — one instance drives the low-fuel grade (15s → 1s), a second
    /// drives the near-death grade (3s → 0s). A normalized threshold would
    /// couple every layer to the candle's tank size and drift whenever
    /// <c>maxSeconds</c> was retuned.
    /// </para>
    /// </summary>
    public sealed class CandleAtmosphereDriver : VolumeBlendDriver
    {
        [Tooltip("Candle whose remaining fuel drives the blend.")]
        [SerializeField] private CandleFuel candle;

        [Tooltip("Seconds of fuel remaining at which this look starts blending in.")]
        [SerializeField, Min(0f)] private float blendStartSeconds = 15f;

        [Tooltip("Seconds of fuel remaining at which this look reaches full blend.")]
        [SerializeField, Min(0f)] private float fullBlendSeconds = 1f;

        [Tooltip("Blend weight reached when the candle is fully spent.")]
        [SerializeField, Range(0f, 1f)] private float maxWeight = 1f;

        protected override bool TryResolveDependencies()
            => RequireReference(candle, nameof(CandleFuel));

        protected override void SubscribeDriver(CompositeDisposable subscriptions)
            => candle.SecondsRemainingRx.Subscribe(OnFuelChanged).AddTo(subscriptions);

        private void OnFuelChanged(float seconds)
        {
            // Linear divisor goes non-positive here. Silent hard switch, not a
            // log: fuel updates every frame and we don't want console spam.
            if (blendStartSeconds <= fullBlendSeconds)
            {
                Weight = seconds <= fullBlendSeconds ? maxWeight : 0f;
                return;
            }

            float t = (blendStartSeconds - seconds) / (blendStartSeconds - fullBlendSeconds);
            Weight = Mathf.Clamp01(t) * maxWeight;
        }
    }
}
