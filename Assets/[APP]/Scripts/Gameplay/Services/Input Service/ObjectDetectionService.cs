using System;
using Modules;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class ObjectDetectionService : IInitializable, IDisposable, ITickable
{
    // private readonly InputSystemService inputSystemService;
    private readonly Camera cam;
    private IInteractObject interactable;
    public Action<IInteractObject> OnInteractDetected;
    // public Action OnInteractCanceled;

    private readonly Plane plane;
    private bool isUsed = false;
    private Vector2 mousePos;

    [Inject]
    public ObjectDetectionService(InputSystemService inputSystemService, Camera cam, Plane plane)
    {
        // this.inputSystemService = inputSystemService;
        this.cam = cam;
        this.plane = plane;
    }

    public void Initialize()
    {
        // inputSystemService.OnLeftPressStarted += HandleLeftPressStarted;
        InteractionEvents.OnMouseMoved += HandleMouseMove;
        // inputSystemService.OnLeftPressEnded += HandleLeftPressEnded;
    }

    public void Dispose()
    {
        // inputSystemService.OnLeftPressStarted -= HandleLeftPressStarted;
        InteractionEvents.OnMouseMoved -= HandleMouseMove;
        // inputSystemService.OnLeftPressEnded -= HandleLeftPressEnded;
    }

    private void HandleLeftPressStarted(Vector2 vector)
    {
        OnInteractDetected?.Invoke(interactable);

        if (interactable is IDragObject)
        {
            CursorController.instance?.SetCursorState(CursorState.GrabClose);
        }
    }

    private void HandleLeftPressEnded(Vector2 vector)
    {
        if (interactable != null)
        {
            if (interactable is IDragObject) CursorController.instance?.SetCursorState(CursorState.GrabOpen);
            else CursorController.instance?.SetCursorState(CursorState.Hover);
        }
        else
        {
            CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
        }

        interactable = null;
    }

    public void SetInteractObjectUsed(bool isUsed)
    {
        if (interactable == null) return;
        this.isUsed = isUsed;
    }

    public IInteractObject GetCurrentInteract()
    {
        return interactable;
    }

    private void HandleMouseMove(Vector2 screenPos)
    {
        mousePos = screenPos;

        if (Mouse.current != null && !Mouse.current.leftButton.isPressed && !Mouse.current.rightButton.isPressed)
        {
            if (interactable != null)
            {
                if (interactable is IDragObject) CursorController.instance?.SetCursorState(CursorState.GrabOpen);
                else CursorController.instance?.SetCursorState(CursorState.Hover);
            }
            else
            {
                CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
            }
        }
    }

    public bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit);
    }

    public Vector3 ScreenToWorld(Vector2 screenPos, IInteractObject target)
    {
        if (target is Component targetComp && targetComp != null && !targetComp.Equals(null))
        {
            Camera cam = Camera.main;
            float zDistance = cam.WorldToScreenPoint(targetComp.transform.position).z;

            Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, zDistance);
            return cam.ScreenToWorldPoint(screenPosWithDepth);
        }
        return Vector3.zero;
    }

    public Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    public void Tick()
    {

        if (isUsed) return;
        IInteractObject newTarget = null;

        if (TryRaycast(mousePos, out var hit))
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        if (newTarget != interactable)
        {
            interactable?.OnInteractEnded();
            interactable = newTarget;
            newTarget?.OnInteractDetected();
            // AppLogger.Log("Mouse moved, new interactable: " + newTarget);
        }

        OnInteractDetected?.Invoke(newTarget);
    }
}