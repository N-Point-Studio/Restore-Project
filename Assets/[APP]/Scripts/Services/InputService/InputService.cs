using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class InputService : IInitializable, IDisposable
{
        private readonly PlayerInputSystem inputSystem;
        private GameInput Input => inputSystem.Input;

        public event Action<Vector2> OnPressMoved;

        public event Action<Vector2> OnPrimaryStarted;
        public event Action<Vector2> OnPrimaryEnded;

        public event Action<Vector2> OnSecondaryFingerStarted;
        public event Action<Vector2> OnSecondaryFingerMoved;
        public event Action<Vector2> OnSecondaryFingerEnded;

        public event Action<Vector2> OnSecondaryPressStarted;
        public event Action<Vector2> OnSecondaryPressEnded;

        public event Action<float> OnScrollPerformed;

        [Inject]
        public InputService(PlayerInputSystem inputSystem)
        {
                this.inputSystem = inputSystem;
        }

        public void Initialize()
        {
                inputSystem.ChangeInputState(InputStateType.Player);
                Input.Player.Press.started += PressStarted;
                Input.Player.Press.canceled += PressEnded;
                Input.Player.ScreenPos.performed += PressMoved;

#if UNITY_IOS || UNITY_ANDROID
                Input.Player.SecondaryFingerPress.started += SecondaryFingerPressStarted;
                Input.Player.SecondaryFingerPress.canceled += SecondaryFingerPressEnded;
                Input.Player.SecondaryFingerPos.performed += SecondaryFingerPressMoved;
#endif

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
                Input.Player.Scroll.performed += ScrollPerformed;
                Input.Player.SecondaryPress.started += SecondaryPressStarted;
                Input.Player.SecondaryPress.canceled += SecondaryPressEnded;
#endif
        }

        public void Dispose()
        {
                Input.Player.Press.started -= PressStarted;
                Input.Player.Press.canceled -= PressEnded;
                Input.Player.ScreenPos.performed -= PressMoved;

#if UNITY_IOS || UNITY_ANDROID
                Input.Player.SecondaryFingerPress.started -= SecondaryFingerPressStarted;
                Input.Player.SecondaryFingerPress.canceled -= SecondaryFingerPressEnded;
                Input.Player.SecondaryFingerPos.performed -= SecondaryFingerPressMoved;
#endif

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
                Input.Player.Scroll.performed -= ScrollPerformed;
                Input.Player.SecondaryPress.started -= SecondaryPressStarted;
                Input.Player.SecondaryPress.canceled -= SecondaryPressEnded;
#endif
        }

        private void PressStarted(InputAction.CallbackContext context) => OnPrimaryStarted?.Invoke(GetPrimaryPos());
        private void PressMoved(InputAction.CallbackContext context) => OnPressMoved?.Invoke(GetPrimaryPos());
        private void PressEnded(InputAction.CallbackContext context) => OnPrimaryEnded?.Invoke(GetPrimaryPos());
        private void SecondaryFingerPressStarted(InputAction.CallbackContext context) => OnSecondaryFingerStarted?.Invoke(GetSecondaryPos());
        private void SecondaryFingerPressMoved(InputAction.CallbackContext context) => OnSecondaryFingerMoved?.Invoke(GetSecondaryPos());
        private void SecondaryFingerPressEnded(InputAction.CallbackContext context) => OnSecondaryFingerEnded?.Invoke(GetSecondaryPos());
        private void SecondaryPressEnded(InputAction.CallbackContext context) => OnSecondaryPressEnded?.Invoke(GetSecondaryPos());
        private void SecondaryPressStarted(InputAction.CallbackContext context) => OnSecondaryPressStarted?.Invoke(GetSecondaryPos());
        private void ScrollPerformed(InputAction.CallbackContext context) => OnScrollPerformed?.Invoke(context.ReadValue<float>());

        public Vector2 GetPrimaryPos() { return Input.Player.ScreenPos.ReadValue<Vector2>(); }
        public Vector2 GetSecondaryPos() { return Input.Player.SecondaryFingerPos.ReadValue<Vector2>(); }

}