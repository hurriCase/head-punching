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
        public Observable<Unit> OnLeftClicked => _leftPunchSubject;
        public Observable<Unit> OnRightClicked => _rightPunchSubject;

        private readonly ReactiveProperty<Vector2> _currentMousePosition = new();
        private readonly Subject<Unit> _leftPunchSubject = new();
        private readonly Subject<Unit> _rightPunchSubject = new();

        private readonly IA_Gameplay _inputActions;

        internal InputService(IA_Gameplay inputActions)
        {
            _inputActions = inputActions;

            inputActions.Gameplay.MousePosition.performed += OnMouseMoved;
            inputActions.Gameplay.LeftClick.performed += OnLeftMouseClick;
            inputActions.Gameplay.RightClick.performed += OnRightMouseClick;
            inputActions.Enable();
        }

        private void OnMouseMoved(InputAction.CallbackContext context)
        {
            _currentMousePosition.Value = context.ReadValue<Vector2>();
        }

        private void OnLeftMouseClick(InputAction.CallbackContext context)
        {
            _leftPunchSubject.OnNext(Unit.Default);
        }

        private void OnRightMouseClick(InputAction.CallbackContext context)
        {
            _rightPunchSubject.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _inputActions.Gameplay.MousePosition.performed -= OnMouseMoved;
            _inputActions.Gameplay.LeftClick.performed -= OnLeftMouseClick;
            _inputActions.Gameplay.RightClick.performed -= OnRightMouseClick;
            _inputActions.Disable();

            _currentMousePosition.Dispose();
            _leftPunchSubject.Dispose();
            _rightPunchSubject.Dispose();
        }
    }
}