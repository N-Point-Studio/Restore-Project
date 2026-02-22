using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class HoldService : IInitializable, IDisposable, ITickable
{
    private readonly InputService input;
    private const float HoldDuration = 0.5f;
    private const float HoldThreshold = 20f;

    private Vector2 startPos;
    private float startTime;
    private bool isPressing;
    private bool holdTriggered;

    public event Action<Vector2> OnHoldPerformed;

    [Inject]
    public HoldService(InputService input)
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

    public void Tick()
    {
        if (isPressing && !holdTriggered)
        {
            if (Vector2.Distance(startPos, input.GetPrimaryPos()) > HoldThreshold)
            {
                isPressing = false;
                return;
            }

            if (Time.time - startTime >= HoldDuration)
            {
                holdTriggered = true;
                OnHoldPerformed?.Invoke(startPos);
                Debug.Log("Hold Detected");
            }
        }
    }

    private void OnPrimaryStarted(Vector2 pos)
    {
        startPos = pos;
        startTime = Time.time;
        isPressing = true;
        holdTriggered = false;
    }

    private void OnPrimaryEnded(Vector2 pos)
    {
        isPressing = false;
    }
}