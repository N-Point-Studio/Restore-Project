using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class InspectionService : IInitializable, IDisposable
{
    private IInspectable currentInspectable;
    public IInspectable CurrentInspected => currentInspectable;
    private Transform currentTransform;
    private readonly Transform inspectRoot;
    private readonly IInteractionEvent eventBus;

    public InspectionService(IInteractionEvent eventBus, Transform inspectRoot)
    {
        Debug.Log("Inspect");
        this.eventBus = eventBus;
        this.inspectRoot = inspectRoot;
    }
    public void Initialize()
    {
        eventBus.OnObjectDropped += HandleObjectDropped;
    }
    public void Dispose()
    {
        eventBus.OnObjectDropped -= HandleObjectDropped;
    }
    private void HandleObjectDropped(IInteractable target, Vector3 worldPos, bool isClick)
    {
        if (target is IInspectable inspectable)
        {
            if (isClick || IsNearInspectZone(inspectable.Transform))
            {
                Inspect(inspectable, inspectable.Transform);
            }
        }
    }

    public void Inspect(IInspectable inspectable, Transform targetTransform)
    {
        if (currentInspectable != null)
            ExitInspect();

        currentInspectable = inspectable;
        currentTransform = targetTransform;

        targetTransform.SetParent(inspectRoot);
        targetTransform.localPosition = Vector3.zero;
        targetTransform.localRotation = Quaternion.identity;

        inspectable.EnterInspect();
    }

    public void ExitInspect()
    {
        if (currentInspectable == null) return;

        currentTransform.SetParent(null);
        currentInspectable.ExitInspect();

        currentInspectable = null;
        currentTransform = null;
    }

    public bool IsNearInspectZone(Transform target)
    {
        float distance = Vector3.Distance(target.position, inspectRoot.position);
        return distance < 1.5f;
    }
}