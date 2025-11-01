using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class PunchService : MonoBehaviour, IPunchService
    {
        [SerializeField] private SerializedDictionary<PunchType, SplineRotationData> _punches;

        private SplineRotationData _currentRotationData;
        private Transform _target;

        private Vector3 _punchStartPoint;
        private Vector3 _punchEndPoint;
        private Vector3 _splineStartPoint;
        private Quaternion _transformRotation;
        private float _scale;

        private void Awake()
        {
            _currentRotationData = _punches[PunchType.Uppercut];
        }

        public async UniTask ExecutePunch(Transform target, Vector3 startPoint, Vector3 endPoint)
        {
            _target = target;
            _punchStartPoint = startPoint;
            _punchEndPoint = endPoint;

            CalculateTransformation();

            await Tween.Custom(this, 0f, 1f, 3f,
                onValueChange: static (self, currentTime) => self.AnimateTransformedPunch(currentTime));

            _target.position = startPoint;
        }

        private void CalculateTransformation()
        {
            var spline = _currentRotationData.SplineContainer;

            _splineStartPoint = spline.EvaluatePosition(0f);
            var splineEndPoint = (Vector3)spline.EvaluatePosition(1f);

            var splineDirection = (splineEndPoint - _splineStartPoint).normalized;
            var punchDirection = (_punchEndPoint - _punchStartPoint).normalized;

            var splineLength = Vector3.Distance(_splineStartPoint, splineEndPoint);
            var punchLength = Vector3.Distance(_punchStartPoint, _punchEndPoint);

            _scale = punchLength / splineLength;
            _transformRotation = Quaternion.FromToRotation(splineDirection, punchDirection);
        }

        private void AnimateTransformedPunch(float currentTime)
        {
            var spline = _currentRotationData.SplineContainer;

            var splineProgress = _currentRotationData.GetSplineProgress(currentTime);

            var splinePoint = (Vector3)spline.EvaluatePosition(splineProgress);
            var splineRotation = _currentRotationData.GetRotation(splineProgress);

            var offsetPoint = splinePoint - _splineStartPoint;
            var rotatedPoint = _transformRotation * offsetPoint;
            var scaledPoint = rotatedPoint * _scale;
            var finalPosition = _punchStartPoint + scaledPoint;

            var finalRotation = _transformRotation * splineRotation;

            _target.position = finalPosition;
            _target.rotation = finalRotation;
        }
    }
}