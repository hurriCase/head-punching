using Cysharp.Threading.Tasks;
using PrimeTween;
using R3;
using Source.Scripts.Input;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class GloveMovementHandler : MonoBehaviour
    {
        [SerializeField] private Transform _punchTransform;
        [SerializeField] private Transform _visualTransform;

        [SerializeField] private Vector3 _offset;

        [SerializeField] private TweenSettings _punchSettings;
        [SerializeField] private TweenSettings _returnSettings;
        [SerializeField] private ShakeSettings _shakeSettings;

        [Inject] private IInputService _inputService;
        [Inject] private Camera _camera;

        private Vector3 _basePunchPosition;
        private Quaternion _basePunchRotation;

        internal void Init()
        {
            _basePunchPosition = _punchTransform.localPosition;
            _basePunchRotation = _punchTransform.localRotation;

            _inputService.CurrentMousePosition
                .Subscribe(this, static (position, self) => self.MoveToMousePosition(position))
                .RegisterTo(destroyCancellationToken);

            ShakeGlove();
        }

        internal async UniTask ExecutePunch(Vector3 localPunchTarget, Quaternion localTargetRotation)
        {
            var sequence = CreateGloveSequence(localPunchTarget, localTargetRotation, _punchSettings);

            await sequence.OnComplete(this, static self => self
                .CreateGloveSequence(self._basePunchPosition, self._basePunchRotation, self._returnSettings));
        }

        private Sequence CreateGloveSequence(Vector3 position, Quaternion rotation, TweenSettings settings) =>
            Sequence.Create()
                .Chain(Tween.LocalPosition(_punchTransform, position, settings))
                .Group(Tween.LocalRotation(_punchTransform, rotation, settings));

        private void MoveToMousePosition(Vector2 mousePosition)
        {
            var screenPosition = new Vector3(mousePosition.x, mousePosition.y, _camera.nearClipPlane);
            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            var targetPosition = worldPosition + _offset;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
        }

        private void ShakeGlove()
        {
            Tween.ShakeLocalPosition(_visualTransform, _shakeSettings);
        }
    }
}