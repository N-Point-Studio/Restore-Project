using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RotateService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private bool isRotating;
    private Vector2 lastMousePos;
    private const float RotateSensitivity = 0.2f;

    [Inject]
    public RotateService(InputSystemService inputSystemService)
    {
        this.inputSystemService = inputSystemService;
    }

    public void Initialize()
    {
        inputSystemService.OnRightPressStarted += HandleRightStarted;
        inputSystemService.OnRightPressEnded += HandleRightEnded;
        inputSystemService.OnMouseMoved += HandleMouseMove;
    }

    public void Dispose()
    {
        inputSystemService.OnRightPressStarted -= HandleRightStarted;
        inputSystemService.OnRightPressEnded -= HandleRightEnded;
        inputSystemService.OnMouseMoved -= HandleMouseMove;
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
        Vector2 rotateDelta = delta * RotateSensitivity;

        InteractionEvents.OnRotatePerformed?.Invoke(rotateDelta);
    }
}