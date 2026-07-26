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

        [Tooltip("The spot light carried by the player.")]
        [SerializeField] private Light2D playerLight;

        [Tooltip("Amount removed from the light falloff and, after the threshold, intensity each second.")]
        [SerializeField, Min(0.001f)] private float decreasePerSecond = 0.01f;

        [Tooltip("Intensity begins draining when falloff reaches this value.")]
        [SerializeField, Range(0f, 1f)] private float intensityDrainThreshold = 0.5f;

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
                while (playerLight.pointLightInnerRadius > 0f)
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
            playerLight.falloffIntensity = Mathf.Max(0f, playerLight.falloffIntensity - decreasePerSecond);

            if (playerLight.falloffIntensity <= intensityDrainThreshold)
                playerLight.intensity = Mathf.Max(0f, playerLight.intensity - decreasePerSecond);
        }
    }
}
