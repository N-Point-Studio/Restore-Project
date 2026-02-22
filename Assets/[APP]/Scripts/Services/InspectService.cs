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
        if (sm == currentSM) return;
        Inspect(sm);
    }

    private void OnDropPerformed(IInspectable inspectable, Vector3 worldPos)
    {
        Debug.Log("Drop performed on " + inspectable);
        if (inspectable is not ArtefactPieceStateMachine sm) return;
        if (sm == currentSM) return;
        float distance = Vector3.Distance(worldPos, inspectPoint.position);

        if (distance < 1.5f)
        {
            Inspect(sm);
            // if (currentSM == null)
            // {
            //     // if (sm.parent == null)
            //     // else
            //     //     Inspect(sm.parent);
            // }
            // else if (currentSM != null)
            // {
            //     Inspect(sm);

            //     // var smChild = sm.GetComponentsInChildren<ArtefactPieceStateMachine>();
            //     // foreach (ArtefactPieceStateMachine sem in smChild)
            //     // {
            //     //     if (!assemble.TryAssemble(currentSM, sem)) Inspect(sem);
            //     // }

            // }
        }
    }

    private void OnHoldPerformed(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm)
            return;

        // assemble.Detach(sm);

        if (sm == currentSM)
        {
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
}