using System;
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
        currentInteract = obj;
    }

    private void HandlePressStart()
    {
        if (isFinished && currentInteract == null) return;

        // 1. When clicking a Tool to pick it up
        if (currentInteract is IToolObject tool)
        {
            if (currentToolObject != tool)
            {
                ReturnCurrentTool(); // Safely put away old tool if holding one

                currentToolObject = tool;
                currentTool = tool as ITool; // Sync interface references

                currentToolObject?.Use();

                CursorController.instance?.LockCursorState(CursorState.Crosshair);
            }
        }
        // 2. When clicking a cleanable surface/chunk
        else if (currentInteract is IClean clean)
        {
            isCleaning = true;
            if (clean != null && clean.IsCleanable())
            {
                CleaningTarget();
            }
            else
            {
                ReturnCurrentTool();
            }
        }
        else if (currentToolObject != null)
        {
            if (currentInteract == currentToolObject.GetOrigin() || currentInteract == null)
            {
                ReturnCurrentTool();
            }
        }
    }

    private void ReturnCurrentTool()
    {
        currentToolObject?.Return();
        currentToolObject = null;
        currentTool = null;
        isCleaning = false;

        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void CleaningTarget()
    {
        if (surfaceDetectionService.HasHit)
        {
            if (currentToolObject is IBrushTool brush)
            {
                CleanSurface(brush);
            }
            else
            {
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
        }
        else
        {
            currentToolObject.FollowMouse(worldPos);
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
            if (surfaceDetectionService.HasHit)
            {
                isCleaning = true;
                PlayToolSfx(true);
            }
            else
            {
                ReturnCurrentTool();
                return; // Exit early to prevent cleaning on nothing
            }

            cleaningService.CleanSurface(surface, brush, hitPoint, hitNormal, brushTransform, brushScale, brushStrength, brushColor);
        }
    }

    private void CleanChunk()
    {
        if (isFinished) return;

        PlayToolSfx(false);

        audioKeepAliveTimer = 0f;
        isCleaning = false;

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
            CleaningTarget();
        }
    }

    private void HandleGameWrapped()
    {
        PlayToolSfx(false);
        ReturnCurrentTool();
    }

    public bool IsOnToolMode => GetCurrentTool() != null;
}