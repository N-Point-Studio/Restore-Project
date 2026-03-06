using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class DragService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;

    private const float DragThreshold = 25f;

    private Vector2 startPos;
    private Vector2 lastPos;

    private bool isPressing;
    private bool isDragging;

    public event Action OnDragStarted;
    public event Action OnDragEnded;
    public event Action<Vector2> OnDragPerformed;

    [Inject]
    public DragService(InputSystemService inputSystemService)
    {
        this.inputSystemService = inputSystemService;
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

    private void HandlePressStarted()
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

        if (!isDragging && Vector2.Distance(startPos, currentPos) > DragThreshold)
        {
            isDragging = true;
            OnDragStarted?.Invoke();
        }

        if (!isDragging) return;

        Vector2 delta = currentPos - lastPos;
        lastPos = currentPos;

        OnDragPerformed?.Invoke(delta);
    }

    private void HandlePressEnded()
    {
        if (isDragging)
        {
            OnDragEnded?.Invoke();
        }

        isPressing = false;
        isDragging = false;
    }
}