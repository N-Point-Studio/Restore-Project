using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class ClickService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;
    private readonly PointerService pointer;
    private readonly InspectionService inspectService;

    private IClickable currentClicked;
    private IInspectable inspectable;
    private Transform inspectableTransform;
    public ClickService(PlayerInputSystem inputSystem, PointerService pointer, InspectionService inspectService)
    {
        Debug.Log("click here");
        this.inputSystem = inputSystem;
        this.pointer = pointer;
        this.inspectService = inspectService;
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
        Vector2 screen = Input.Player.ScreenPos.ReadValue<Vector2>();

        if (!pointer.TryRaycast(screen, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IClickable clickable)) return;
        if (hit.collider.TryGetComponent(out IInspectable inspectable))
        {
            this.inspectable = inspectable;
            inspectableTransform = hit.transform;
        }


        currentClicked = clickable;
        currentClicked.OnClickedPerformed();
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (currentClicked == null) return;

        if (inspectable != null && inspectableTransform != null) { inspectService.Inspect(inspectable, inspectableTransform); }

        currentClicked.OnClickedEnd();
        currentClicked = null;

        inspectable = null;
        inspectableTransform = null;
    }
}
