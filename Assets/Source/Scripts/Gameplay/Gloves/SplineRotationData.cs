using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class SplineRotationData : MonoBehaviour
    {
        [field: SerializeField] internal SplineContainer SplineContainer { get; private set; }

        [SerializeField, NonReorderable] private SplineData<quaternion> _rotationData = new();
        [SerializeField] private AnimationCurve _timeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
        [SerializeField] private Transform _target;
        [SerializeField, Range(0f, 1f)] private float _progress;
        [SerializeField] private bool _isValidatePoints;
#endif

        internal float GetSplineProgress(float progress) => _timeCurve.Evaluate(progress);

        internal quaternion GetRotation(float splineProgress) => _rotationData
            .Evaluate(SplineContainer.Spline, splineProgress, PathIndexUnit.Normalized, new InterpolatedQuaternion());

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!SplineContainer)
                return;

            if (_isValidatePoints)
                SyncRotationDataWithKnots();

            if (!_target)
                return;

            var splineProgress = GetSplineProgress(_progress);
            AnimateObject(splineProgress, _target);
        }

        private void AnimateObject(float splineProgress, Transform target)
        {
            target.position = SplineContainer.EvaluatePosition(splineProgress);
            target.rotation = GetRotation(splineProgress);
        }

        private void SyncRotationDataWithKnots()
        {
            var spline = SplineContainer.Spline;
            var knotCount = spline.Count;

            if (_rotationData.Count == knotCount)
                return;

            _rotationData.Clear();

            for (var i = 0; i < knotCount; i++)
            {
                var knotRotation = spline[i].Rotation;
                var rotationData = new DataPoint<quaternion>(i, knotRotation);
                _rotationData.Add(rotationData);
            }
        }
#endif
    }
}