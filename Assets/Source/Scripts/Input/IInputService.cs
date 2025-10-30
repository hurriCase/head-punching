using R3;
using UnityEngine;

namespace Source.Scripts.Input
{
    internal interface IInputService
    {
        ReadOnlyReactiveProperty<Vector2> CurrentMousePosition { get; }
        Observable<Unit> OnLeftClicked { get; }
        Observable<Unit> OnRightClicked { get; }
    }
}