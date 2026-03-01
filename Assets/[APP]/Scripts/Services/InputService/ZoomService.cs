using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ZoomService : IInitializable, IDisposable
{
    private readonly InputService input;
    private float lastPinchDistance;
    private bool isZooming;

    private const float ScrollSensitivity = 10f;

    public event Action<float> OnZoomPerformed;

    [Inject]
    public ZoomService(InputService input)
    {
        this.input = input;
    }

    public void Initialize()
    {
        input.OnSecondaryFingerStarted += OnPinchStarted;
        input.OnSecondaryFingerMoved += OnPinchMoved;
        input.OnSecondaryFingerEnded += OnPinchEnded;

        input.OnScrollPerformed += OnScrollPerformed;
    }

    public void Dispose()
    {
        input.OnSecondaryFingerStarted -= OnPinchStarted;
        input.OnSecondaryFingerMoved -= OnPinchMoved;
        input.OnSecondaryFingerEnded -= OnPinchEnded;

        input.OnScrollPerformed -= OnScrollPerformed;
    }

    #region Mobile Pinch Logic
    private void OnPinchStarted(Vector2 secondaryPos)
    {
        isZooming = true;
        lastPinchDistance = Vector2.Distance(input.GetPrimaryPos(), secondaryPos);
    }

    private void OnPinchMoved(Vector2 secondaryPos)
    {
        if (!isZooming) return;

        float currentDistance = Vector2.Distance(input.GetPrimaryPos(), secondaryPos);
        float delta = currentDistance - lastPinchDistance;
        lastPinchDistance = currentDistance;
        OnZoomPerformed?.Invoke(delta);
    }

    private void OnPinchEnded(Vector2 secondaryPos)
    {
        isZooming = false;
    }
    #endregion

    #region PC Scroll Logic
    private void OnScrollPerformed(float scrollValue)
    {
        Debug.Log($"Scroll Value: {scrollValue}");
        float zoomDelta = scrollValue * ScrollSensitivity;

        OnZoomPerformed?.Invoke(zoomDelta);
    }
    #endregion
}