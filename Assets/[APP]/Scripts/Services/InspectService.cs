using UnityEngine;
using VContainer;

public class InspectService
{
    private readonly Transform inspectPoint;
    private ArtefactPieceStateMachine currentSM;
    private Transform originalParent;

    [Inject]
    public InspectService(Transform inspectPoint)
    {
        this.inspectPoint = inspectPoint;
    }

    public ArtefactPieceStateMachine GetCurrentInspected() => currentSM;
    public void ResetInspectPoint() { inspectPoint.SetPositionAndRotation(Vector3.zero, Quaternion.identity); }
    public Transform GetInspectPoint() => inspectPoint;

    public void InspectNewChild(ArtefactPieceStateMachine child)
    {
        ExitInspect();

        currentSM = child;
        originalParent = child.transform;

        Debug.Log($"[Test] InspectNewChild: {child.name}");
        child.transform.SetParent(inspectPoint);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        ResetInspectPoint();
        child.GetInspectable()?.EnterInspect();
    }

    public void Inspect(ArtefactPieceStateMachine sm)
    {

        if (sm.GetCurrentState() is not IAssembled) return;
        if (currentSM == sm)
            return;

        if (currentSM != null)

            ExitInspect();

        currentSM = sm;

        originalParent = sm.transform.parent;

        sm.transform.SetParent(inspectPoint);
        sm.transform.localPosition = Vector3.zero;
        sm.transform.localRotation = Quaternion.identity;
        ResetInspectPoint();
        sm.GetInspectable()?.EnterInspect();
    }

    public void ExitInspect()
    {
        if (currentSM == null)
            return;

        var sm = currentSM;

        // Kembalikan parent dulu
        sm.transform.SetParent(originalParent, true);

        // Beritahu state machine
        Debug.Log("Exit Inspect: " + sm.name);
        sm.GetInspectable()?.ExitInspect();

        currentSM = null;
        originalParent = null;
    }
}