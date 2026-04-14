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
    private Vector2 lastMousePos;
    private float audioKeepAliveTimer = 0f;
    private const float AUDIO_GRACE_PERIOD = 0.15f;

    private bool isSfxPlaying = false;

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

    private void HandlePressStarted(IInteractObject interact)
    {
        if (isFinished) return;
        if (interact is not ITool)
        {
            if (surfaceDetectionService.HasHit)
            {
                isCleaning = true;
                var hardObject = surfaceDetectionService.HardObject;
                var surfaceObject = surfaceDetectionService.CleanObject;
                if (hardObject != null)
                {
                    if (currentTool.ToolType == SurfaceDetectionType.Mesh)
                    {
                        cleaningService.TryCleaningHardSurface(hardObject);
                        PlayToolSfx(true);
                    }
                }
                else if (surfaceObject != null)
                {
                    if (currentTool.ToolType == SurfaceDetectionType.Texture)
                    {
                        PlayToolSfx(true);
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

        // Matikan suara via helper
        PlayToolSfx(false);

        audioKeepAliveTimer = 0f;
        cleaningService.EndClean();
        isCleaning = false;
    }

    public ITool GetCurrentTool()
    {
        return currentTool;
    }

    private void StickToSurface(IInteractObject interact, float mouseDelta)
    {
        Vector3 targetPos = surfaceDetectionService.RaycastPos;
        Vector3 targetNormal = surfaceDetectionService.RaycastNormal;

        Quaternion targetRotation = Quaternion.LookRotation(-targetNormal, Vector3.up);
        currentTool.StickToSurface(targetPos, targetRotation);

        if (isCleaning && surfaceDetectionService.CleanObject != null)
        {
            ICleanSurfaceObject cleanObject = surfaceDetectionService.CleanObject;
            if (cleanObject != null)
            {
                Vector2 texture = surfaceDetectionService.TextureSurface;
                Texture2D brush = currentTool.GetBrush();

                if (currentTool.ToolType == SurfaceDetectionType.Texture)
                {
                    cleaningService.TryCleaning(cleanObject, texture, brush);

                    if (mouseDelta > 0.1f)
                    {
                        currentTool.Moving(true);
                    }
                    else
                    {
                        currentTool.Moving(false);
                    }
                }
            }
        }
    }

    public void Tick()
    {
        var mousePos = inputSystemService.GetMousePosition();
        if (currentTool == null) return;

        if (audioKeepAliveTimer > 0f)
        {
            audioKeepAliveTimer -= Time.deltaTime;
        }

        if (currentTool is IInteractObject interact)
        {
            var worldPos = objectDetectionService.ScreenToWorld(mousePos, interact);

            float mouseDelta = Vector2.Distance(mousePos, lastMousePos);
            lastMousePos = mousePos;

            if (surfaceDetectionService.PerformRaycast(mousePos, currentTool.ToolType))
            {
                audioKeepAliveTimer = AUDIO_GRACE_PERIOD;
                StickToSurface(interact, mouseDelta);
                
                if (isCleaning)
                {
                    PlayToolSfx(true);
                }
            }
            else
            {
                currentTool.FollowMouse(worldPos);
                
                if (audioKeepAliveTimer <= 0f)
                {
                    PlayToolSfx(false);
                }
            }
        }
    }

    private void HandleGameWrapped()
    {
        PlayToolSfx(false);
        currentTool?.Return();
    }

    public bool IsOnToolMode => GetCurrentTool() != null;
}