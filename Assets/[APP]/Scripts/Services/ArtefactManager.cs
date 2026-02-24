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
    }

    public void Dispose()
    {
        GestureEvents.OnDropPerformed -= HandleDrop;
        GestureEvents.OnHoldPerformed -= HandleHold;
        GestureEvents.OnClickPerformed -= HandleClick;
    }

    ArtefactPieceStateMachine GetTopParent(ArtefactPieceStateMachine current)
    {
        while (current.parent != null)
        {
            current = current.parent;
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

    private void HandleDrop(IInspectable inspectable, Vector3 worldPos)
    {
        if (inspectable is not ArtefactPieceStateMachine incoming) return;
        if (incoming.GetCurrentState() is not IDrag) return;
        if (incoming == inspectService.GetCurrentInspected()) return;

        float distance = Vector3.Distance(worldPos, inspectService.GetInspectPoint().position);
        if (distance < 1.5f)
        {
            if (inspectService.GetCurrentInspected() == null)
            {
                inspectService.Inspect(GetTopParent(incoming));
            }
            else
            {
                if (!assemblyService.TryAssemble(inspectService.GetCurrentInspected(), incoming))
                {
                    inspectService.Inspect(GetTopParent(incoming));
                }
            }
        }
    }

    private void HandleHold(IInteract interact)
    {
        if (interact is not ArtefactPieceStateMachine sm) return;

        var currentInspect = inspectService.GetCurrentInspected();

        if (sm == currentInspect)
        {
            // Debug.Log("[Test] Hold on current inspect, try to detach or exit inspect");
            // var assemble = assemblyService.TryAssembleParent(sm);
            // if (assemble != null)
            // {
            // Debug.Log($"[Test] Assembled {assemble.name} as new inspect");
            // inspectService.InspectNewChild(assemble);
            // }
            inspectService.ExitInspect();
            return;
        }
        else if (sm.parent != null && assemblyService.GetTopParent(sm) == currentInspect)
        {
            assemblyService.Detach(sm);
            inspectService.Inspect(sm);
        }
    }
}