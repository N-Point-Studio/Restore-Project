using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ObjectRotateService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly GameConfigData config;
    private readonly ObjectDetectionService detectionService;
    private readonly ToolService toolService;
    private bool isRotating;
    private Vector2 lastMousePos;
    private IInteractObject currentInteract;

    [Inject]
    public ObjectRotateService(InputSystemService inputSystemService, GameConfigData config, ObjectDetectionService detectionService, ToolService toolService)
    {
        this.inputSystemService = inputSystemService;
        this.config = config;
        this.detectionService = detectionService;
        this.toolService = toolService;
    }

    public void Initialize()
    {
        inputSystemService.OnRightPressStarted += HandleRightStarted;
        inputSystemService.OnRightPressEnded += HandleRightEnded;
        
        inputSystemService.OnLeftPressStarted += HandleLeftStarted;
        inputSystemService.OnLeftPressEnded += HandleLeftEnded;
        
        InteractionEvents.OnMouseMoved += HandleMouseMove;
        detectionService.OnInteractDetected += HandleInteractDetected;
    }

    public void Dispose()
    {
        inputSystemService.OnRightPressStarted -= HandleRightStarted;
        inputSystemService.OnRightPressEnded -= HandleRightEnded;
        
        inputSystemService.OnLeftPressStarted -= HandleLeftStarted;
        inputSystemService.OnLeftPressEnded -= HandleLeftEnded;
        
        InteractionEvents.OnMouseMoved -= HandleMouseMove;
        detectionService.OnInteractDetected -= HandleInteractDetected;
    }

    private void HandleRightStarted()
    {
        // PC Right Click Rotation
        StartRotation(inputSystemService.GetMousePosition());
    }

    private void HandleRightEnded()
    {
        StopRotation();
    }

    private void HandleLeftStarted(Vector2 pos)
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
        // 1-Finger Mobile Rotation Logic

        // 1. Prevent rotation if the player is using a PC tool or holding a Mobile tool
        if (toolService.IsOnToolMode) return;
        if (currentInteract is IToolObject || currentInteract is IDraggableTool) return;

        // 2. Prevent rotation if the player touches an UNASSEMBLED piece (so they can drag it to the spot)
        if (currentInteract is IArtefactPart part && part.CurrentState != ArtefactPieceState.Assembled) return;

        // 3. If all checks pass (touching empty space or the assembled artefact), start rotating!
        StartRotation(pos);
#endif
    }

    private void HandleLeftEnded(Vector2 pos)
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
        StopRotation();
#endif
    }

    private void HandleMouseMove(Vector2 currentPos)
    {
        if (!isRotating) return;

        Vector2 delta = currentPos - lastMousePos;
        lastMousePos = currentPos;
        Vector2 rotateDelta = delta * config.rotateSensitivity;

        InteractionEvents.OnRotatePerformed?.Invoke(rotateDelta);
    }

    private void HandleInteractDetected(IInteractObject interact)
    {
        currentInteract = interact;
    }

    private void StartRotation(Vector2 pos)
    {
        isRotating = true;
        lastMousePos = pos;

        CursorController.instance?.SetOverrideCursor(CursorState.Rotate);
    }

    private void StopRotation()
    {
        if (isRotating)
        {
            isRotating = false;
            CursorController.instance?.ClearOverrideCursor();
        }
    }
}