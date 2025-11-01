using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class SplineRotationData : MonoBehaviour
    {
        [field: SerializeField] internal SplineContainer SplineContainer { get; private set; }

        [SerializeField] private SplineData<quaternion> _rotationData = new();

#if UNITY_EDITOR
        [SerializeField] private Transform _target;
        [SerializeField, Range(0f, 1f)] private float _progress;
#endif
        internal void AnimateObject(float progress, Transform target)
        {
            target.position = SplineContainer.EvaluatePosition(progress);
            target.rotation = GetRotation(progress);
        }

        internal quaternion GetRotation(float t) => _rotationData
            .Evaluate(SplineContainer.Spline, t, PathIndexUnit.Normalized, new InterpolatedQuaternion());

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!SplineContainer)
                return;

            SyncRotationDataWithKnots();

            if (!_target)
                return;

            AnimateObject(_progress, _target);
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