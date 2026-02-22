using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class ClickService : IInitializable, IDisposable
{
    private readonly InputService input;

    private const float ClickThreshold = 20f;
    private const float ClickDuration = 0.2f;

    private Vector2 startPos;
    private float startTime;

    public event Action<Vector2> OnClickPerformed;

    [Inject]
    public ClickService(InputService input)
    {
        this.input = input;
    }

    public void Initialize()
    {
        input.OnPrimaryStarted += OnPrimaryStarted;
        input.OnPrimaryEnded += OnPrimaryEnded;
    }

    public void Dispose()
    {
        input.OnPrimaryStarted -= OnPrimaryStarted;
        input.OnPrimaryEnded -= OnPrimaryEnded;
    }

    private void OnPrimaryStarted(Vector2 pos)
    {
        startPos = pos;
        startTime = Time.time;
    }

    private void OnPrimaryEnded(Vector2 endPos)
    {
        float duration = Time.time - startTime;
        float distance = Vector2.Distance(startPos, endPos);
        if (duration <= ClickDuration && distance <= ClickThreshold)
        {
            OnClickPerformed?.Invoke(endPos);
        }
    }
}