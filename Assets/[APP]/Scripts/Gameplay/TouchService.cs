using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class TouchService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;
    private Camera cam;
    private IDraggable current;
    private float depth;
    private Vector3 offset;

    public TouchService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {
        cam = Camera.main;
        inputSystem.ChangeInputState(InputStateType.Player);

        Input.Player.Press.performed += OnPress;
        Input.Player.Press.canceled += OnRelease;
        Input.Player.ScreenPos.performed += OnDrag;
    }

    public void Dispose()
    {
        Input.Player.Press.performed -= OnPress;
        Input.Player.Press.canceled -= OnRelease;
        Input.Player.ScreenPos.performed -= OnDrag;
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        Vector2 screen = Input.Player.ScreenPos.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screen);

        if (!Physics.Raycast(ray, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IDraggable draggable)) return;

        current = draggable;

        depth = cam.WorldToScreenPoint(hit.transform.position).z;
        offset = hit.transform.position - ScreenToWorld(screen);

        current.OnDragStart(hit.point);
    }

    private void OnDrag(InputAction.CallbackContext context)
    {
        if (current == null) return;

        Vector2 screen = context.ReadValue<Vector2>();
        current.OnDrag(ScreenToWorld(screen) + offset);
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (current == null) return;

        current.OnDragEnd();
        current = null;
    }

    private Vector3 ScreenToWorld(Vector2 screen)
    {
        return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
    }
}