using System;
using Modules;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ToolService : IInitializable, IDisposable
{
    private readonly ObjectDetectionService objectDetectionService;
    private readonly SurfaceDetectionService surfaceDetectionService;
    private readonly CleaningService cleaningService;
    private ITool currentTool;
    public bool isCleaning;
    public bool isFinished = false;
    private Vector2 lastMousePos;
    private float audioKeepAliveTimer = 0f;
    private const float AUDIO_GRACE_PERIOD = 0.15f;

    private IToolObject currentToolObject;
    private IInteractObject currentInteract;

    private bool isSfxPlaying = false;

    [Inject]
    public ToolService(ObjectDetectionService objectDetectionService,
    SurfaceDetectionService surfaceDetectionService,
    CleaningService cleaningService)
    {
        this.objectDetectionService = objectDetectionService;
        this.surfaceDetectionService = surfaceDetectionService;
        this.cleaningService = cleaningService;
    }

    public void Initialize()
    {
        objectDetectionService.OnInteractDetected += HandleObjectDetected;

        InteractionEvents.OnPressStart += HandlePressStart;
        InteractionEvents.OnPressEnd += HandlePressEnd;
        InteractionEvents.OnMouseMoved += HandleMouseMove;

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        objectDetectionService.OnInteractDetected -= HandleObjectDetected;

        InteractionEvents.OnPressStart -= HandlePressStart;
        InteractionEvents.OnPressEnd -= HandlePressEnd;
        InteractionEvents.OnMouseMoved -= HandleMouseMove;

        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
    }

    private void HandleObjectDetected(IInteractObject obj)
    {
        // if (obj is IToolObject toolObj) currentToolObject = toolObj;
        currentInteract = obj;
    }

    private void HandlePressStart()
    {
        if (isFinished && currentInteract == null) return;

        if (currentInteract is IToolObject tool)
        {
            if (currentToolObject != tool)
            {
                currentToolObject?.Return();
                currentToolObject = tool;
                currentToolObject?.Use();
            }
        }
        else if (currentInteract is IClean clean)
        {
            isCleaning = true;
            if (clean != null && clean.IsCleanable())
            {
                CleaningTarget();
            }
            else
            {
                currentToolObject?.Return();
                currentToolObject = null;
                isCleaning = false;
            }
        }
        else if (currentToolObject != null)
        {
            if (currentInteract == currentToolObject.GetOrigin())
            {
                currentToolObject?.Return();
                currentToolObject = null;
                isCleaning = false;
            }
        }
    }

    private void CleaningTarget()
    {
        if (surfaceDetectionService.HasHit)
        {
            if (currentToolObject is IBrushTool brush)
            {
                // Debug.Log("current tool is brush? " + (currentToolObject is IBrushTool));
                CleanSurface(brush);
            }
            else
            {
                // Debug.Log("current tool is brush? " + (currentToolObject is IBrushTool));
                CleanChunk();
            }
        }
    }

    private void HandlePressEnd()
    {
        if (isFinished) return;
        isCleaning = false;
    }

    private void HandleMouseMove(Vector2 mousePos)
    {
        if (currentToolObject == null) return;
        var worldPos = objectDetectionService.ScreenToWorld(mousePos, currentToolObject as IInteractObject);
        float mouseDelta = Vector2.Distance(mousePos, lastMousePos);
        lastMousePos = mousePos;

        if (surfaceDetectionService.DetectSurface(mousePos))
        {
            StickToSurfaces(mouseDelta);
            // Debug.Log("Tool is sticking to surface");
        }
        else
        {
            currentToolObject.FollowMouse(worldPos);
            // Debug.Log("Tool is NOT sticking to surface");
        }
    }


    private void PlayToolSfx(bool play)
    {
        if (currentTool == null) return;

        if (play && !isSfxPlaying)
        {
            currentTool.PlaySfx(true);
            isSfxPlaying = true;
        }
        else if (!play && isSfxPlaying)
        {
            currentTool.PlaySfx(false);
            isSfxPlaying = false;
        }
    }

    private void CleanSurface(IBrushTool brushTool)
    {
        var brush = brushTool.GetBrush;
        var brushScale = brushTool.BrushScale;
        var brushStrength = brushTool.BrushStrength;
        var brushColor = brushTool.BrushColor;
        var brushTransform = brushTool.BrushTransform.up;
        var surface = surfaceDetectionService.CleanableSurface;
        var hitPoint = surfaceDetectionService.RaycastPos;
        var hitNormal = surfaceDetectionService.RaycastNormal;
        if (surface != null)
        {
            Debug.Log("Cleaning surface with brush tool");
            cleaningService.CleanSurface(surface, brush, hitPoint, hitNormal, brushTransform, brushScale, brushStrength, brushColor);
        }
    }

    private void CleanChunk()
    {
        if (currentInteract == null) return;
        var chunk = surfaceDetectionService.CleanableChunk;
        if (chunk != null) cleaningService.CleanChunk(chunk);
    }

    public ITool GetCurrentTool()
    {
        return currentTool;
    }

    private void StickToSurfaces(float mouseDelta)
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;

        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);
        currentToolObject.StickToSurface(targetPos, targetRotation);

        if (isCleaning)
        {
            Debug.Log("Cleaning surface with brush tool");
            CleaningTarget();
        }
    }

    // public void Tick()
    // {
    //     var mousePos = inputSystemService.GetMousePosition();
    //     if (currentTool == null) return;

    //     if (audioKeepAliveTimer > 0f)
    //     {
    //         audioKeepAliveTimer -= Time.deltaTime;
    //     }

    //     if (currentTool is IInteractObject interact)
    //     {
    //         var worldPos = objectDetectionService.ScreenToWorld(mousePos, interact);

    //         float mouseDelta = Vector2.Distance(mousePos, lastMousePos);
    //         lastMousePos = mousePos;

    //         if (surfaceDetectionService.PerformRaycast(mousePos, currentTool.ToolType))
    //         {
    //             audioKeepAliveTimer = AUDIO_GRACE_PERIOD;
    //             StickToSurface(interact, mouseDelta);

    //             if (isCleaning && currentTool.ToolType == SurfaceDetectionType.Texture)
    //             {
    //                 PlayToolSfx(true);
    //             }
    //         }
    //         else
    //         {
    //             currentTool.FollowMouse(worldPos);

    //             if (audioKeepAliveTimer <= 0f && isCleaning && currentTool.ToolType == SurfaceDetectionType.Texture)
    //             {
    //                 PlayToolSfx(false);
    //             }
    //         }
    //     }
    // }

    private void HandleGameWrapped()
    {
        PlayToolSfx(false);
        currentTool?.Return();
    }

    public bool IsOnToolMode => GetCurrentTool() != null;
}