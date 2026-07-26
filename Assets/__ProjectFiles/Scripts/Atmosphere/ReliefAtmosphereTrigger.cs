using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using Orpita.Player;

namespace Orpita.Atmosphere
{
    /// <summary>
    /// Blends in a "relief" post-processing <see cref="Volume"/> when the player
    /// crosses back into the entrance/front-door area — the distinct atmosphere
    /// change for finally reaching the escape route. Sibling in spirit to
    /// <see cref="CandleAtmosphereDriver"/> (same Volume-weight-blend idea), but
    /// triggered by the player entering a zone rather than by candle fuel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ReliefAtmosphereTrigger : MonoBehaviour
    {
        [Tooltip("The relief-look Volume. Its weight is blended in on entry.")]
        [SerializeField] private Volume reliefVolume;

        [Tooltip("Blend-in duration in seconds.")]
        [SerializeField] private float blendSeconds = 1.5f;

        private MotionHandle _blendHandle;
        private bool _triggered;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            if (reliefVolume != null)
                reliefVolume.weight = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered || reliefVolume == null)
                return;

            if (other.GetComponentInParent<PlayerMotor>() == null)
                return;

            _triggered = true;

            _blendHandle.TryCancel();
            _blendHandle = LMotion.Create(reliefVolume.weight, 1f, blendSeconds)
                .Bind(this, (w, self) => self.reliefVolume.weight = w);
        }

        private void OnDestroy()
        {
            _blendHandle.TryCancel();
        }
    }
}
