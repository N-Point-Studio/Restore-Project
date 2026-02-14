using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public class DragService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;
    private readonly PointerService pointer;
    private readonly InspectionService inspectService;
    private IDraggable currentDrag;
    private IInspectable inspectable;
    private Transform inspectableTransform;

    private float depth;
    private Vector3 offset;

    public DragService(PlayerInputSystem inputSystem, PointerService pointer, InspectionService inspectService)
    {
        this.inputSystem = inputSystem;
        this.pointer = pointer;
        this.inspectService = inspectService;
    }

    public void Initialize()
    {
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

        if (!pointer.TryRaycast(screen, out var hit)) return;
        if (!hit.collider.TryGetComponent(out IDraggable draggable)) return;
        if (hit.collider.TryGetComponent(out IInspectable inspectable))
        {
            this.inspectable = inspectable;
            inspectableTransform = hit.transform;
        }

        currentDrag = draggable;

        depth = pointer.GetDepth(hit.transform.position);
        offset = hit.transform.position - pointer.ScreenToWorld(screen, depth);

        currentDrag.OnDragStart(hit.point);
    }

    private void OnDrag(InputAction.CallbackContext context)
    {
        if (currentDrag == null) return;

        Vector2 screen = context.ReadValue<Vector2>();
        Vector3 world = pointer.ScreenToWorld(screen, depth) + offset;

        currentDrag.OnDragPerformed(world);
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (currentDrag == null) return;
        if (inspectable != null && inspectableTransform != null) { inspectService.Inspect(inspectable, inspectableTransform); }

        Vector2 screen = context.ReadValue<Vector2>();
        currentDrag.OnDragEnd(screen);
        currentDrag = null;

        inspectable = null;
        inspectableTransform = null;
    }
}