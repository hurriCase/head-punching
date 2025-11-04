using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.ImpactEffects
{
    internal sealed class CameraShake : ImpactEffectBase
    {
        [SerializeField] private ShakeSettings _shakeSettings;

        private Tween _shakeTween;

        internal override void Play(float intensity)
        {
            _shakeTween.Stop();

            var initialStrength = _shakeSettings.strength;
            _shakeSettings.strength *= intensity;

            _shakeTween = Tween.ShakeLocalPosition(transform, _shakeSettings);
            _shakeSettings.strength = initialStrength;
        }
    }
}