using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Source.Scripts.Input
{
    [Preserve]
    internal sealed class InputService : IDisposable, IInputService
    {
        public ReadOnlyReactiveProperty<Vector2> CurrentMousePosition => _currentMousePosition;
        public Observable<Unit> OnLeftPressed { get; }
        public Observable<Unit> OnLeftReleased { get; }
        public Observable<Unit> OnRightPressed { get; }
        public Observable<Unit> OnRightReleased { get; }

        private readonly ReactiveProperty<Vector2> _currentMousePosition = new();
        private readonly IA_Gameplay _inputActions;

        internal InputService(IA_Gameplay inputActions)
        {
            _inputActions = inputActions;

            OnLeftPressed = Observable.FromEvent<InputAction.CallbackContext>(
                handler => inputActions.Gameplay.LeftClick.started += handler,
                handler => inputActions.Gameplay.LeftClick.started -= handler
            ).Select(static _ => Unit.Default);

            OnLeftReleased = Observable.FromEvent<InputAction.CallbackContext>(
                handler => inputActions.Gameplay.LeftClick.canceled += handler,
                handler => inputActions.Gameplay.LeftClick.canceled -= handler
            ).Select(static _ => Unit.Default);

            OnRightPressed = Observable.FromEvent<InputAction.CallbackContext>(
                handler => inputActions.Gameplay.RightClick.started += handler,
                handler => inputActions.Gameplay.RightClick.started -= handler
            ).Select(static _ => Unit.Default);

            OnRightReleased = Observable.FromEvent<InputAction.CallbackContext>(
                handler => inputActions.Gameplay.RightClick.canceled += handler,
                handler => inputActions.Gameplay.RightClick.canceled -= handler
            ).Select(static _ => Unit.Default);

            inputActions.Gameplay.MousePosition.performed += OnMouseMoved;
            inputActions.Enable();
        }

        private void OnMouseMoved(InputAction.CallbackContext context) =>
            _currentMousePosition.Value = context.ReadValue<Vector2>();

        public void Dispose()
        {
            _inputActions.Gameplay.MousePosition.performed -= OnMouseMoved;
            _inputActions.Disable();
            _currentMousePosition.Dispose();
        }
    }
}