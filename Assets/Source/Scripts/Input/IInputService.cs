using R3;
using UnityEngine;

namespace Source.Scripts.Input
{
    internal interface IInputService
    {
        ReadOnlyReactiveProperty<Vector2> CurrentMousePosition { get; }
        Observable<Unit> OnLeftPressed { get; }
        Observable<Unit> OnLeftReleased { get; }
        Observable<Unit> OnRightPressed { get; }
        Observable<Unit> OnRightReleased { get; }
    }
}