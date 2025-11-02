using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
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
        [field: SerializeField] internal Transform PreviewPositionTarget { get; private set; }
        [field: SerializeField] internal Transform PreviewRotationTarget { get; private set; }

        private SplineAnimationConfig _config;
        private SplineTransformation _transformation;

        internal async UniTask Animate(SplineAnimationConfig config)
        {
            var initialLocalPosition = config.PositionTarget.localPosition;
            var initialLocalRotation = config.RotationTarget.localRotation;

            _config = config;
            _transformation = CalculateTransformation();

            await Tween.Custom(
                this,
                0f,
                1f,
                PunchSettings,
                onValueChange: static (state, currentTime) => state.UpdateTransformAlongPath(currentTime));

            config.AnimationFinished?.Invoke(config.BoxingGlove, config.StartPoint, config.EndPoint);

            await Sequence.Create()
                .Chain(Tween.LocalPosition(config.PositionTarget, initialLocalPosition, ReturnSettings))
                .Group(Tween.LocalRotation(config.RotationTarget, initialLocalRotation, ReturnSettings));
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

            if (_config.InvertRotation)
                (splinePosition, splineRotation) =
                    ApplyInversion(splinePosition, splineRotation, _transformation.SplineStartPoint);

            var finalPosition = TransformPosition(splinePosition);
            _config.PositionTarget.position = finalPosition;
            _config.RotationTarget.rotation = splineRotation;
        }

        private Vector3 TransformPosition(Vector3 splinePosition)
        {
            var offsetPoint = splinePosition - _transformation.SplineStartPoint;
            var rotatedPoint = _transformation.Rotation * offsetPoint;
            var scaledPoint = rotatedPoint * _transformation.Scale;
            return _config.StartPoint + scaledPoint;
        }

        private (Vector3 position, Quaternion rotation) ApplyInversion(
            Vector3 position,
            Quaternion rotation,
            Vector3 splineStartPoint)
        {
            var localPosition = position - splineStartPoint;
            localPosition.x = -localPosition.x;
            position = splineStartPoint + localPosition;

            rotation = new Quaternion(-rotation.x, rotation.y, rotation.z, -rotation.w);

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
    }
}