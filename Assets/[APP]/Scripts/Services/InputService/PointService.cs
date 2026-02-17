using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PointService : IInitializable, IDisposable
{
    private readonly GestureService gesture;
    private readonly Camera cam;
    private IInteract interactable;
    private bool isInteracting;

    [Inject]
    public PointService(GestureService gesture, Camera cam)
    {
        Debug.Log("Point Service");
        this.gesture = gesture;
        this.cam = cam;
    }

    public void Initialize()
    {
        gesture.OnPrimaryStarted += PressStart;
        gesture.OnPrimaryEnded += PressEnd;
    }

    public void Dispose()
    {
        gesture.OnPrimaryStarted -= PressStart;
        gesture.OnPrimaryEnded -= PressEnd;
    }

    private void PressStart(Vector2 vector)
    {
        isInteracting = true;
        if (TryRaycast(vector, out var hit))
        {
            if (!hit.collider.TryGetComponent(out IInteract interactable)) return;
            this.interactable = interactable;
        }
        else
        {
            interactable = null;
        }
    }

    private void PressEnd(Vector2 vector)
    {
        isInteracting = false;
    }

    public bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit);
    }

    public IInteract GetInteractObject()
    {
        return interactable;
    }
}
