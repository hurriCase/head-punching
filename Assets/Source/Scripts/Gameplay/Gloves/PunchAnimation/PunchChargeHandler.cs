using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves.PunchAnimation
{
    internal sealed class PunchChargeHandler : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private ShakeSettings _shakeSettings;
        [SerializeField] private float _endScale;
        [SerializeField] private AnimationCurve _chargeCurve;

        private Sequence _scaleTween;
        private float _chargeStartTime;

        internal void StartCharge()
        {
            _scaleTween.Complete();

            _chargeStartTime = Time.time;

            var duration = _chargeCurve.keys[^1].time;
            _shakeSettings.duration = duration;
            _scaleTween = Sequence.Create()
                .Chain(Tween.Scale(_visualTransform, _endScale, duration))
                .Group(Tween.ShakeLocalPosition(_visualTransform, _shakeSettings));
        }

        internal float ReleaseCharge()
        {
            _scaleTween.Stop();
            _visualTransform.localScale = Vector3.one;

            var chargeDuration = Time.time - _chargeStartTime;
            return _chargeCurve.Evaluate(chargeDuration);
        }
    }
}