using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InspectService : IInitializable, IDisposable
{
    private readonly InteractionService interaction;
    private readonly AssemblyService assemble;
    private readonly Transform inspectPoint;
    private ArtefactPieceStateMachine currentSM;
    private Transform originalParent;

    [Inject]
    public InspectService(Transform inspectPoint, InteractionService interaction, AssemblyService assemble)
    {
        this.inspectPoint = inspectPoint;
        this.interaction = interaction;
        this.assemble = assemble;
    }

    public void Initialize()
    {
        interaction.OnClick += OnClickPerformed;
        interaction.OnHold += OnHoldPerformed;
        interaction.OnDrag += OnDragPerformed;
    }

    public void Dispose()
    {
        interaction.OnClick -= OnClickPerformed;
        interaction.OnHold -= OnHoldPerformed;
        interaction.OnDrag -= OnDragPerformed;
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

        if (currentSM != null && distance < 1.5f)
        {
            if (!assemble.TryAssemble(currentSM, sm)) Inspect(sm);
        }
        else if (distance < 1.5f)
        {
            Inspect(sm);
        }
    }

    private void OnHoldPerformed(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm)
            return;

        if (sm == currentSM)
        {
            ExitInspect();
        }
    }

    private void Inspect(ArtefactPieceStateMachine sm)
    {
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
}