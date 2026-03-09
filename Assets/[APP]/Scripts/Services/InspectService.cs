using UnityEngine;
using VContainer;

public class InspectService
{
    private readonly Inspection inspectPoint;
    private IInspectable currentInspect;
    private Transform originalParent;

    [Inject]
    public InspectService(Inspection inspectPoint)
    {
        this.inspectPoint = inspectPoint;
    }

    public IInspectable GetCurrentInspected() => currentInspect;
    public Transform GetInspectPoint() => inspectPoint.transform;

    public void Inspect(IInspectable inspectable)
    {
        if (currentInspect == inspectable) return;

        if (currentInspect != null)
        {
            ExitInspect();
        }

        currentInspect = inspectable;
        originalParent = inspectable.GetTransform().parent;

        inspectPoint.ResetPosition();

        inspectable.GetTransform().SetParent(inspectPoint.transform);
        inspectable.EnterInspect(inspectPoint.transform);

    }

    public void ExitInspect()
    {
        if (currentInspect == null) return;

        currentInspect.GetTransform().SetParent(originalParent, true);
        currentInspect.ExitInspect();

        currentInspect = null;
        originalParent = null;

        inspectPoint.ResetPosition();
    }
}