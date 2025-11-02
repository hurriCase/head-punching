using R3;
using Source.Scripts.Input;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class GlovesHandler : MonoBehaviour, IGlovesHandler
    {
        [SerializeField] private BoxingGlove _leftGlove;
        [SerializeField] private BoxingGlove _rightGlove;

        [Inject] private IInputService _inputService;
        [Inject] private Camera _camera;

        public void Init()
        {
            _leftGlove.Init(_inputService.OnLeftPressed, _inputService.OnLeftReleased);
            _rightGlove.Init(_inputService.OnRightPressed, _inputService.OnRightReleased);

            _inputService.CurrentMousePosition
                .Subscribe(this, static (position, self) => self.MoveToMousePosition(position))
                .RegisterTo(destroyCancellationToken);
        }

        private void MoveToMousePosition(Vector2 mousePosition)
        {
            var screenPosition = new Vector3(mousePosition.x, mousePosition.y, _camera.nearClipPlane);
            var targetPosition = _camera.ScreenToWorldPoint(screenPosition);
            transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
        }
    }
}