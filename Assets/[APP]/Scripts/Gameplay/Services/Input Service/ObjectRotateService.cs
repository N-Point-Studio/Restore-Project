using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ObjectRotateService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly GameConfigData config;
    private bool isRotating;
    private Vector2 lastMousePos;

    [Inject]
    public ObjectRotateService(InputSystemService inputSystemService, GameConfigData config)
    {
        this.inputSystemService = inputSystemService;
        this.config = config;
    }

    public void Initialize()
    {
        inputSystemService.OnRightPressStarted += HandleRightStarted;
        inputSystemService.OnRightPressEnded += HandleRightEnded;
        InteractionEvents.OnMouseMoved += HandleMouseMove;
    }

    public void Dispose()
    {
        inputSystemService.OnRightPressStarted -= HandleRightStarted;
        inputSystemService.OnRightPressEnded -= HandleRightEnded;
        InteractionEvents.OnMouseMoved -= HandleMouseMove;
    }

    private void HandleRightStarted()
    {
        isRotating = true;
        lastMousePos = inputSystemService.GetMousePosition();
    }

    private void HandleRightEnded()
    {
        isRotating = false;
    }

    private void HandleMouseMove(Vector2 currentPos)
    {
        if (!isRotating) return;

        Vector2 delta = currentPos - lastMousePos;
        lastMousePos = currentPos;
        Vector2 rotateDelta = delta * config.rotateSensitivity;

        InteractionEvents.OnRotatePerformed?.Invoke(rotateDelta);
    }
}