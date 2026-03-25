using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class InputSystemService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;

    // Player
    public event Action<Vector2> OnMouseMoved;
    public event Action<Vector2> OnLeftPressStarted;
    public event Action<Vector2> OnLeftPressEnded;
    public event Action OnRightPressStarted;
    public event Action OnRightPressEnded;
    public event Action<float> OnScrollPerformed;
    public event Action OnPlayerKeycodeEscapePerformed;
    public event Action OnPlayerKeycodeEnterPerformed;

    // UI
    public event Action OnUIKeycodeEnterPerformed;
    public event Action OnUIKeycodeEscapePerformed;
    public event Action OnUIKeycodeRPerformed;

    [Inject]
    public InputSystemService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {        
        Input.Player.Press.started += HandleLeftPressStarted;
        Input.Player.Press.canceled += HandleLeftPressCanceled;
        Input.Player.SecondaryPress.started += HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled += HandleRightPressCanceled;
        Input.Player.ScreenPos.performed += HandleMouseMoved;
        Input.Player.Scroll.performed += HandleScrollPerformed;
        Input.Player.KeycodeEscape.performed += HandlePlayerKeycodeEscapePerformed;
        Input.Player.KeycodeEnter.performed += HandlePlayerKeycodeEnterPerformed;

        Input.UI.KeycodeEnter.performed += HandleUIKeycodeEnterPerformed;
        Input.UI.KeycodeEscape.performed += HandleUIKeycodeEscapePerformed;
        Input.UI.KeycodeR.performed += HandleUIKeycodeRPerformed;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= HandleLeftPressStarted;
        Input.Player.Press.canceled -= HandleLeftPressCanceled;
        Input.Player.SecondaryPress.started -= HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled -= HandleRightPressCanceled;
        Input.Player.ScreenPos.performed -= HandleMouseMoved;
        Input.Player.Scroll.performed -= HandleScrollPerformed;
        Input.Player.KeycodeEscape.performed -= HandlePlayerKeycodeEscapePerformed;
        Input.Player.KeycodeEnter.performed -= HandlePlayerKeycodeEnterPerformed;
        
        Input.UI.KeycodeEnter.performed -= HandleUIKeycodeEnterPerformed;
        Input.UI.KeycodeEscape.performed -= HandleUIKeycodeEscapePerformed;
        Input.UI.KeycodeR.performed -= HandleUIKeycodeRPerformed;
    }

    public void ChangeInputState(InputStateType state)
    {
        inputSystem.ChangeInputState(state);
    }

    // Player
    private void HandleLeftPressStarted(InputAction.CallbackContext context) => OnLeftPressStarted?.Invoke(GetMousePosition());
    private void HandleLeftPressCanceled(InputAction.CallbackContext context) => OnLeftPressEnded?.Invoke(GetMousePosition());
    private void HandleRightPressStarted(InputAction.CallbackContext context) => OnRightPressStarted?.Invoke();
    private void HandleRightPressCanceled(InputAction.CallbackContext context) => OnRightPressEnded?.Invoke();
    private void HandleMouseMoved(InputAction.CallbackContext context) => OnMouseMoved?.Invoke(context.ReadValue<Vector2>());
    private void HandleScrollPerformed(InputAction.CallbackContext context) => OnScrollPerformed?.Invoke(context.ReadValue<float>());
    private void HandlePlayerKeycodeEscapePerformed(InputAction.CallbackContext context) => OnPlayerKeycodeEscapePerformed?.Invoke();
    private void HandlePlayerKeycodeEnterPerformed(InputAction.CallbackContext context) => OnPlayerKeycodeEnterPerformed?.Invoke();
    public Vector2 GetMousePosition() { return Input.Player.ScreenPos.ReadValue<Vector2>(); }

    // UI
    private void HandleUIKeycodeEnterPerformed(InputAction.CallbackContext context) => OnUIKeycodeEnterPerformed?.Invoke();
    private void HandleUIKeycodeEscapePerformed(InputAction.CallbackContext context) => OnUIKeycodeEscapePerformed?.Invoke();
    private void HandleUIKeycodeRPerformed(InputAction.CallbackContext context) => OnUIKeycodeRPerformed?.Invoke();

}