using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PointService : IInitializable, IDisposable
{
    private readonly InputService gesture;
    private readonly Camera cam;
    private IInteract interactable;

    public Action<IInteract> OnInteractDetected;

    [Inject]
    public PointService(InputService gesture, Camera cam)
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
        if (TryRaycast(vector, out var hit))
        {
            if (!hit.collider.TryGetComponent(out IInteract interactable)) return;
            this.interactable = interactable;
        }
        else
        {
            interactable = null;
        }

        OnInteractDetected?.Invoke(interactable);
    }

    private void PressEnd(Vector2 vector) { interactable = null; }

    public bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit);
    }

    public Vector3 ScreenToWorld(Vector2 screenPos, IInteract target)
    {
        if (target is Component targetComp)
        {
            Camera cam = Camera.main;
            float zDistance = cam.WorldToScreenPoint(targetComp.transform.position).z;

            Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, zDistance);
            return cam.ScreenToWorldPoint(screenPosWithDepth);
        }
        return Vector3.zero;
    }
}
