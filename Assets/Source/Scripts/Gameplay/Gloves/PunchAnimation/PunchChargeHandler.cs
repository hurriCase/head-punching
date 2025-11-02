using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves.PunchAnimation
{
    internal sealed class PunchChargeHandler : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _endScale;
        [SerializeField] private AnimationCurve _chargeCurve;

        private Tween _scaleTween;
        private float _chargeStartTime;

        internal void StartCharge()
        {
            _scaleTween.Complete();

            _chargeStartTime = Time.time;

            _scaleTween = Tween.Scale(_visualTransform, _endScale, _chargeCurve.keys[^1].time);
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