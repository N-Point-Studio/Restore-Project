using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ObjectDetectionService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly Camera cam;
    private IInteractObject interactable;
    public Action<IInteractObject> OnInteractDetected;
    public Action OnInteractCanceled;

    private readonly Plane plane;

    [Inject]
    public ObjectDetectionService(InputSystemService inputSystemService, Camera cam, Plane plane)
    {
        this.inputSystemService = inputSystemService;
        this.cam = cam;
        this.plane = plane;
    }

    public void Initialize()
    {
        inputSystemService.OnLeftPressStarted += HandleLeftPressStarted;
        inputSystemService.OnMouseMoved += HandleMouseMove;
        inputSystemService.OnLeftPressEnded += HandleLeftPressEnded;
    }

    public void Dispose()
    {
        inputSystemService.OnLeftPressStarted -= HandleLeftPressStarted;
        inputSystemService.OnMouseMoved -= HandleMouseMove;
        inputSystemService.OnLeftPressEnded -= HandleLeftPressEnded;
    }

    private void HandleLeftPressStarted(Vector2 vector)
    {
        OnInteractDetected?.Invoke(interactable);
    }

    private void HandleLeftPressEnded(Vector2 vector)
    {
        interactable = null;
    }

    private void HandleMouseMove(Vector2 screenPos)
    {
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
}