using AYellowpaper.SerializedCollections;
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

        [SerializeField] private SerializedDictionary<PunchType, SplineAnimation> _punches;

        public void Init()
        {
            var uppercutAnimation = _punches[PunchType.Uppercut];

            _leftGlove.PunchAnimation = uppercutAnimation;
            _rightGlove.PunchAnimation = uppercutAnimation;

            _leftGlove.Init(_inputService.OnLeftPressed, _inputService.OnLeftReleased);
            _rightGlove.Init(_inputService.OnRightPressed, _inputService.OnRightReleased);
        }
    }
}