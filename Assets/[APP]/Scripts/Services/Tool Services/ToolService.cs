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

    public bool isCleaning;

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
                isCleaning = true;
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
        isCleaning = false;
    }

    private void HandleMouseMoved(Vector2 vector)
    {
        if (currentTool == null) return;
        if (currentTool is IInteractObject interact)
        {
            // Debug.Log($"interact adalah {interact} apakah dia cleanable? {interact is ICleanObject}");
            var worldPos = objectDetectionService.ScreenToWorld(vector, interact);
            currentTool.FollowMouse(worldPos);

            if (surfaceDetectionService.PerformRaycast(vector, ESurfaceDetectionType.Texture))
            {
                // Debug.Log($"Surface hit? {surfaceDetectionService.HasHit}");
                StickToSurface(interact);
            }

        }
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
            Debug.Log("Surface hit " + surfaceDetectionService.CleanObject);
            cleaningService.TryCleaning(
                surfaceDetectionService.CleanObject,
                surfaceDetectionService.TextureSurface,
                currentTool.GetBrush()
            );
        }
    }

    public bool IsOnToolMode => GetCurrentTool() != null;

}