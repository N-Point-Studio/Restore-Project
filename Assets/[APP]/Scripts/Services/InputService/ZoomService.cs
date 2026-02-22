using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ZoomService : IInitializable, IDisposable
{
    private readonly InputService input;
    private float lastPinchDistance;
    private bool isZooming;

    public event Action<float> OnZoomPerformed;

    [Inject]
    public ZoomService(InputService input)
    {
        this.input = input;
    }

    public void Initialize()
    {
        input.OnSecondaryStarted += OnSecondaryStarted;
        input.OnSecondaryMoved += OnSecondaryMoved;
        input.OnSecondaryEnded += OnSecondaryEnded;
    }

    public void Dispose()
    {
        input.OnSecondaryStarted -= OnSecondaryStarted;
        input.OnSecondaryMoved -= OnSecondaryMoved;
        input.OnSecondaryEnded -= OnSecondaryEnded;
    }

    private void OnSecondaryStarted(Vector2 pos)
    {
        isZooming = true;
        lastPinchDistance = Vector2.Distance(input.GetPrimaryPos(), pos);
    }

    private void OnSecondaryMoved(Vector2 pos)
    {
        if (!isZooming) return;

        float currentDistance = Vector2.Distance(input.GetPrimaryPos(), pos);
        float delta = currentDistance - lastPinchDistance;
        lastPinchDistance = currentDistance;
        OnZoomPerformed?.Invoke(delta);
    }

    private void OnSecondaryEnded(Vector2 pos)
    {
        isZooming = false;
    }
}