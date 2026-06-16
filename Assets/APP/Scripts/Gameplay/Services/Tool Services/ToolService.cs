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
    private readonly InputSystemService inputSystemService;

    public bool isCleaning { get; private set; }
    public bool isFinished { get; set; } = false;
    public bool IsOnToolMode => currentToolObject != null;

    private IToolObject currentToolObject;
    private IDraggableTool draggableTool;
    private IInteractObject currentInteract;
    private Vector2 mousePos;

    private bool isSfxPlaying = false;
    private bool isVfxPlaying = false;

    private float movementTimer = 0f;
    private const float MOVEMENT_TIMEOUT = 0.15f;

    [Inject]
    public ToolService(ObjectDetectionService objectDetectionService,
        SurfaceDetectionService surfaceDetectionService,
        CleaningService cleaningService,
        TutorialService tutorialService,
        GameplayManager gameplayManager,
        InputSystemService inputSystemService)
    {
        this.objectDetectionService = objectDetectionService;
        this.surfaceDetectionService = surfaceDetectionService;
        this.cleaningService = cleaningService;
        this.tutorialService = tutorialService;
        this.gameplayManager = gameplayManager;
        this.inputSystemService = inputSystemService;
    }

    public void Initialize()
    {
        objectDetectionService.OnInteractDetected += HandleObjectDetected;

        InteractionEvents.OnPressStart += HandlePressStart;
        InteractionEvents.OnPressEnd += HandlePressEnd;
        InteractionEvents.OnMouseMoved += HandleMouseMove;

        InteractionEvents.OnDragStarted += HandleDragStarted;
        InteractionEvents.OnDragPerformed += HandleDragPerformed;
        InteractionEvents.OnDragEnded += HandleDragEnded;

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        objectDetectionService.OnInteractDetected -= HandleObjectDetected;

        InteractionEvents.OnPressStart -= HandlePressStart;
        InteractionEvents.OnPressEnd -= HandlePressEnd;
        InteractionEvents.OnMouseMoved -= HandleMouseMove;

        InteractionEvents.OnDragStarted -= HandleDragStarted;
        InteractionEvents.OnDragPerformed -= HandleDragPerformed;
        InteractionEvents.OnDragEnded -= HandleDragEnded;

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
            var worldPos = objectDetectionService.ScreenToWorld(mousePos, currentToolObject as IInteractObject);
            currentToolObject.FollowMouse(worldPos);

            // Kursor keluar dari permukaan artefak -> Matikan suara
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
#if UNITY_STANDALONE
        if (isFinished && currentInteract == null) return;

        switch (currentInteract)
        {
            case IToolObject tool:
                EquipTool(tool);
                break;
            case IClean clean when clean.IsCleanable():
                isCleaning = true;
                movementTimer = MOVEMENT_TIMEOUT;
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
#endif
    }

    private void HandleMouseMove(Vector2 newMousePos)
    {
#if UNITY_STANDALONE
        float delta = Vector2.Distance(mousePos, newMousePos);
        mousePos = newMousePos;

        if (isCleaning && delta > 1.0f)
        {
            movementTimer = MOVEMENT_TIMEOUT;
            ProcessCleaning();
        }
#endif
    }

    private void HandlePressEnd()
    {
#if UNITY_STANDALONE
        if (isFinished) return;
        isCleaning = false;
        PlayToolSfx(false);
        PlayToolVfx(false);
#endif
    }

    private void HandleDragStarted(IInteractObject interact, Vector3 vector)
    {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
        if (interact is IDraggableTool tool) draggableTool = tool;
        if (draggableTool != null && draggableTool is IDragObject drag) drag.OnDragStarted(vector);
#endif
    }

    private void HandleDragPerformed(IInteractObject interact, Vector3 vector)
    {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
        if (interact is not IDraggableTool) { return; }
        if (draggableTool != null && draggableTool is IDragObject tool) tool.OnDragPerformed(vector);
        PerformRaycastTouch(inputSystemService.GetMousePosition());
#endif

    }

    private void HandleDragEnded(IInteractObject interact, Vector3 vector)
    {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
        if (interact is not IDraggableTool) { return; }
        if (draggableTool != null && draggableTool is IDragObject tool) tool.OnDragEnded(vector);
        draggableTool = null;
#endif
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

        currentToolObject.Return();
        currentToolObject = null;

        isCleaning = false;
        PlayToolSfx(false);
        PlayToolVfx(false);

        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    void PerformRaycastTouch(Vector2 screenPos)
    {
        if (draggableTool == null) return;
        var tipPoint = new Vector2(screenPos.x, screenPos.y + 350);

        if (surfaceDetectionService.DetectSurface(tipPoint))
        {
            StickToSurfaces();
        }
    }

    private void StickToSurfaces()
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;
        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);
#if UNITY_STANDALONE
        currentToolObject.StickToSurface(targetPos, targetRotation);
#elif UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
        draggableTool?.StickToSurface(targetPos, targetRotation);
        ProcessCleaning();
#endif
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

    private void CleanSurface(IDraggableBrushTool brushTool)
    {
        var surface = surfaceDetectionService.CleanableSurface;

        if (surface == null) return;

        PlayToolSfx(true);
        cleaningService.CleanSurface(
            surface,
            brushTool.GetBrush(),
            surfaceDetectionService.RaycastPos,
            surfaceDetectionService.RaycastNormal,
            brushTool.GetBrushTransform().up,
            brushTool.GetBrushScale(),
            brushTool.GetBrushStrength(),
            brushTool.GetBrushColor(),
            brushTool.GetBrushDepth()
        );

        // if (cleaningService.IsDustRemoved(surface))
        // {
        //     PlayToolVfx(true);
        // }
        // else
        // {
        //     PlayToolVfx(false);
        // }
    }

    private void CleanChunk()
    {
        if (isFinished || currentInteract == null) return;
        Debug.Log("Attempting to clean chunk...");

        var chunk = surfaceDetectionService.CleanableChunk;
        if (chunk == null) return;

        isCleaning = false;
        PlayToolSfx(true);
        PlayToolVfx(true);

        if (currentToolObject is IChiselTool tool) tool?.PlayGouge();
        if (draggableTool is IDraggableChiselTool draggableChisel) draggableChisel?.PlayGouge();

        cleaningService.CleanChunk(chunk);
    }

    private void ProcessCleaning()
    {
        if (!surfaceDetectionService.HasHit) return;

        if (currentToolObject is IBrushTool brush)
        {
            CleanSurface(brush);
        }
        else if (draggableTool is IDraggableBrushTool draggableBrush)
        {
            CleanSurface(draggableBrush);
        }
        else
        {
            CleanChunk();
        }
    }

    private void PlayToolVfx(bool play)
    {
        if (currentToolObject == null) return;

        if (play && !isVfxPlaying)
        {
            currentToolObject?.PlayVfx(true);
            draggableTool?.PlayVfx(true);
            isVfxPlaying = true;
        }
        else if (!play && isVfxPlaying)
        {
            currentToolObject?.PlayVfx(false);
            draggableTool?.PlayVfx(false);
            isVfxPlaying = false;
        }
    }

    private void PlayToolSfx(bool play)
    {
        if (currentToolObject == null) return;

        if (play && !isSfxPlaying)
        {
            currentToolObject.PlaySfx(true);
            draggableTool?.PlaySfx(true);
            isSfxPlaying = true;
        }
        else if (!play && isSfxPlaying)
        {
            currentToolObject?.PlaySfx(false);
            draggableTool?.PlaySfx(false);
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