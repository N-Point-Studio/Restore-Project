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
        InteractionEvents.OnHoldPerformed += HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted += HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled += HandleHoldCanceled;

        InteractionEvents.OnDragStarted += HandleDragStarted;
        InteractionEvents.OnDragPerformed += HandleDragPerformed;
        InteractionEvents.OnDragEnded += HandleDragEnded;
    }

    public void Dispose()
    {
        InteractionEvents.OnHoldPerformed -= HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted -= HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled -= HandleHoldCanceled;

        InteractionEvents.OnDragStarted -= HandleDragStarted;
        InteractionEvents.OnDragPerformed -= HandleDragPerformed;
        InteractionEvents.OnDragEnded -= HandleDragEnded;
    }

    private void HandleDragStarted(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IAssembled assembled)
        {
            var parent = assemblyService.GetAssembledRoot(assembled);
            if (parent is IDragObject drag) { drag.OnDragStarted(worldPos); }
        }
    }


    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is not ArtefactPieceStateMachine) return;

        if (interact is IAssembled assembled)
        {
            var parent = assemblyService.GetAssembledRoot(assembled);
            if (parent is IDragObject drag) { drag.OnDragPerformed(worldPos); }
        }
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
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

    private void HandleHoldPerformed(IInteractObject interact, float obj)
    {
    }

    private void HandleHoldCompleted(IInteractObject interact)
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

    private void HandleHoldCanceled(IInteractObject interact)
    {
    }

}