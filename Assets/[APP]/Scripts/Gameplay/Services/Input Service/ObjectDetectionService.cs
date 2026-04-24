using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class ObjectDetectionService : IInitializable, IDisposable, ITickable
{
    private readonly Camera cam;
    private IInteractObject interactable;
    public Action<IInteractObject> OnInteractDetected;

    private readonly Plane plane;
    private bool isUsed = false;
    private Vector2 mousePos;

    [Inject]
    public ObjectDetectionService(InputSystemService inputSystemService, Camera cam, Plane plane)
    {
        this.cam = cam;
        this.plane = plane;
    }

    public void Initialize()
    {
        InteractionEvents.OnMouseMoved += HandleMouseMove;
    }

    public void Dispose()
    {
        InteractionEvents.OnMouseMoved -= HandleMouseMove;
    }

    public void SetInteractObjectUsed(bool isUsed)
    {
        OnInteractDetected?.Invoke(interactable);

        if (interactable is IDragObject)
        {
            CursorController.instance?.SetCursorState(CursorState.GrabClose);
        }
        if (interactable == null) return;
        this.isUsed = isUsed;
    }

    public IInteractObject GetCurrentInteract()
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
        return interactable;
    }

    private void HandleMouseMove(Vector2 screenPos)
    {
        // This prevents dropping the artefact or tool if the finger slips off the collider.
        if (isUsed)
        {
            mousePos = screenPos; // Keep updating position for drag math
            return;
        }

        IInteractObject newTarget = null;

        if (TryRaycast(screenPos, out var hit))
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        if (newTarget != interactable)
        {
            interactable?.OnInteractEnded();
            interactable = newTarget;
            newTarget?.OnInteractDetected();

            // Touch screens send Position and Press in the exact same frame. 
            // If we wait for Tick() to notify the manager, the Press event will be ignored!
            OnInteractDetected?.Invoke(newTarget);
        }

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
        mousePos = screenPos;
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
            Camera camera = this.cam != null ? this.cam : Camera.main;
            float zDistance = camera.WorldToScreenPoint(targetComp.transform.position).z;

            Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, zDistance);
            return camera.ScreenToWorldPoint(screenPosWithDepth);
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

            OnInteractDetected?.Invoke(newTarget);
        }
    }
}