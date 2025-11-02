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
        [field: SerializeField] internal TweenSettings AnimationSettings { get; private set; }
        [field: SerializeField] internal Transform PositionTarget { get; private set; }
        [field: SerializeField] internal Transform RotationTarget { get; private set; }

        private SplineAnimationConfig _config;
        private SplineTransformation _transformation;

        internal async UniTask Animate(SplineAnimationConfig config)
        {
            var initialPosition = config.PositionTarget.position;
            var initialRotation = config.RotationTarget.rotation;

            _config = config;
            _transformation = CalculateTransformation();

            await Tween.Custom(
                this,
                0f,
                1f,
                AnimationSettings,
                onValueChange: static (state, currentTime) => state.UpdateTransformAlongPath(currentTime));

            config.PositionTarget.position = initialPosition;
            config.RotationTarget.rotation = initialRotation;
        }

        internal void PreviewAtProgress(float normalizedProgress)
        {
            var (position, rotation) = EvaluateSplineTransform(normalizedProgress);
            ApplyTransform(PositionTarget, RotationTarget, position, rotation);
        }

        internal void PreviewAtKnot(int knotIndex)
        {
            var normalizedProgress =
                SplineContainer.Spline.ConvertIndexUnit(knotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            PreviewAtProgress(normalizedProgress);
        }

        private void UpdateTransformAlongPath(float currentTime)
        {
            var (splinePosition, splineRotation) = EvaluateSplineTransform(currentTime);

            if (_config.InvertRotation)
                (splinePosition, splineRotation) =
                    ApplyInversion(splinePosition, splineRotation, _transformation.SplineStartPoint);

            var finalPosition = TransformPosition(splinePosition);
            ApplyTransform(_config.PositionTarget, _config.RotationTarget, finalPosition, splineRotation);
        }

        private Vector3 TransformPosition(Vector3 splinePosition)
        {
            var offsetPoint = splinePosition - _transformation.SplineStartPoint;
            var rotatedPoint = _transformation.Rotation * offsetPoint;
            var scaledPoint = rotatedPoint * _transformation.Scale;
            return _config.StartPoint + scaledPoint;
        }

        private (Vector3 position, Quaternion rotation) EvaluateSplineTransform(float normalizedProgress)
        {
            var splineProgress = TimeCurve.Evaluate(normalizedProgress);
            var position = (Vector3)SplineContainer.EvaluatePosition(splineProgress);
            var rotation = (Quaternion)RotationData.Evaluate(
                SplineContainer.Spline,
                splineProgress,
                PathIndexUnit.Normalized,
                new InterpolatedQuaternion());

            return (position, rotation);
        }

        private (Vector3 position, Quaternion rotation) ApplyInversion(
            Vector3 position,
            Quaternion rotation,
            Vector3 splineStartPoint)
        {
            var localPosition = position - splineStartPoint;
            localPosition.x = -localPosition.x;
            position = splineStartPoint + localPosition;

            var mirrorAxis = Vector3.right;
            rotation = Quaternion.AngleAxis(180f, mirrorAxis) * rotation * Quaternion.AngleAxis(180f, mirrorAxis);

            return (position, rotation);
        }

        private SplineTransformation CalculateTransformation()
        {
            var splineStartPoint = (Vector3)SplineContainer.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)SplineContainer.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - splineStartPoint).normalized;
            var animationDirection = (_config.EndPoint - _config.StartPoint).normalized;

            var splineLength = Vector3.Distance(splineStartPoint, splineEndPoint);
            var animationLength = Vector3.Distance(_config.StartPoint, _config.EndPoint);

            var scale = animationLength / splineLength;
            var rotation = Quaternion.FromToRotation(splineDirection, animationDirection);

            return new SplineTransformation(splineStartPoint, rotation, scale);
        }

        private void ApplyTransform(
            Transform positionTarget,
            Transform rotationTarget,
            Vector3 position,
            Quaternion rotation)
        {
            positionTarget.position = position;
            rotationTarget.rotation = rotation;
        }
    }
}