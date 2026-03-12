using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ArtefactManager : IInitializable, IDisposable
{
    private readonly InspectService inspectService;
    private readonly ToolService toolService;

    [Inject]
    public ArtefactManager(InspectService inspectService, ToolService toolService)
    {
        this.inspectService = inspectService;
        this.toolService = toolService;
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
        if (interact is IDragObject drag) { drag.OnDragStarted(worldPos); }
    }

    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IDragObject drag) { drag.OnDragPerformed(worldPos); }
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
        if (toolService.IsOnToolMode) return;

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
                inspectService.Inspect(inspectable);
            }
        }
        else
        {
            if (interact is IDragObject drag) { drag.OnDragEnded(worldPos); }
        }
    }

    private void HandleHoldPerformed(IInteractObject interact, float obj) { }

    private void HandleHoldCompleted(IInteractObject interact)
    {
        if (toolService.IsOnToolMode) return;
        var currentInspect = inspectService.GetCurrentInspected();

        if (interact as IInspectable == currentInspect)
        {
            inspectService.ExitInspect();
            return;
        }
    }

    private void HandleHoldCanceled(IInteractObject interact) { }
}