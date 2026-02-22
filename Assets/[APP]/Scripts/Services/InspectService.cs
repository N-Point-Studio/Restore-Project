using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InspectService : IInitializable, IDisposable
{
    // private readonly InteractionService interaction;
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
        // interaction.OnClick += OnClickPerformed;
        // interaction.OnHold += OnHoldPerformed;
        // interaction.OnDrag += OnDragPerformed;
    }

    public void Dispose()
    {
        // interaction.OnClick -= OnClickPerformed;
        // interaction.OnHold -= OnHoldPerformed;
        // interaction.OnDrag -= OnDragPerformed;
    }

    private void OnClickPerformed(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm)
            return;

        if (!sm.IsInspectable)
            return;

        Inspect(sm);
    }

    private void OnDragPerformed(IInteract interact, Vector3 worldPos)
    {
        if (interact is not ArtefactPieceStateMachine sm) return;
        if (sm == currentSM) return;
        float distance = Vector3.Distance(sm.transform.position, inspectPoint.position);

        if (distance < 1.5f)
        {
            if (currentSM == null)
            {
                if (sm.parent == null)
                    Inspect(sm);
                else
                    Inspect(sm.parent);
            }
            else if (currentSM != null)
            {
                var smChild = sm.GetComponentsInChildren<ArtefactPieceStateMachine>();
                foreach (ArtefactPieceStateMachine sem in smChild)
                {
                    if (!assemble.TryAssemble(currentSM, sem)) Inspect(sem);
                }

            }
        }
    }

    private void OnHoldPerformed(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm)
            return;

        assemble.Detach(sm);

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

    public ArtefactPieceStateMachine CheckParent(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm) return null;

        if (sm.GetCurrentState() is IAssembled)
        {
            return sm.parent;
        }
        else
        {
            return sm;
        }
    }
}