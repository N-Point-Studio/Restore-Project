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

    private void HandleLeftPressStart(Vector2 currentPos) => OnPressStarted?.Invoke();
    private void HandleLeftPressEnd(Vector2 currentPos) => OnPressEnded?.Invoke();
    public Vector2 GetCurrentPos() => inputSystemService.GetMousePosition();
}