using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ToolService : IInitializable, IDisposable, ITickable
{
    private readonly InputSystemService inputSystemService;
    private readonly ObjectDetectionService objectDetectionService;
    private readonly SurfaceDetectionService surfaceDetectionService;
    private readonly CleaningService cleaningService;
    private ITool currentTool;
    public bool isCleaning;
    public bool isFinished = false;

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
        InteractionEvents.OnPressEnded += HandlePressEnded;

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        InteractionEvents.OnPressStarted -= HandlePressStarted;
        InteractionEvents.OnPressEnded -= HandlePressEnded;

        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
    }

    private void HandlePressStarted(IInteractObject interact)
    {
        if (isFinished) return;
        if (interact is not ITool)
        {
            if (surfaceDetectionService.HasHit)
            {
                isCleaning = true;
                var hardObject = surfaceDetectionService.HardObject;
                if (hardObject != null)
                {
                    if (currentTool.ToolType == SurfaceDetectionType.Mesh)
                    {
                        cleaningService.TryCleaningHardSurface(hardObject);
                        currentTool?.PlaySfx(true);
                    }
                }
            }
            else
            {
                currentTool?.Return();
                currentTool = null;
                isCleaning = false;
            }
        }
    }

    private void HandlePressEnded(IInteractObject interact)
    {
        if (isFinished) return;
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
        currentTool?.PlaySfx(false);
        isCleaning = false;
    }

    public ITool GetCurrentTool()
    {
        return currentTool;
    }

    private void StickToSurface(IInteractObject interact)
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;

        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);
        currentTool.StickToSurface(targetPos, targetRotation);

        if (isCleaning && surfaceDetectionService.CleanObject != null)
        {
            var cleanObject = surfaceDetectionService.CleanObject;
            if (cleanObject != null)
            {
                var texture = surfaceDetectionService.TextureSurface;
                var brush = currentTool.GetBrush();
                if (currentTool.ToolType == SurfaceDetectionType.Texture)
                {
                    cleaningService.TryCleaning(cleanObject, texture, brush);
                    currentTool.PlaySfx(true);
                }
            }
        }
    }

    public void Tick()
    {
        var mousePos = inputSystemService.GetMousePosition();
        if (currentTool == null) return;
        if (currentTool is IInteractObject interact)
        {
            // Debug.Log($"interact adalah {interact} apakah dia cleanable? {interact is ICleanObject}");
            var worldPos = objectDetectionService.ScreenToWorld(mousePos, interact);
            currentTool.FollowMouse(worldPos);

            if (surfaceDetectionService.PerformRaycast(mousePos, currentTool.ToolType))
            {
                // Debug.Log($"Surface hit? {surfaceDetectionService.HasHit}");
                StickToSurface(interact);
                // Debug.Log("hit will always run la " + surfaceDetectionService.HasHit);
            }

        }
    }

    private void HandleGameWrapped()
    {
        currentTool?.Return();
    }

    public bool IsOnToolMode => GetCurrentTool() != null;

}