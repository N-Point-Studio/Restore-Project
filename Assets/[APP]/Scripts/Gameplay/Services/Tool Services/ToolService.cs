using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ToolService : IInitializable, IDisposable, ITickable
{
    private readonly ObjectDetectionService objectDetectionService;
    private readonly SurfaceDetectionService surfaceDetectionService;
    private readonly CleaningService cleaningService;

    public bool isCleaning { get; private set; }
    public bool isFinished { get; set; } = false;
    public bool IsOnToolMode => currentToolObject != null;

    private IToolObject currentToolObject;
    private IInteractObject currentInteract;
    private Vector2 mousePos;
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

    public void Tick()
    {
        if (currentToolObject == null) return;

        if (surfaceDetectionService.DetectSurface(mousePos))
        {
            StickToSurfaces();
        }
        else
        {
            var worldPos = objectDetectionService.ScreenToWorld(mousePos, currentToolObject as IInteractObject);
            currentToolObject.FollowMouse(worldPos);
            PlayToolSfx(false);
            PlayToolVfx(false);
        }
    }

    private void HandleObjectDetected(IInteractObject obj)
    {
        currentInteract = obj;
    }

    private void HandlePressStart()
    {
        if (isFinished && currentInteract == null) return;

        switch (currentInteract)
        {
            case IToolObject tool:
                EquipTool(tool);
                break;

            case IClean clean when clean.IsCleanable():
                isCleaning = true;
                ProcessCleaning();
                break;

            case IClean clean when !clean.IsCleanable():
                isCleaning = false;
                ReturnCurrentTool();
                break;

            case var _ when IsOnToolMode && currentInteract == currentToolObject.GetOrigin():
                ReturnCurrentTool();
                break;
        }
    }

    private void HandleMouseMove(Vector2 newMousePos)
    {
        mousePos = newMousePos;

        if (isCleaning)
        {
            ProcessCleaning();
        }
    }

    private void HandlePressEnd()
    {
        if (isFinished) return;

        isCleaning = false;
        PlayToolSfx(false);
        PlayToolVfx(false);
    }

    private void EquipTool(IToolObject tool)
    {
        if (currentToolObject == tool) return;

        ReturnCurrentTool();

        currentToolObject = tool;
        currentToolObject.Use();

        CursorController.instance?.LockCursorState(CursorState.Crosshair);
    }

    private void ReturnCurrentTool()
    {
        if (currentToolObject == null) return;

        currentToolObject.Return();
        currentToolObject = null;

        isCleaning = false;
        PlayToolSfx(false);

        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void ProcessCleaning()
    {
        if (!surfaceDetectionService.HasHit) return;

        if (currentToolObject is IBrushTool brush)
        {
            CleanSurface(brush);
        }
        else
        {
            CleanChunk();
        }
        PlayToolVfx(true);
    }

    private void StickToSurfaces()
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;
        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);

        currentToolObject.StickToSurface(targetPos, targetRotation);
    }

    private void CleanSurface(IBrushTool brushTool)
    {
        var surface = surfaceDetectionService.CleanableSurface;

        if (surface == null) return;

        PlayToolSfx(true);
        cleaningService.CleanSurface(
            surface,
            brushTool.GetBrush,
            surfaceDetectionService.RaycastPos,
            surfaceDetectionService.RaycastNormal,
            brushTool.BrushTransform.up,
            brushTool.BrushScale,
            brushTool.BrushStrength,
            brushTool.BrushColor
        );
    }

    private void CleanChunk()
    {
        if (isFinished || currentInteract == null) return;

        var chunk = surfaceDetectionService.CleanableChunk;
        if (chunk == null) return;

        isCleaning = false;
        PlayToolSfx(true);
        if (currentToolObject is IClickTool tool) tool.PlayAnimation();
        cleaningService.CleanChunk(chunk);
    }

    private void PlayToolVfx(bool play)
    {
        if (currentToolObject == null) return;
        currentToolObject.PlayVfx(play);
    }

    private void PlayToolSfx(bool play)
    {
        if (currentToolObject == null) return;

        if (play && !isSfxPlaying)
        {
            currentToolObject.PlaySfx(true);
            isSfxPlaying = true;
        }
        else if (!play && isSfxPlaying)
        {
            currentToolObject.PlaySfx(false);
            isSfxPlaying = false;
        }
    }

    private void HandleGameWrapped()
    {
        PlayToolSfx(false);
        PlayToolVfx(false);
        ReturnCurrentTool();
    }
}