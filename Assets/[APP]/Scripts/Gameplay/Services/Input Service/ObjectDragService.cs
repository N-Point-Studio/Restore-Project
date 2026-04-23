using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ObjectDragService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly GameConfigData config;

    private Vector2 startPos;
    private Vector2 lastPos;

    private bool isPressing;
    private bool isDragging;

    public event Action<Vector2> OnDragStarted;
    public event Action<Vector2> OnDragEnded;
    public event Action<Vector2> OnDragPerformed;

    [Inject]
    public ObjectDragService(InputSystemService inputSystemService, GameConfigData config)
    {
        this.inputSystemService = inputSystemService;
        this.config = config;
    }

    public void Initialize()
    {
        inputSystemService.OnLeftPressStarted += HandlePressStarted;
        inputSystemService.OnMouseMoved += HandlePressMoved;
        inputSystemService.OnLeftPressEnded += HandlePressEnded;
    }

    public void Dispose()
    {
        inputSystemService.OnLeftPressStarted -= HandlePressStarted;
        inputSystemService.OnMouseMoved -= HandlePressMoved;
        inputSystemService.OnLeftPressEnded -= HandlePressEnded;
    }

    private void HandlePressStarted(Vector2 currentPos)
    {
        isPressing = true;
        isDragging = false;

        Vector2 pos = inputSystemService.GetMousePosition();

        startPos = pos;
        lastPos = pos;
    }

    private void HandlePressMoved(Vector2 currentPos)
    {
        if (!isPressing) return;

        if (!isDragging && Vector2.Distance(startPos, currentPos) > config.dragThreshold)
        {
            isDragging = true;
            OnDragStarted?.Invoke(currentPos);
        }

        if (!isDragging) return;

        Vector2 delta = currentPos - lastPos;
        lastPos = currentPos;

        OnDragPerformed?.Invoke(currentPos);
    }

    private void HandlePressEnded(Vector2 currentPos)
    {
        if (isDragging)
        {
            OnDragEnded?.Invoke(currentPos);
        }

        isPressing = false;
        isDragging = false;
    }
}