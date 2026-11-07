using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class CameraService : IInitializable, ITickable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly Camera camera;

    private readonly float rotationSpeed = 50f;
    private readonly float boardPitch = 63f;

    private float initialPitch;
    private float currentPitch;
    private float defaultYRotation;
    private float defaultZRotation;

    private Vector2 currentMousePosition;

    [Inject]
    public CameraService(InputSystemService inputSystemService, Camera camera)
    {
        this.inputSystemService = inputSystemService;
        this.camera = camera;
    }

    public void Initialize()
    {
        InteractionEvents.OnMouseMoved += HandleMouseMoved;

        initialPitch = camera.transform.localEulerAngles.x;
        if (initialPitch > 180f) initialPitch -= 360f;

        currentPitch = initialPitch;
        defaultYRotation = camera.transform.localEulerAngles.y;
        defaultZRotation = camera.transform.localEulerAngles.z;
    }

    public void Dispose()
    {
        InteractionEvents.OnMouseMoved -= HandleMouseMoved;
    }

    private void HandleMouseMoved(Vector2 mousePos)
    {
        currentMousePosition = mousePos;
    }

    public void Tick()
    {
        if (currentMousePosition.y >= Screen.height - (Screen.height / 10))
        {
            currentPitch = Mathf.MoveTowards(currentPitch, boardPitch, rotationSpeed * Time.deltaTime);
        }
        else if (currentMousePosition.y <= Screen.height - (Screen.height / 2))
        {
            currentPitch = Mathf.MoveTowards(currentPitch, initialPitch, rotationSpeed * Time.deltaTime);
        }

        camera.transform.localRotation = Quaternion.Euler(currentPitch, defaultYRotation, defaultZRotation);
    }
}