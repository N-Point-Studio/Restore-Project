using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class GestureService : IInitializable, IDisposable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;

    public event Action<Vector2> OnPrimaryStarted;
    public event Action<Vector2> OnPrimaryMoved;
    public event Action<Vector2> OnPrimaryEnded;
    public event Action<Vector2> OnSecondaryStarted;
    public event Action<Vector2> OnSecondaryMoved;
    public event Action<Vector2> OnSecondaryEnded;

    [Inject]
    public GestureService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);
        Input.Player.Press.started += OnPressStarted;
        Input.Player.Press.canceled += OnPressEnded;
        Input.Player.ScreenPos.performed += OnPressMoved;
        Input.Player.SecondaryFingerPress.started += OnSecondaryPressStarted;
        Input.Player.SecondaryFingerPress.canceled += OnSecondaryPressEnded;
        Input.Player.SecondaryFingerPos.performed += OnSecondaryPressMoved;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= OnPressStarted;
        Input.Player.Press.canceled -= OnPressEnded;
        Input.Player.ScreenPos.performed -= OnPressMoved;
        Input.Player.SecondaryFingerPress.started -= OnSecondaryPressStarted;
        Input.Player.SecondaryFingerPress.canceled -= OnSecondaryPressEnded;
        Input.Player.SecondaryFingerPos.performed -= OnSecondaryPressMoved;
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        OnPrimaryStarted?.Invoke(GetPrimaryPos());
    }

    private void OnPressMoved(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        OnPrimaryMoved?.Invoke(GetPrimaryPos());
    }

    private void OnPressEnded(InputAction.CallbackContext context)
    {
        OnPrimaryEnded?.Invoke(GetPrimaryPos());
    }

    private void OnSecondaryPressStarted(InputAction.CallbackContext context)
    {
        OnSecondaryStarted?.Invoke(GetSecondaryPos());
    }

    private void OnSecondaryPressMoved(InputAction.CallbackContext context)
    {
        OnSecondaryMoved?.Invoke(GetSecondaryPos());
    }

    private void OnSecondaryPressEnded(InputAction.CallbackContext context)
    {
        OnSecondaryEnded?.Invoke(GetSecondaryPos());
    }
    public Vector2 GetPrimaryPos() { return Input.Player.ScreenPos.ReadValue<Vector2>(); }
    public Vector2 GetSecondaryPos() { return Input.Player.SecondaryFingerPos.ReadValue<Vector2>(); }

}