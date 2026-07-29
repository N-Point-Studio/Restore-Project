using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class MobileInputSystemService : IInitializable, IDisposable, ITickable
{
    private readonly PlayerInputSystem inputSystem;
    private GameInput Input => inputSystem.Input;

    [Inject]
    public MobileInputSystemService(PlayerInputSystem inputSystem)
    {
        this.inputSystem = inputSystem;
    }

    public void Initialize()
    {
        Input.Player.Press.started += HandlePressStarted;
        Input.Player.Press.canceled += HandlePressEnded;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= HandlePressStarted;
        Input.Player.Press.canceled -= HandlePressEnded;
    }

    public void Tick()
    {
    }

    private void HandlePressStarted(InputAction.CallbackContext context)
    {
        // Baca nilai absolut langsung dari hardware/device (bisa touch atau mouse)
        if (Pointer.current != null)
        {
            Vector2 startPosition = Pointer.current.position.ReadValue();
            Debug.Log("Press Started " + startPosition);
        }
    }

    private void HandlePressEnded(InputAction.CallbackContext context)
    {
        if (Pointer.current != null)
        {
            Vector2 endPosition = Pointer.current.position.ReadValue();
            Debug.Log("Press Ended " + endPosition);
        }
    }
}