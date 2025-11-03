using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.CameraEffects
{
    internal sealed class CameraShake : MonoBehaviour, ICameraShake
    {
        [SerializeField] private ShakeSettings _shakeSettings;

        private Tween _shakeTween;

        public void Shake(float intensity)
        {
            _shakeTween.Stop();

            var initialStrength = _shakeSettings.strength;
            _shakeSettings.strength *= intensity;

            _shakeTween = Tween.ShakeLocalPosition(transform, _shakeSettings);
            _shakeSettings.strength = initialStrength;
        }
    }
}