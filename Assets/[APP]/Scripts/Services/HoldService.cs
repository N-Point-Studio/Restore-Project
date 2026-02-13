using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class HoldService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;
    private readonly PointerService pointer;
    private IHoldable currentHold;

    public HoldService(PlayerInputSystem inputSystem, PointerService pointer)
    {
        this.inputSystem = inputSystem;
        this.pointer = pointer;
    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);
        Input.Player.Hold.performed += OnHold;
        Input.Player.Hold.canceled += OnHoldRelease;
    }

    public void Dispose()
    {
        Input.Player.Hold.performed -= OnHold;
        Input.Player.Hold.canceled -= OnHoldRelease;
    }
    private void OnHold(InputAction.CallbackContext context)
    {
        Vector2 screen = Input.Player.ScreenPos.ReadValue<Vector2>();

        if (!pointer.TryRaycast(screen, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IHoldable holdable)) return;

        currentHold = holdable;
        currentHold.OnHoldPerformed();
    }

    private void OnHoldRelease(InputAction.CallbackContext context)
    {
        if (currentHold == null) return;
        currentHold.OnHoldEnd();
        currentHold = null;
    }
}
