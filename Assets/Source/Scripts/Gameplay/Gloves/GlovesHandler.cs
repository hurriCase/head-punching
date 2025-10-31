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

        public void Init()
        {
            _leftGlove.Init(_inputService.OnLeftPressed, _inputService.OnLeftReleased);
            _rightGlove.Init(_inputService.OnRightPressed, _inputService.OnRightReleased);
        }
    }
}