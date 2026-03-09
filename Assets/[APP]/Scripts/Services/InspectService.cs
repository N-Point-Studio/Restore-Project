using UnityEngine;
using VContainer;

public class InspectService
{
    private readonly Transform inspectPoint;
    private IInspectable currentInspect;
    private Transform originalParent;

    [Inject]
    public InspectService(Transform inspectPoint)
    {
        this.inspectPoint = inspectPoint;
    }

    public IInspectable GetCurrentInspected() => currentInspect;
    public Transform GetInspectPoint() => inspectPoint;
    public void ResetInspectPoint() { inspectPoint.SetPositionAndRotation(Vector3.zero, Quaternion.identity); }
    public void Inspect(IInspectable inspectable)
    {
        if (currentInspect == inspectable) return;

        if (currentInspect != null)
        {
            ExitInspect();
        }

        currentInspect = inspectable;
        originalParent = inspectable.GetTransform().parent;
        inspectable.GetTransform().SetParent(inspectPoint);
        inspectable.EnterInspect(inspectPoint.transform);
        ResetInspectPoint();
    }

    public void ExitInspect()
    {
        if (currentInspect == null) return;

        currentInspect.GetTransform().SetParent(originalParent, true);
        currentInspect.ExitInspect();

        currentInspect = null;
        originalParent = null;
    }
}