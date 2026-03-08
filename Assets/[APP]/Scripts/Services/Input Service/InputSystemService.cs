using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class InputSystemService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;

    public event Action<Vector2> OnMouseMoved;

    public event Action<Vector2> OnLeftPressStarted;
    public event Action<Vector2> OnLeftPressEnded;

    public event Action OnRightPressStarted;
    public event Action OnRightPressEnded;

    public event Action<float> OnScrollPerformed;

    [Inject]
    public InputSystemService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);
        Input.Player.Press.started += HandleLeftPressStarted;
        Input.Player.Press.canceled += HandleLeftPressCanceled;

        Input.Player.SecondaryPress.started += HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled += HandleRightPressCanceled;

        Input.Player.ScreenPos.performed += HandleMouseMoved;

        Input.Player.Scroll.performed += HandleScrollPerformed;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= HandleLeftPressStarted;
        Input.Player.Press.canceled -= HandleLeftPressCanceled;

        Input.Player.SecondaryPress.started -= HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled -= HandleRightPressCanceled;

        Input.Player.ScreenPos.performed -= HandleMouseMoved;

        Input.Player.Scroll.performed -= HandleScrollPerformed;
    }

    private void HandleLeftPressStarted(InputAction.CallbackContext context) => OnLeftPressStarted?.Invoke(GetMousePosition());
    private void HandleLeftPressCanceled(InputAction.CallbackContext context) => OnLeftPressEnded?.Invoke(GetMousePosition());
    private void HandleRightPressStarted(InputAction.CallbackContext context) => OnRightPressStarted?.Invoke();
    private void HandleRightPressCanceled(InputAction.CallbackContext context) => OnRightPressEnded?.Invoke();
    private void HandleMouseMoved(InputAction.CallbackContext context) => OnMouseMoved?.Invoke(context.ReadValue<Vector2>());
    private void HandleScrollPerformed(InputAction.CallbackContext context) => OnScrollPerformed?.Invoke(context.ReadValue<float>());
    public Vector2 GetMousePosition() { return Input.Player.ScreenPos.ReadValue<Vector2>(); }

}