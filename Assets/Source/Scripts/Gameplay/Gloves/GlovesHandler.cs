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

        public void Init()
        {
            _leftGlove.Init();
            _rightGlove.Init();

            _inputService.OnLeftClicked
                .Subscribe(this, static (_, self) => self._leftGlove.Punch())
                .RegisterTo(destroyCancellationToken);

            _inputService.OnRightClicked
                .Subscribe(this, static (_, self) => self._rightGlove.Punch())
                .RegisterTo(destroyCancellationToken);
        }
    }
}