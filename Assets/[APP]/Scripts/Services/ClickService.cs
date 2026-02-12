using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class ClickService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;
    private readonly PointerService pointer;
    private IClickable currentClicked;

    public ClickService(PlayerInputSystem inputSystem, PointerService pointer)
    {
        Debug.Log("Click constructor");
        this.inputSystem = inputSystem;
        this.pointer = pointer;
    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);
        Input.Player.Press.performed += OnPress;
        Input.Player.Press.canceled += OnRelease;
    }

    public void Dispose()
    {
        Input.Player.Press.performed -= OnPress;
        Input.Player.Press.canceled -= OnRelease;
    }
    private void OnPress(InputAction.CallbackContext context)
    {
        Debug.Log("Pres");
        Vector2 screen = Input.Player.ScreenPos.ReadValue<Vector2>();

        if (!pointer.TryRaycast(screen, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IClickable clickable)) return;

        currentClicked = clickable;
        currentClicked.OnClickedPerformed();
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        Debug.Log("Tap end");

        if (currentClicked == null) return;
        currentClicked.OnClickedEnd();
        currentClicked = null;
    }
}
