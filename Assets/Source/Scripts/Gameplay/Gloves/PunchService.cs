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

        private void Awake()
        {
            _currentRotationData = _punches[PunchType.Uppercut];
        }

        public async UniTask ExecutePunch(Transform target, Vector3 startPoint, Vector3 endPoint)
        {
            _target = target;

            await Tween.Custom(this, 0f, 1f, 0.5f,
                onValueChange: static (self, progress) =>
                    self._currentRotationData.AnimateObject(progress, self._target));

            _target.position = startPoint;
        }
    }
}