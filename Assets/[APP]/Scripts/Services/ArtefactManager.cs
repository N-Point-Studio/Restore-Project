using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ArtefactManager : IInitializable, IDisposable
{
    private readonly InspectService inspectService;
    private readonly AssemblyService assemblyService;

    [Inject]
    public ArtefactManager(InspectService inspectService, AssemblyService assemblyService)
    {
        this.inspectService = inspectService;
        this.assemblyService = assemblyService;
    }

    public void Initialize()
    {
        GestureEvents.OnDropPerformed += HandleDrop;
        GestureEvents.OnHoldPerformed += HandleHold;
        GestureEvents.OnClickPerformed += HandleClick;
        GestureEvents.OnDragPerformed += HandleDrag;
    }

    public void Dispose()
    {
        GestureEvents.OnDropPerformed -= HandleDrop;
        GestureEvents.OnHoldPerformed -= HandleHold;
        GestureEvents.OnClickPerformed -= HandleClick;
        GestureEvents.OnDragPerformed -= HandleDrag;
    }


    ArtefactPieceStateMachine GetTopParent(ArtefactPieceStateMachine current)
    {
        while (current.parent != null)
        {
            // current = current.parent;
        }
        return current;
    }

    private void HandleClick(IInspectable inspectable, Vector2 pos)
    {
        if (inspectable is not ArtefactPieceStateMachine sm) return;

        var topParent = GetTopParent(sm);
        if (topParent.GetCurrentStateEnum() == ArtefactPieceState.Idle)
        {
            inspectService.Inspect(topParent);
        }
    }

    private void HandleDrag(IInteract interact, Vector3? nullable)
    {
        if (interact is not ArtefactPieceStateMachine sm) return;
        if (inspectService.GetCurrentInspected() == null) return;
    }

    private void HandleDrop(IInspectable inspectable, Vector3 worldPos)
    {
        // if (inspectable is not ArtefactPieceStateMachine incoming) return;
        // if (incoming.GetCurrentState() is not IDrag) return;
        // if (incoming == inspectService.GetCurrentInspected()) return;

        float distance = Vector3.Distance(worldPos, inspectService.GetInspectPoint().position);
        if (distance < 1.5f)
        {
            if (inspectService.GetCurrentInspected() == null)
            {
                inspectService.Inspect(inspectable);
            }
            else
            {
                if (!assemblyService.TryAssemble(inspectService.GetCurrentInspected() as IAssembled, inspectable as IAssembled))
                {
                    inspectService.Inspect(inspectable);
                    // inspectService.Inspect(GetTopParent(inspectable as ArtefactPieceStateMachine));
                }
            }
        }
    }

    private void HandleHold(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm) return;

        var currentInspect = inspectService.GetCurrentInspected();

        if (sm as IInspectable == currentInspect)
        {
            inspectService.ExitInspect();
            return;
        }
        else if (sm.parent != null)
        {
            assemblyService.Detach(sm);
        }
    }
}