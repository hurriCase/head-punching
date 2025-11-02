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

        private readonly AnimationState _animationState = new();

        internal async UniTask Animate(SplineAnimationConfig config)
        {
            var initialPosition = config.PositionTarget.position;
            var initialRotation = config.RotationTarget.rotation;

            var transformation = CalculateTransformation(config);

            _animationState.Config = config;
            _animationState.Transformation = transformation;
            _animationState.SplineAnimation = this;

            await Tween.Custom(
                _animationState,
                0f,
                1f,
                AnimationSettings,
                onValueChange: static (state, currentTime) =>
                    state.SplineAnimation.UpdateTransformAlongPath(currentTime, state.Config, state.Transformation));

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

        private void UpdateTransformAlongPath(float currentTime, SplineAnimationConfig config, SplineTransformation transformation)
        {
            var (splinePosition, splineRotation) = EvaluateSplineTransform(currentTime);

            if (config.InvertRotation)
                (splinePosition, splineRotation) = ApplyInversion(splinePosition, splineRotation, transformation.SplineStartPoint);

            var finalPosition = TransformPosition(splinePosition, config, transformation);

            ApplyTransform(config.PositionTarget, config.RotationTarget, finalPosition, splineRotation);
        }

        private Vector3 TransformPosition(Vector3 splinePosition, SplineAnimationConfig config, SplineTransformation transformation)
        {
            var offsetPoint = splinePosition - transformation.SplineStartPoint;
            var rotatedPoint = transformation.Rotation * offsetPoint;
            var scaledPoint = rotatedPoint * transformation.Scale;
            return config.StartPoint + scaledPoint;
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

        private SplineTransformation CalculateTransformation(SplineAnimationConfig config)
        {
            var splineStartPoint = (Vector3)SplineContainer.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)SplineContainer.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - splineStartPoint).normalized;
            var animationDirection = (config.EndPoint - config.StartPoint).normalized;

            var splineLength = Vector3.Distance(splineStartPoint, splineEndPoint);
            var animationLength = Vector3.Distance(config.StartPoint, config.EndPoint);

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