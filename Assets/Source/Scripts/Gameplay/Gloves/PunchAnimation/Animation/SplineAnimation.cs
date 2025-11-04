using Cysharp.Threading.Tasks;
using PrimeTween;
using R3;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation
{
    internal sealed class SplineAnimation : MonoBehaviour
    {
        [field: SerializeField] internal SplineContainer SplineContainer { get; private set; }
        [field: SerializeField] internal SplineData<quaternion> RotationData { get; private set; } = new();
        [field: SerializeField] internal TweenSettings PunchSettings { get; private set; }
        [field: SerializeField] internal TweenSettings ReturnSettings { get; private set; }
        [field: SerializeField] internal Vector3 PunchTargetOffset { get; private set; }
        [field: SerializeField] internal Transform PreviewTarget { get; private set; }

        internal Observable<Unit> OnPunchFinished => _punchFinished;
        private readonly Subject<Unit> _punchFinished = new();

        private SplineTransformation _transformation;

        private Transform _target;
        private Vector3 _startPoint;
        private Vector3 _endPoint;

        internal async UniTask Animate(Transform target, Vector3 startPoint, Vector3 endPoint)
        {
            var initialLocalPosition = target.localPosition;
            var initialLocalRotation = target.localRotation;

            _target = target;
            _startPoint = startPoint;
            _endPoint = endPoint;

            _transformation = CalculateTransformation();

            await Tween.Custom(
                this,
                0f,
                1f,
                PunchSettings,
                onValueChange: static (state, currentTime) => state.UpdateTransformAlongPath(currentTime));

            _punchFinished.OnNext(Unit.Default);

            await Sequence.Create()
                .Chain(Tween.LocalPosition(_target, initialLocalPosition, ReturnSettings))
                .Group(Tween.LocalRotation(_target, initialLocalRotation, ReturnSettings));
        }

        internal (Vector3 position, Quaternion rotation) EvaluateSplineTransform(float normalizedProgress)
        {
            var position = (Vector3)SplineContainer.EvaluatePosition(normalizedProgress);
            var rotation = (Quaternion)RotationData.Evaluate(
                SplineContainer.Spline,
                normalizedProgress,
                PathIndexUnit.Normalized,
                new InterpolatedQuaternion());

            return (position, rotation);
        }

        private void UpdateTransformAlongPath(float currentTime)
        {
            var (splinePosition, splineRotation) = EvaluateSplineTransform(currentTime);
            var finalPosition = TransformPosition(splinePosition);

            _target.position = finalPosition;
            _target.rotation = splineRotation;
        }

        private Vector3 TransformPosition(Vector3 splinePosition)
        {
            var offsetPoint = splinePosition - _transformation.SplineStartPoint;
            var rotatedPoint = _transformation.Rotation * offsetPoint;
            var scaledPoint = rotatedPoint * _transformation.Scale;
            return _startPoint + scaledPoint;
        }

        private SplineTransformation CalculateTransformation()
        {
            var splineStartPoint = (Vector3)SplineContainer.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)SplineContainer.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - splineStartPoint).normalized;
            var animationDirection = (_endPoint - _startPoint).normalized;

            var splineLength = Vector3.Distance(splineStartPoint, splineEndPoint);
            var animationLength = Vector3.Distance(_startPoint, _endPoint);

            var scale = animationLength / splineLength;
            var rotation = Quaternion.FromToRotation(splineDirection, animationDirection);

            return new SplineTransformation(splineStartPoint, rotation, scale);
        }
    }
}