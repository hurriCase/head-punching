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
        [SerializeField] private Transform _visualTransform;

        [SerializeField] private Vector3 _offset;

        [SerializeField] private ShakeSettings _shakeSettings;

        [Inject] private IInputService _inputService;
        [Inject] private Camera _camera;

        internal void Init()
        {
            _inputService.CurrentMousePosition
                .Subscribe(this, static (position, self) => self.MoveToMousePosition(position))
                .RegisterTo(destroyCancellationToken);

            ShakeGlove();
        }

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