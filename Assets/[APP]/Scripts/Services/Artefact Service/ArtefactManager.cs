using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ArtefactManager : IInitializable, IDisposable
{
    private readonly InspectService inspectService;
    private readonly ToolService toolService;
    private readonly AssemblyService assemblyService;
    // private readonly ObjectDetectionService objectDetectionService;

    // private IArtefactSlot slotDetected;

    [Inject]
    public ArtefactManager(InspectService inspectService, ToolService toolServic, AssemblyService assemblyService, ObjectDetectionService objectDetectionService)
    {
        this.inspectService = inspectService;
        this.assemblyService = assemblyService;
        this.toolService = toolServic;
        // this.objectDetectionService = objectDetectionService;
    }

    public void Initialize()
    {
        // objectDetectionService.OnInteractDetected += HandleInteractDetected;

        InteractionEvents.OnHoldPerformed += HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted += HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled += HandleHoldCanceled;

        InteractionEvents.OnDragStarted += HandleDragStarted;
        InteractionEvents.OnDragPerformed += HandleDragPerformed;
        InteractionEvents.OnDragEnded += HandleDragEnded;
    }

    public void Dispose()
    {
        // objectDetectionService.OnInteractDetected += HandleInteractDetected;

        InteractionEvents.OnHoldPerformed -= HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted -= HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled -= HandleHoldCanceled;

        InteractionEvents.OnDragStarted -= HandleDragStarted;
        InteractionEvents.OnDragPerformed -= HandleDragPerformed;
        InteractionEvents.OnDragEnded -= HandleDragEnded;
    }
    // private void HandleInteractDetected(IInteractObject interact)
    // {
    //     if (interact is IArtefactSlot slot)
    //     {
    //         slotDetected = slot;
    //     }
    //     else
    //     {
    //         slotDetected = null;
    //     }
    // }

    private void HandleDragStarted(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IDragObject drag) { drag.OnDragStarted(worldPos); }
        // if (interact is IInteractObject interactObject) interactObject.SetColliderEnable(false);
    }

    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IDragObject drag) { drag.OnDragPerformed(worldPos); }
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
        if (toolService.IsOnToolMode) return;
        Debug.Log($"hit di {toolService.IsOnToolMode}");
        if (interact is not IInspectable inspectable) return;

        float distance = Vector3.Distance(worldPos, assemblyService.GetInspectPoint().position);
        if (distance < 1.5f)
        {
            if (!assemblyService.TryAssemble(inspectable as IArtefactPart))
            {
                if (interact is IDragObject drag) { drag.OnDragEnded(worldPos); }
            }
            // if (inspectService.IsInspectEmpty())
            // {
            //     // Debug.Log("currentinspect null");
            //     inspectService.Inspect(inspectable);
            // }
            // else
            // {
            //     // if (slotDetected != null)
            //     // {
            //     //     Debug.Log("Harusnya attach di sini dengan " + slotDetected.TargetPieceId);
            //     // }
            //     if (interact is IDragObject drag) { drag.OnDragEnded(worldPos); }

            //     // Debug.Log("currentinspect gak null");

            //     // if (!assemblyService.TryAssemblePart(inspectService.GetCurrentInspected() as IAssemble, inspectable as IAssemble))
            //     // {
            //     //     inspectService.Inspect(inspectable);
            //     //     // if (interact is IDragObject drag) { drag.OnDragEnded(worldPos); }
            //     // }
            // }
        }
        else
        {
            if (interact is IDragObject drag) { drag.OnDragEnded(worldPos); }
        }

        // if (interact is IInteractObject interactObject) interactObject.SetColliderEnable(true);
    }

    private void HandleHoldPerformed(IInteractObject interact, float obj) { }

    private void HandleHoldCompleted(IInteractObject interact)
    {
        if (toolService.IsOnToolMode) return;
        // var currentInspect = inspectService.GetCurrentInspected();

        // if (interact as IInspectable == currentInspect)
        // {
        //     inspectService.ExitInspect();
        //     return;
        // }

        // if (interact is IInspectable inspect)
        // {
        //     inspectService.ExitInspect(inspect);
        //     return;
        // }

        if (interact is IArtefactPart part)
        {
            // inspectService.ExitInspect(part);
            assemblyService.Detach(part);
            return;
        }
    }

    private void HandleHoldCanceled(IInteractObject interact) { }
}