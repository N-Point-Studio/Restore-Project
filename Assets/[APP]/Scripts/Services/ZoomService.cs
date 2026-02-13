using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class ZoomService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private readonly PointerService pointer;
    private GameInput Input => inputSystem.Input;

    private IZoomable currentZoom;
    private float lastDistance;
    private bool isZooming;

    public ZoomService(PlayerInputSystem inputSystem, PointerService pointer)
    {
        this.inputSystem = inputSystem;
        this.pointer = pointer;
    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);

        Input.Player.SecondaryFingerPress.performed += OnPress;
        Input.Player.SecondaryFingerPress.canceled += OnRelease;

        Input.Player.ScreenPos.performed += OnZoom;
        Input.Player.SecondaryFingerPos.performed += OnZoom;
    }

    public void Dispose()
    {
        Input.Player.SecondaryFingerPress.performed -= OnPress;
        Input.Player.SecondaryFingerPress.canceled -= OnRelease;

        Input.Player.ScreenPos.performed -= OnZoom;
        Input.Player.SecondaryFingerPos.performed -= OnZoom;
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        Vector2 primary = Input.Player.ScreenPos.ReadValue<Vector2>();
        Vector2 secondary = Input.Player.SecondaryFingerPos.ReadValue<Vector2>();

        if (!pointer.TryRaycast(primary, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IZoomable zoomable)) return;

        currentZoom = zoomable;
        isZooming = true;

        lastDistance = Vector2.Distance(primary, secondary);

        currentZoom.OnZoomStart();
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        if (!isZooming || currentZoom == null) return;

        Vector2 primary = Input.Player.ScreenPos.ReadValue<Vector2>();
        Vector2 secondary = Input.Player.SecondaryFingerPos.ReadValue<Vector2>();

        float currentDistance = Vector2.Distance(primary, secondary);
        float delta = currentDistance - lastDistance;

        if (Mathf.Abs(delta) < 0.01f) return;

        lastDistance = currentDistance;

        Debug.Log($"Secondary zoom {delta}");

        currentZoom.OnZoomPerformed(delta);
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        if (!isZooming || currentZoom == null) return;
        currentZoom.OnZoomEnd();
        currentZoom = null;
        isZooming = false;
    }
}