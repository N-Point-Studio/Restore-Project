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

    [Inject]
    public ObjectDetectionService(InputSystemService inputSystemService, Camera cam)
    {
        this.inputSystemService = inputSystemService;
        this.cam = cam;
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
        Debug.Log("Pressed " + interactable);
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