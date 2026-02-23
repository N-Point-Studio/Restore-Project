using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InspectService : IInitializable, IDisposable
{
    private readonly AssemblyService assemble;
    private readonly Transform inspectPoint;
    private ArtefactPieceStateMachine currentSM;
    private Transform originalParent;

    [Inject]
    public InspectService(Transform inspectPoint, AssemblyService assemble)
    {
        this.inspectPoint = inspectPoint;
        this.assemble = assemble;
    }

    public void Initialize()
    {
        GestureEvents.OnDropPerformed += OnDropPerformed;
        GestureEvents.OnClickPerformed += OnClickPerformed;
        GestureEvents.OnHoldPerformed += OnHoldPerformed;
    }
    public void Dispose()
    {
        GestureEvents.OnDropPerformed -= OnDropPerformed;
        GestureEvents.OnClickPerformed -= OnClickPerformed;
        GestureEvents.OnHoldPerformed -= OnHoldPerformed;
    }

    public void ResetInspectPoint() { inspectPoint.SetPositionAndRotation(Vector3.zero, Quaternion.identity); }

    private void OnClickPerformed(IInspectable inspectable, Vector2 vector)
    {
        if (inspectable is not ArtefactPieceStateMachine sm) return;
        // if (sm.GetCurrentState() is not IClick) return;
        if (sm == currentSM) return;

        var topParent = GetTopParent(sm);
        if (topParent.GetCurrentStateEnum() == ArtefactPieceState.Idle)
        {
            Debug.Log("Clicked on " + topParent.name);
            Inspect(topParent);
        }
    }

    private void OnDropPerformed(IInspectable inspectable, Vector3 worldPos)
    {
        Debug.Log("Drop performed on " + inspectable);
        if (inspectable is not ArtefactPieceStateMachine sm) return;
        if (sm.GetCurrentState() is not IDrag) return;
        if (sm == currentSM) return;
        float distance = Vector3.Distance(worldPos, inspectPoint.position);

        if (distance < 1.5f)
        {
            if (currentSM == null)
            {
                Inspect(sm);
            }
            else if (currentSM != null)
            {
                Debug.Log("ada yg diinspect");
                if (!assemble.TryAssemble(currentSM, sm)) Inspect(sm);
            }
        }
    }

    ArtefactPieceStateMachine GetTopParent(ArtefactPieceStateMachine current)
    {
        while (current.parent != null)
        {
            current = current.parent;
        }

        return current;
    }

    private void OnHoldPerformed(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm)
            return;

        if (sm == currentSM)
        {
            // if (TryCheckInspect(sm)) { }
            ExitInspect();
        }
    }

    private void Inspect(ArtefactPieceStateMachine sm)
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
        sm.GetInspectable()?.ExitInspect();

        currentSM = null;
        originalParent = null;
    }

    public bool TryCheckInspect(ArtefactPieceStateMachine sm)
    {
        var children = sm.GetComponentsInChildren<ArtefactPieceStateMachine>();
        Debug.Log($"Children count: {children.Length}");
        if (children.Length <= 1) return false;
        return true;
    }
}