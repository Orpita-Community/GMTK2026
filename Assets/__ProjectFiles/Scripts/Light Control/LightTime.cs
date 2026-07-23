using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Orpita.Lighting
{
    /// <summary>
    /// Gradually drains the player's 2D light once per game second.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LightTime : MonoBehaviour
    {
        private const float TickIntervalSeconds = 1f;

        [Tooltip("The Spot or Point light carried by the player.")]
        [SerializeField] private Light2D playerLight;

        [Tooltip("Amount removed from Inner Radius each second.")]
        [SerializeField, Min(0.001f)] private float innerRadiusDecreasePerSecond = 0.1f;

        [Tooltip("Inner Radius at which intensity starts draining.")]
        [SerializeField, Min(0f)] private float intensityDrainThreshold = 2.5f;

        [Tooltip("Amount removed from intensity each second after Inner Radius reaches the threshold.")]
        [SerializeField, Min(0.001f)] private float intensityDecreasePerSecond = 0.05f;

        private CancellationTokenSource _lifetimeCancellation;

        private void Awake()
        {
            playerLight ??= GetComponent<Light2D>();

            if (playerLight == null)
            {
                Debug.LogError($"{nameof(LightTime)} on '{name}' requires a {nameof(Light2D)} reference.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (playerLight == null)
                return;

            _lifetimeCancellation = new CancellationTokenSource();
            DrainLightAsync(_lifetimeCancellation.Token);
        }

        private void OnDisable()
        {
            _lifetimeCancellation?.Cancel();
            _lifetimeCancellation?.Dispose();
            _lifetimeCancellation = null;
        }

        private async void DrainLightAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (playerLight.pointLightInnerRadius > 0f || playerLight.intensity > 0f)
                {
                    await Awaitable.WaitForSecondsAsync(TickIntervalSeconds, cancellationToken);
                    DrainOneTick();
                }
            }
            catch (OperationCanceledException)
            {
                // The component was disabled or destroyed while waiting.
            }
        }

        private void DrainOneTick()
        {
            playerLight.pointLightInnerRadius = Mathf.Max(
                0f,
                playerLight.pointLightInnerRadius - innerRadiusDecreasePerSecond);

            if (playerLight.pointLightInnerRadius <= intensityDrainThreshold)
                playerLight.intensity = Mathf.Max(0f, playerLight.intensity - intensityDecreasePerSecond);
        }
    }
}
