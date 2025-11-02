using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class SplineAnimation : MonoBehaviour
    {
        [field: SerializeField] internal SplineContainer SplineContainer { get; private set; }
        [field: SerializeField] internal SplineData<quaternion> RotationData { get; private set; } = new();
        [field: SerializeField] internal AnimationCurve TimeCurve { get; private set; }
        [field: SerializeField] internal Transform PositionTarget { get; private set; }
        [field: SerializeField] internal Transform RotationTarget { get; private set; }

        private SplineAnimationConfig _animationConfig;
        private Vector3 _splineStartPoint;
        private Quaternion _transformRotation;
        private float _scale;

        internal async UniTask Animate(SplineAnimationConfig animationConfig)
        {
            _animationConfig = animationConfig;
            var initialPosition = _animationConfig.PositionTarget.position;
            var initialRotation = _animationConfig.RotationTarget.rotation;

            RotationData[0] = new DataPoint<quaternion>(0, initialRotation);

            CalculateTransformation();

            await Tween.Custom(this, 0f, 1f, 0.5f,
                onValueChange: static (self, currentTime) => self.UpdateTransformAlongPath(currentTime));

            _animationConfig.PositionTarget.position = initialPosition;
            _animationConfig.RotationTarget.rotation = initialRotation;
        }

        internal void PreviewAtProgress(float normalizedProgress)
        {
            var (position, rotation) = SampleSplineAt(normalizedProgress);
            ApplyTransformForPreview(position, rotation);
        }

        internal void PreviewAtKnot(int knotIndex)
        {
            var normalizedProgress =
                SplineContainer.Spline.ConvertIndexUnit(knotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            PreviewAtProgress(normalizedProgress);
        }

        private void UpdateTransformAlongPath(float currentTime)
        {
            var (splinePosition, splineRotation) = SampleSplineAt(currentTime);

            var offsetPoint = splinePosition - _splineStartPoint;

            var rotatedPoint = _transformRotation * offsetPoint;
            var scaledPoint = rotatedPoint * _scale;
            var finalPosition = _animationConfig.StartPoint + scaledPoint;

            ApplyTransformForAnimation(finalPosition, splineRotation);
        }

        private (Vector3 position, Quaternion rotation) SampleSplineAt(float normalizedProgress)
        {
            var splineProgress = TimeCurve.Evaluate(normalizedProgress);
            var position = (Vector3)SplineContainer.EvaluatePosition(splineProgress);
            var rotation = (Quaternion)RotationData.Evaluate(
                SplineContainer.Spline,
                splineProgress,
                PathIndexUnit.Normalized,
                new InterpolatedQuaternion());

            if (_animationConfig.InvertRotation is false)
                return (position, rotation);

            var localPosition = position - _splineStartPoint;
            localPosition.x = -localPosition.x;
            position = _splineStartPoint + localPosition;

            var mirrorAxis = Vector3.right;
            rotation = Quaternion.AngleAxis(180f, mirrorAxis) * rotation * Quaternion.AngleAxis(180f, mirrorAxis);

            return (position, rotation);
        }

        private void ApplyTransformForPreview(Vector3 position, Quaternion rotation)
        {
            PositionTarget.position = position;
            RotationTarget.rotation = rotation;
        }

        private void ApplyTransformForAnimation(Vector3 position, Quaternion rotation)
        {
            _animationConfig.PositionTarget.position = position;
            _animationConfig.RotationTarget.rotation = rotation;
        }

        private void CalculateTransformation()
        {
            _splineStartPoint = SplineContainer.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)SplineContainer.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - _splineStartPoint).normalized;
            var animationDirection = (_animationConfig.EndPoint - _animationConfig.StartPoint).normalized;

            var splineLength = Vector3.Distance(_splineStartPoint, splineEndPoint);
            var animationLength = Vector3.Distance(_animationConfig.StartPoint, _animationConfig.EndPoint);

            _scale = animationLength / splineLength;
            _transformRotation = Quaternion.FromToRotation(splineDirection, animationDirection);
        }
    }
}