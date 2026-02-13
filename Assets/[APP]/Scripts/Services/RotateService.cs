using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class RotateService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private readonly PointerService pointer;
    private GameInput Input => inputSystem.Input;

    private IRotateable currentRotate;
    private Vector2 lastScreenPos;
    private bool isRotating;

    public RotateService(PlayerInputSystem inputSystem, PointerService pointer)
    {
        this.inputSystem = inputSystem;
        this.pointer = pointer;
    }

    public void Initialize()
    {
        Input.Player.Press.started += OnPress;
        Input.Player.ScreenPos.performed += OnMove;
        Input.Player.Press.canceled += OnRelease;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= OnPress;
        Input.Player.ScreenPos.performed -= OnMove;
        Input.Player.Press.canceled -= OnRelease;
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        Vector2 screen = Input.Player.ScreenPos.ReadValue<Vector2>();

        if (!pointer.TryRaycast(screen, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IRotateable rotateable)) return;

        currentRotate = rotateable;
        lastScreenPos = screen;
        isRotating = true;

        currentRotate.OnRotateStarted();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!isRotating || currentRotate == null) return;

        Vector2 current = context.ReadValue<Vector2>();
        Vector2 delta = current - lastScreenPos;
        lastScreenPos = current;

        currentRotate.OnRotatePerformed(delta);
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (!isRotating || currentRotate == null) return;

        currentRotate.OnRotateEnd();
        currentRotate = null;
        isRotating = false;
    }
}