using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PressService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    public event Action OnPressStarted;
    public event Action OnPressEnded;

    [Inject]
    public PressService(InputSystemService inputSystemService) { this.inputSystemService = inputSystemService; }

    public void Initialize()
    {
        inputSystemService.OnLeftPressStarted += HandleLeftPressStart;
        inputSystemService.OnLeftPressEnded += HandleLeftPressEnd;
    }

    public void Dispose()
    {
        inputSystemService.OnLeftPressStarted += HandleLeftPressStart;
        inputSystemService.OnLeftPressEnded += HandleLeftPressEnd;
    }

    private void HandleLeftPressStart() => OnPressStarted?.Invoke();
    private void HandleLeftPressEnd() => OnPressEnded?.Invoke();
    public Vector2 GetCurrentPos() => inputSystemService.GetMousePosition();
}