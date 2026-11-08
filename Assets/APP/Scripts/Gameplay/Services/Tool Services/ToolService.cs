using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ToolService : IInitializable, IDisposable, ITickable
{
    private readonly ObjectDetectionService objectDetectionService;
    private readonly SurfaceDetectionService surfaceDetectionService;
    private readonly CleaningService cleaningService;
    private readonly TutorialService tutorialService;
    private readonly GameplayManager gameplayManager;
    private readonly ObjectInteractionManager objectInteractionManager;
    private readonly GameplayToolUIManager gameplayToolUIManager;

    public bool isCleaning { get; private set; }
    public bool isFinished { get; set; } = false;
    public bool IsOnToolMode => currentToolObject != null;

    private IToolObject currentToolObject;
    private IInteractObject currentInteract;
    private Vector2 mousePos;

    private bool isSfxPlaying = false;
    private bool isVfxPlaying = false;

    private float movementTimer = 0f;
    private const float MOVEMENT_TIMEOUT = 0.15f;

    [Inject]
    public ToolService(
        ObjectDetectionService objectDetectionService,
        SurfaceDetectionService surfaceDetectionService,
        CleaningService cleaningService,
        TutorialService tutorialService,
        GameplayManager gameplayManager,
        ObjectInteractionManager objectInteractionManager,
        GameplayToolUIManager gameplayToolUIManager)
    {
        this.objectDetectionService = objectDetectionService;
        this.surfaceDetectionService = surfaceDetectionService;
        this.cleaningService = cleaningService;
        this.tutorialService = tutorialService;
        this.gameplayManager = gameplayManager;
        this.objectInteractionManager = objectInteractionManager;
        this.gameplayToolUIManager = gameplayToolUIManager;
    }

    public void Initialize()
    {
        gameplayToolUIManager.OnActiveToolChanged += HandleActiveToolChanged;
        objectDetectionService.OnInteractDetected += HandleObjectDetected;

        InteractionEvents.OnPressStart += HandlePressStart;
        InteractionEvents.OnPressEnd += HandlePressEnd;
        InteractionEvents.OnMouseMoved += HandleMouseMove;

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        gameplayToolUIManager.OnActiveToolChanged -= HandleActiveToolChanged;
        objectDetectionService.OnInteractDetected -= HandleObjectDetected;

        InteractionEvents.OnPressStart -= HandlePressStart;
        InteractionEvents.OnPressEnd -= HandlePressEnd;
        InteractionEvents.OnMouseMoved -= HandleMouseMove;

        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
    }

    public void Tick()
    {
        if (currentToolObject == null) return;

        if (isCleaning)
        {
            if (movementTimer > 0f)
            {
                movementTimer -= Time.deltaTime;
                if (movementTimer <= 0f)
                {
                    PlayToolSfx(false);
                    PlayToolVfx(false);
                }
            }
        }

        if (surfaceDetectionService.DetectSurface(mousePos))
        {
            StickToSurfaces();
        }
        else
        {
            Vector3 worldPos = objectDetectionService.GetInspectionPlaneWorldPosition(mousePos);
            currentToolObject.FollowMouse(worldPos);
            PlayToolSfx(false);
            PlayToolVfx(false);
        }
    }

    private void HandleActiveToolChanged(ToolType type)
    {
        if (type == ToolType.None)
        {
            ReturnCurrentTool();
            gameplayToolUIManager.ReleaseAllTool();
            return;
        }

        GameObject toolObj = gameplayToolUIManager.GetToolObject(type);

        if (toolObj != null && toolObj.TryGetComponent(out IToolObject tool))
        {
            if (gameplayManager.isTutorialAvailable && !tutorialService.CanUseTool(tool.ToolId)) return;
            EquipTool(tool);
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
            case IClean clean when clean.IsCleanable():
                isCleaning = true;
                movementTimer = MOVEMENT_TIMEOUT;
                ProcessCleaning();
                break;
            case IClean clean when !clean.IsCleanable():
                isCleaning = false;
                gameplayToolUIManager.ReleaseAllTool();
                ReturnCurrentTool();
                break;
        }
    }

    private void HandleMouseMove(Vector2 newMousePos)
    {
        float delta = Vector2.Distance(mousePos, newMousePos);
        mousePos = newMousePos;

        if (isCleaning && delta > 1.0f)
        {
            movementTimer = MOVEMENT_TIMEOUT;
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

        if (gameplayManager.isTutorialAvailable)
        {
            tutorialService.TriggerHighlight(false, tool.ToolId);
        }

        CursorController.instance?.LockCursorState(CursorState.Crosshair);
    }

    private void ReturnCurrentTool()
    {
        if (currentToolObject == null) return;
        Debug.Log("cek return tool: " + currentToolObject.ToolId);

        isCleaning = false;
        PlayToolSfx(false);
        PlayToolVfx(false);

        currentToolObject.Return();
        currentToolObject = null;

        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
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
            brushTool.BrushColor,
            brushTool.BrushDepth
        );

        if (cleaningService.IsDustRemoved(surface))
        {
            PlayToolVfx(true);
        }
        else
        {
            PlayToolVfx(false);
        }
    }

    private void CleanChunk()
    {
        if (isFinished || currentInteract == null) return;
        var chunk = surfaceDetectionService.CleanableChunk;
        if (chunk == null) return;

        isCleaning = false;
        PlayToolSfx(true);

        if (currentToolObject is IChiselTool tool) tool?.PlayGouge();

        cleaningService.CleanChunk(chunk);
        PlayToolSfx(false);
    }

    private void ProcessCleaning()
    {
        if (!surfaceDetectionService.HasHit) return;

        if (currentToolObject != null && currentToolObject is IBrushTool brush)
        {
            CleanSurface(brush);
        }
        else
        {
            CleanChunk();
        }
    }

    private void PlayToolVfx(bool play)
    {
        if (currentToolObject != null)
        {
            if (play && !isVfxPlaying)
            {
                currentToolObject.PlayVfx(true);
                isVfxPlaying = true;
            }
            else if (!play && isVfxPlaying)
            {
                currentToolObject.PlayVfx(false);
                isVfxPlaying = false;
            }
        }
    }

    private void PlayToolSfx(bool play)
    {
        if (currentToolObject != null)
        {
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
    }

    public void ForceReturnTool()
    {
        objectInteractionManager.ForceDropCurrentObject();
        ReturnCurrentTool();
    }

    private void HandleGameWrapped()
    {
        PlayToolSfx(false);
        PlayToolVfx(false);
        ForceReturnTool();
    }
}