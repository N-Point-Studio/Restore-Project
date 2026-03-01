using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PressService : IInitializable, IDisposable
{
    private readonly InputService input;
    public event Action<Vector2> OnPressStarted;
    public event Action<Vector2> OnPressEnded;

    [Inject]
    public PressService(InputService input) { this.input = input; }

    public void Initialize()
    {
        input.OnPrimaryStarted += OnPressStart;
        input.OnPrimaryEnded += OnPressEnd;
    }

    public void Dispose()
    {
        input.OnPrimaryStarted -= OnPressStart;
        input.OnPrimaryEnded -= OnPressEnd;
    }

    private void OnPressStart(Vector2 vector) => OnPressStarted?.Invoke(vector);
    private void OnPressEnd(Vector2 vector) => OnPressEnded?.Invoke(vector);
    public Vector2 GetCurrentPos() => input.GetPrimaryPos();
}