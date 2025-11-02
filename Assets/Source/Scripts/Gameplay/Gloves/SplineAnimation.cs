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

        private Vector3 _punchStartPoint;
        private Vector3 _punchEndPoint;
        private Vector3 _splineStartPoint;
        private Quaternion _transformRotation;
        private float _scale;

        internal async UniTask Animate(
            Transform positionTarget,
            Transform rotationTarget,
            Vector3 startPoint,
            Vector3 endPoint)
        {
            PositionTarget = positionTarget;
            RotationTarget = rotationTarget;
            _punchStartPoint = startPoint;
            _punchEndPoint = endPoint;

            CalculateTransformation();

            await Tween.Custom(this, 0f, 1f, 0.3f,
                onValueChange: static (self, currentTime) => self.UpdateRuntimeTransform(currentTime));

            rotationTarget.position = startPoint;
        }

        internal void PreviewAtProgress(float normalizedProgress)
        {
            var (position, rotation) = EvaluateSpline(normalizedProgress);
            ApplyTransform(position, rotation);
        }

        internal void PreviewAtKnot(int knotIndex)
        {
            var normalizedProgress =
                SplineContainer.Spline.ConvertIndexUnit(knotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            PreviewAtProgress(normalizedProgress);
        }

        internal void InsertRotationData(int index, quaternion rotation)
        {
            RotationData[index] = new DataPoint<quaternion>(index, rotation);
        }

        private void UpdateRuntimeTransform(float currentTime)
        {
            var (splinePosition, splineRotation) = EvaluateSpline(currentTime);

            var offsetPoint = splinePosition - _splineStartPoint;
            var rotatedPoint = _transformRotation * offsetPoint;
            var scaledPoint = rotatedPoint * _scale;
            var finalPosition = _punchStartPoint + scaledPoint;

            ApplyTransform(finalPosition, splineRotation);
        }

        private (Vector3 position, Quaternion rotation) EvaluateSpline(float normalizedProgress)
        {
            var splineProgress = TimeCurve.Evaluate(normalizedProgress);
            var position = (Vector3)SplineContainer.EvaluatePosition(splineProgress);
            var rotation = RotationData.Evaluate(
                SplineContainer.Spline,
                splineProgress,
                PathIndexUnit.Normalized,
                new InterpolatedQuaternion());

            return (position, rotation);
        }

        private void ApplyTransform(Vector3 position, Quaternion rotation)
        {
            PositionTarget.position = position;
            RotationTarget.rotation = rotation;
        }

        private void CalculateTransformation()
        {
            _splineStartPoint = SplineContainer.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)SplineContainer.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - _splineStartPoint).normalized;
            var punchDirection = (_punchEndPoint - _punchStartPoint).normalized;

            var splineLength = Vector3.Distance(_splineStartPoint, splineEndPoint);
            var punchLength = Vector3.Distance(_punchStartPoint, _punchEndPoint);

            _scale = punchLength / splineLength;
            _transformRotation = Quaternion.FromToRotation(splineDirection, punchDirection);
        }
    }
}