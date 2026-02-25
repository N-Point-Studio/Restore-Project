using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ClickService : IInitializable, IDisposable
{
    private readonly PressService pressService;
    private const float ClickThreshold = 20f;
    private const float ClickDuration = 0.2f;
    private Vector2 startPos;
    private float startTime;
    public event Action<Vector2> OnClickPerformed;

    [Inject]
    public ClickService(PressService pressService)
    {
        this.pressService = pressService;
    }

    public void Initialize()
    {
        pressService.OnPressStarted += OnPressStarted;
        pressService.OnPressEnded += OnPressEnded;
    }

    public void Dispose()
    {
        pressService.OnPressStarted -= OnPressStarted;
        pressService.OnPressEnded -= OnPressEnded;
    }

    private void OnPressStarted(Vector2 pos)
    {
        startPos = pos;
        startTime = Time.time;
    }

    private void OnPressEnded(Vector2 endPos)
    {
        float duration = Time.time - startTime;
        float distance = Vector2.Distance(startPos, endPos);
        if (duration <= ClickDuration && distance <= ClickThreshold)
        {
            OnClickPerformed?.Invoke(endPos);
        }
    }
}