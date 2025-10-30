using PrimeTween;
using R3;
using Source.Scripts.Input;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay
{
    internal sealed class BoxingGlove : MonoBehaviour
    {
        [SerializeField] private Transform _punchTransform;
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _xOffset;
        [SerializeField] private TweenSettings _punchSettings;
        [SerializeField] private TweenSettings _returnSettings;
        [SerializeField] private ShakeSettings _shakeSettings;

        [Inject] private IInputService _inputService;
        [Inject] private Camera _camera;
        [Inject] private HeadController _headController;

        private Vector3 _basePunchPosition;
        private Quaternion _basePunchRotation;

        private Sequence _currentPunchTween;
        private Vector3 _currentPunchTarget;

        internal void Init()
        {
            _basePunchPosition = _punchTransform.localPosition;
            _basePunchRotation = _punchTransform.localRotation;

            _inputService.CurrentMousePosition
                .Subscribe(this, static (position, self) => self.MoveGlove(position))
                .RegisterTo(destroyCancellationToken);

            ShakeGlove();
        }

        internal void Punch()
        {
            if (_currentPunchTween.isAlive)
                return;

            var currentPosition = _punchTransform.position;
            _currentPunchTarget = _headController.MeshCollider.ClosestPoint(currentPosition);

            var direction = _currentPunchTarget - currentPosition;
            var targetRotation = Quaternion.LookRotation(direction);

            var parent = _punchTransform.parent;
            var localPunchTarget = parent.InverseTransformPoint(_currentPunchTarget);
            var localTargetRotation = Quaternion.Inverse(parent.rotation) * targetRotation;

            MoveGlove(localPunchTarget, localTargetRotation, _punchSettings);
            _currentPunchTween.OnComplete(this, static self => self.OnPunchComplete());
        }

        private void ShakeGlove()
        {
            Tween.ShakeLocalPosition(_visualTransform, _shakeSettings);
        }

        private void OnPunchComplete()
        {
            MoveGlove(_basePunchPosition, _basePunchRotation, _returnSettings);

            _headController.ApplyPunchImpact(_currentPunchTarget);
        }

        private void MoveGlove(Vector3 position, Quaternion rotation, TweenSettings settings)
        {
            _currentPunchTween = Sequence.Create()
                .Chain(Tween.LocalPosition(_punchTransform, position, settings))
                .Group(Tween.LocalRotation(_punchTransform, rotation, settings));
        }

        private void MoveGlove(Vector2 mousePosition)
        {
            var screenPosition = new Vector3(mousePosition.x, mousePosition.y, _camera.nearClipPlane);
            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            transform.position = new Vector3(worldPosition.x + _xOffset, worldPosition.y, 0);
        }
    }
}