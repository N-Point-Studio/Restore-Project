using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ToolService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly ObjectDetectionService objectDetectionService;
    private readonly SurfaceDetectionService surfaceDetectionService;
    private readonly CleaningService cleaningService;
    private ITool currentTool;

    [Inject]
    public ToolService(InputSystemService inputSystemService, ObjectDetectionService objectDetectionService,
    SurfaceDetectionService surfaceDetectionService, CleaningService cleaningService)
    {
        this.inputSystemService = inputSystemService;
        this.objectDetectionService = objectDetectionService;
        this.surfaceDetectionService = surfaceDetectionService;
        this.cleaningService = cleaningService;
    }

    public void Initialize()
    {
        InteractionEvents.OnPressStarted += HandlePressStarted;
        inputSystemService.OnMouseMoved += HandleMouseMoved;
        InteractionEvents.OnPressEnded += HandlePressEnded;
    }

    public void Dispose()
    {
        InteractionEvents.OnPressStarted -= HandlePressStarted;
        inputSystemService.OnMouseMoved -= HandleMouseMoved;
        InteractionEvents.OnPressEnded -= HandlePressEnded;
    }

    private void HandlePressStarted(IInteractObject interact)
    {
        if (interact is not ITool)
        {
            if (surfaceDetectionService.HasHit)
            {
                cleaningService.TryCleaning();
            }
            else
            {
                currentTool?.Return();
                currentTool = null;
            }
        }
    }

    private void HandlePressEnded(IInteractObject interact)
    {
        if (interact is ITool tool)
        {
            if (currentTool != tool)
            {
                currentTool?.Return();
                currentTool = tool;
                currentTool?.Use();
            }
        }

        cleaningService.EndClean();
    }

    private void HandleMouseMoved(Vector2 vector)
    {
        if (currentTool == null) return;
        if (currentTool is IInteractObject interact)
        {
            var worldPos = objectDetectionService.ScreenToWorld(vector, interact);
            currentTool.FollowMouse(worldPos);

            surfaceDetectionService.PerformRaycast(vector, ESurfaceDetectionType.Texture);
            if (surfaceDetectionService.HasHit)
            {
                StickToSurface();
            }

            Debug.Log($"Surface hit? {surfaceDetectionService.HasHit}");
        }
    }

    public ITool GetCurrentTool()
    {
        return currentTool;
    }

    private void StickToSurface()
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;
        //up bisa ganti ke forward
        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);
        currentTool.StickToSurface(targetPos, targetRotation);
    }

    public bool IsOnToolMode => GetCurrentTool() != null;

}