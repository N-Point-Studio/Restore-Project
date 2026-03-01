using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HoldService : IInitializable, IDisposable, ITickable
{
    private readonly PressService pressService;
    private const float HoldDuration = 0.5f;
    private const float HoldThreshold = 20f;

    private Vector2 startPos;
    private float startTime;
    private bool isPressing;
    private bool holdTriggered;

    public event Action<Vector2> OnHoldPerformed;

    [Inject]
    public HoldService(PressService pressService)
    {
        this.pressService = pressService;
    }

    public void Initialize()
    {
        pressService.OnPressStarted += OnHoldStart;
        pressService.OnPressEnded += OnHoldEnd;
    }

    public void Dispose()
    {
        pressService.OnPressStarted -= OnHoldStart;
        pressService.OnPressEnded -= OnHoldEnd;
    }

    public void Tick()
    {
        if (!isPressing || holdTriggered) return;
        if (Vector2.Distance(startPos, pressService.GetCurrentPos()) > HoldThreshold)
        {
            isPressing = false;
            return;
        }

        if (Time.time - startTime >= HoldDuration)
        {
            holdTriggered = true;
            OnHoldPerformed?.Invoke(startPos);
        }
    }

    private void OnHoldStart(Vector2 pos)
    {
        startPos = pos;
        startTime = Time.time;
        isPressing = true;
        holdTriggered = false;
    }

    private void OnHoldEnd(Vector2 pos)
    {
        isPressing = false;
    }
}