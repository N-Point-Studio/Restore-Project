using System;
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
        // GestureEvents.OnDropPerformed += HandleDrop;
        GestureEvents.OnHoldPerformed += HandleHold;
        // GestureEvents.OnClickPerformed += HandleClick;

        InteractionEvents.OnDragStarted += HandleDragStarted;
        InteractionEvents.OnDragPerformed += HandleDragPerformed;
        InteractionEvents.OnDragEnded += HandleDragEnded;

    }

    public void Dispose()
    {
        // GestureEvents.OnDropPerformed -= HandleDrop;
        GestureEvents.OnHoldPerformed -= HandleHold;
        // GestureEvents.OnClickPerformed -= HandleClick;
        InteractionEvents.OnDragStarted -= HandleDragStarted;
        InteractionEvents.OnDragPerformed -= HandleDragPerformed;
        InteractionEvents.OnDragEnded -= HandleDragEnded;
        // InteractionEvents.OnDragPerformed -= HandleDrag;
    }

    private void HandleDragStarted(IInteractObject interact, Vector3 worldPos)
    {
        Debug.Log("Drag start");

        if (interact is IAssembled assembled)
        {
            var parent = assemblyService.GetAssembledRoot(assembled);
            if (parent is IDragObject drag) { drag.OnDragStarted(worldPos); }
        }
    }

    // private void HandleClick(IInteract interact, Vector2 pos)
    // {
    //     if (interact is not ArtefactPieceStateMachine sm) return;

    //     if (interact is IAssembled assembled)
    //     {
    //         var parent = assemblyService.GetAssembledRoot(assembled);
    //         if (parent is IInspectable inspect)
    //         {
    //             if (!inspect.IsInspected) inspectService.Inspect(inspect);
    //         }
    //     }
    // }

    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is not ArtefactPieceStateMachine sm) return;

        if (interact is IAssembled assembled)
        {
            var parent = assemblyService.GetAssembledRoot(assembled);
            if (parent is IDragObject drag) { drag.OnDragPerformed(worldPos); }
        }
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
        Debug.Log("Drag end");

        // if (inspectable is not ArtefactPieceStateMachine incoming) return;
        // if (incoming.GetCurrentState() is not IDrag) return;
        // if (incoming == inspectService.GetCurrentInspected()) return;
        if (interact is not IInspectable inspectable) return;

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
        else
        {
            if (interact is IAssembled assembled)
            {
                var parent = assemblyService.GetAssembledRoot(assembled);
                if (parent is IDragObject drag) { drag.OnDragEnded(worldPos); }
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