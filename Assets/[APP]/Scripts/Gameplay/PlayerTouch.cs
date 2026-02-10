using System;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerTouch : MonoBehaviour
{
    private PlayerInputSystem playerInputSystem;
    private GameInput input => playerInputSystem.Input;

    [Inject]
    public void Construct(PlayerInputSystem playerInputSystem)
    {
        this.playerInputSystem = playerInputSystem;
    }

    private void Awake()
    {

    }

    void Start()
    {
        playerInputSystem.ChangeInputState(InputStateType.Player);

        input.Player.Press.performed += HandleOnPressPerformed;
        input.Player.ScreenPos.performed += HandleScreenPos;
        input.Player.Tap.performed += HandleOnTapPerformed;
        input.Player.Hold.performed += HandleOnHoldPerformed;
    }

    private void HandleOnHoldPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Press Hold");

    }

    private void HandleOnTapPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Tap Performed");
    }

    private void HandleScreenPos(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        Debug.Log("Screen Pos: " + pos);
    }

    private void HandleOnPressPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Press Performed");

    }

}