using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ZoomService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private const float ScrollSensitivity = 10f;
    public event Action<float> OnZoomPerformed;

    [Inject]
    public ZoomService(InputSystemService inputSystemService)
    {
        this.inputSystemService = inputSystemService;
    }

    public void Initialize()
    {
        inputSystemService.OnScrollPerformed += HandleScrollPerformed;
    }


    public void Dispose()
    {
        inputSystemService.OnScrollPerformed -= HandleScrollPerformed;
    }

    private void HandleScrollPerformed(float value)
    {
        float zoomDelta = value * ScrollSensitivity;
        InteractionEvents.OnZoomPerformed(zoomDelta);
    }
}