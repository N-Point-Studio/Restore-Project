using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class SwipeService : IInitializable, IDisposable
{
    private readonly InputService input;
    private const float DragThreshold = 25f;
    private Vector2 startPos;
    private Vector2 lastPos;
    private bool isSwiping;

    public event Action OnInteractionStart;
    public event Action OnInteractionEnd;
    public event Action<Vector3> OnDragPerformed;
    public event Action<Vector2> OnRotatePerformed;

    [Inject]
    public SwipeService(InputService input)
    {
        this.input = input;
    }

    public void Initialize()
    {
        input.OnPrimaryStarted += OnPrimaryStarted;
        input.OnPrimaryMoved += OnPrimaryMoved;
        input.OnPrimaryEnded += OnPrimaryEnded;
    }

    public void Dispose()
    {
        input.OnPrimaryStarted -= OnPrimaryStarted;
        input.OnPrimaryMoved -= OnPrimaryMoved;
        input.OnPrimaryEnded -= OnPrimaryEnded;
    }

    private void OnPrimaryStarted(Vector2 pos)
    {
        startPos = pos;
        lastPos = pos;
        isSwiping = false;
        OnInteractionStart?.Invoke();
    }

    private void OnPrimaryMoved(Vector2 currentPos)
    {
        if (!isSwiping && Vector2.Distance(startPos, currentPos) > DragThreshold)
        {
            isSwiping = true;
        }

        if (isSwiping)
        {
            Vector2 delta = currentPos - lastPos;
            lastPos = currentPos;

            OnRotatePerformed?.Invoke(delta);
            OnDragPerformed?.Invoke(new Vector3(currentPos.x, currentPos.y, 0));
        }
    }

    private void OnPrimaryEnded(Vector2 pos)
    {
        if (isSwiping)
        {
            OnInteractionEnd?.Invoke();
        }
        isSwiping = false;
    }
}