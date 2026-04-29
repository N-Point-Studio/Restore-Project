using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch; // Added for Touch
using VContainer;
using VContainer.Unity;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch; // Added for Touch

public class InputSystemService : IInitializable, IDisposable, ITickable // Added ITickable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;

    public event Action<Vector2> OnLeftPressStarted;
    public event Action<Vector2> OnLeftPressEnded;
    public event Action OnRightPressStarted;
    public event Action OnRightPressEnded;
    public event Action<float> OnScrollPerformed;
    public event Action OnPlayerKeycodeEscapePerformed;
    public event Action OnPlayerKeycodeEnterPerformed;
    public event Action OnPlayerKeycodeTabPerformed;
    public event Action OnPlayerKeycodeTabCanceled;

    // UI
    public event Action OnUIKeycodeEnterPerformed;
    public event Action OnUIKeycodeEscapePerformed;
    public event Action OnUIKeycodeRPerformed;

    // Pinch Zoom Variables
    private float previousPinchDistance;

    [Inject]
    public InputSystemService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {
        // Enable Enhanced Touch Support
        EnhancedTouchSupport.Enable();

        Input.Player.Press.started += HandleLeftPressStarted;
        Input.Player.Press.canceled += HandleLeftPressCanceled;
        Input.Player.SecondaryPress.started += HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled += HandleRightPressCanceled;
        Input.Player.ScreenPos.performed += HandleMouseMoved;
        Input.Player.Scroll.performed += HandleScrollPerformed;
        Input.Player.KeycodeEscape.performed += HandlePlayerKeycodeEscapePerformed;
        Input.Player.KeycodeEnter.performed += HandlePlayerKeycodeEnterPerformed;
        Input.Player.KeycodeTab.performed += HandlePlayerKeycodeTabPerformed;
        Input.Player.KeycodeTab.canceled += HandlePlayerKeycodeTabCanceled;

        Input.UI.KeycodeEnter.performed += HandleUIKeycodeEnterPerformed;
        Input.UI.KeycodeEscape.performed += HandleUIKeycodeEscapePerformed;
        Input.UI.KeycodeR.performed += HandleUIKeycodeRPerformed;
    }

    public void Dispose()
    {
        EnhancedTouchSupport.Disable(); // Disable when destroyed

        Input.Player.Press.started -= HandleLeftPressStarted;
        Input.Player.Press.canceled -= HandleLeftPressCanceled;
        Input.Player.SecondaryPress.started -= HandleRightPressStarted;
        Input.Player.SecondaryPress.canceled -= HandleRightPressCanceled;
        Input.Player.ScreenPos.performed -= HandleMouseMoved;
        Input.Player.Scroll.performed -= HandleScrollPerformed;
        Input.Player.KeycodeEscape.performed -= HandlePlayerKeycodeEscapePerformed;
        Input.Player.KeycodeEnter.performed -= HandlePlayerKeycodeEnterPerformed;
        Input.Player.KeycodeTab.performed -= HandlePlayerKeycodeTabPerformed;
        Input.Player.KeycodeTab.canceled -= HandlePlayerKeycodeTabCanceled;

        Input.UI.KeycodeEnter.performed -= HandleUIKeycodeEnterPerformed;
        Input.UI.KeycodeEscape.performed -= HandleUIKeycodeEscapePerformed;
        Input.UI.KeycodeR.performed -= HandleUIKeycodeRPerformed;
    }

    public void Tick()
    {
        // Handle Pinch-to-Zoom for Touchscreens
        if (Touch.activeTouches.Count == 2)
        {
            Touch touch0 = Touch.activeTouches[0];
            Touch touch1 = Touch.activeTouches[1];

            float currentPinchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

            // If either finger just started touching, set the initial distance
            if (touch0.phase == UnityEngine.InputSystem.TouchPhase.Began || touch1.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                previousPinchDistance = currentPinchDistance;
            }
            // If both fingers are moving or stationary, calculate the delta
            else if (touch0.phase == UnityEngine.InputSystem.TouchPhase.Moved || touch1.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                float pinchDelta = currentPinchDistance - previousPinchDistance;

                // Trigger the scroll event, mapping the pixel distance to a scroll-like float
                // You may need to multiply this by a small number (e.g., 0.05f) depending on your zoom sensitivity
                OnScrollPerformed?.Invoke(pinchDelta * 0.1f);

                previousPinchDistance = currentPinchDistance;
            }
        }
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
    private void HandleMouseMoved(InputAction.CallbackContext context) => InteractionEvents.OnMouseMoved?.Invoke(context.ReadValue<Vector2>());
    private void HandleScrollPerformed(InputAction.CallbackContext context) => OnScrollPerformed?.Invoke(context.ReadValue<float>());
    private void HandlePlayerKeycodeEscapePerformed(InputAction.CallbackContext context) => OnPlayerKeycodeEscapePerformed?.Invoke();
    private void HandlePlayerKeycodeEnterPerformed(InputAction.CallbackContext context) => OnPlayerKeycodeEnterPerformed?.Invoke();

    public Vector2 GetMousePosition()
    {
        if (Touch.activeTouches.Count > 0)
        {
            return Touch.activeTouches[0].screenPosition;
        }
        return Input.Player.ScreenPos.ReadValue<Vector2>();
    }

    private void HandlePlayerKeycodeTabPerformed(InputAction.CallbackContext context) => OnPlayerKeycodeTabPerformed?.Invoke();
    private void HandlePlayerKeycodeTabCanceled(InputAction.CallbackContext context) => OnPlayerKeycodeTabCanceled?.Invoke();


    // UI
    private void HandleUIKeycodeEnterPerformed(InputAction.CallbackContext context) => OnUIKeycodeEnterPerformed?.Invoke();
    private void HandleUIKeycodeEscapePerformed(InputAction.CallbackContext context) => OnUIKeycodeEscapePerformed?.Invoke();
    private void HandleUIKeycodeRPerformed(InputAction.CallbackContext context) => OnUIKeycodeRPerformed?.Invoke();
}