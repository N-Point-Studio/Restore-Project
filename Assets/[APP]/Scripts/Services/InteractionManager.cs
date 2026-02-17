using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class InteractionManager : IInitializable, IDisposable, ITickable
{
    private readonly PlayerInputSystem inputSystem;
    private readonly PointerService pointer;
    private readonly IInteractionEvent eventBus;
    private GameInput Input => inputSystem.Input;
    private IInteractable currentTarget;
    private Vector2 startScreenPos;
    private const float ClickThreshold = 10f;
    private float initialPinchDist;
    private float currentDepth;
    private Vector3 offset;

    private bool isMoving = false;

    [Inject]
    public InteractionManager(PlayerInputSystem inputSystem, PointerService pointer, IInteractionEvent eventBus)
    {
        this.inputSystem = inputSystem;
        this.pointer = pointer;
        this.eventBus = eventBus;

    }

    public void Initialize()
    {
        inputSystem.ChangeInputState(InputStateType.Player);
        Input.Player.Press.started += OnPressStarted;
        Input.Player.ScreenPos.performed += OnPressMoved;
        Input.Player.Press.canceled += OnPressEnded;
        Input.Player.Hold.performed += OnHoldPerformed;
        Input.Player.Hold.canceled += OnHoldCanceled;
        Input.Player.SecondaryFingerPress.started += OnSecondaryStarted;
        Input.Player.SecondaryFingerPos.performed += OnSecondaryMoved;
    }

    private void OnPressStarted(InputAction.CallbackContext ctx)
    {
        startScreenPos = GetScreenPos();
        if (pointer.TryRaycast(startScreenPos, out var hit))
        {
            if (hit.collider.TryGetComponent(out currentTarget))
            {
                currentDepth = pointer.GetDepth(hit.transform.position);
                Vector3 worldPos = pointer.ScreenToWorld(startScreenPos, currentDepth);
                offset = hit.transform.position - worldPos;

                currentTarget.OnStart(hit.transform.position, startScreenPos);
            }
        }
    }

    private void OnPressMoved(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;
        isMoving = true;

        Vector2 screenPos = GetScreenPos();
        Vector3 worldPos = pointer.ScreenToWorld(screenPos, currentDepth) + offset;

        currentTarget.OnMove(worldPos, screenPos);
    }

    private void OnPressEnded(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;

        Vector2 screenPos = GetScreenPos();
        Vector3 worldPos = pointer.ScreenToWorld(screenPos, currentDepth) + offset;

        bool isClick = Vector2.Distance(screenPos, startScreenPos) < ClickThreshold;

        currentTarget.OnEnd(worldPos, screenPos);

        eventBus.PublishObjectDropped(currentTarget, worldPos, isClick);
        isMoving = false;
        currentTarget = null;
    }

    private void OnHoldPerformed(InputAction.CallbackContext ctx)
    {
        if (isMoving) return;
        Debug.Log("Berhasil");
        currentTarget?.OnHold();
        currentTarget = null;
    }

    private void OnHoldCanceled(InputAction.CallbackContext ctx)
    {
    }

    private void OnSecondaryStarted(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;
        Vector2 p1 = GetScreenPos();
        Vector2 p2 = Input.Player.SecondaryFingerPos.ReadValue<Vector2>();
        initialPinchDist = Vector2.Distance(p1, p2);
    }

    private void OnSecondaryMoved(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;
        Vector2 p1 = GetScreenPos();
        Vector2 p2 = Input.Player.SecondaryFingerPos.ReadValue<Vector2>();

        float currentDist = Vector2.Distance(p1, p2);
        float delta = currentDist - initialPinchDist;

        currentTarget.OnZoom(delta * 0.01f);
        initialPinchDist = currentDist;
    }

    public void Dispose()
    {
        Input.Player.Press.started -= OnPressStarted;
        Input.Player.ScreenPos.performed -= OnPressMoved;
        Input.Player.Press.canceled -= OnPressEnded;
        Input.Player.Hold.performed -= OnHoldPerformed;
        Input.Player.SecondaryFingerPress.started -= OnSecondaryStarted;
        Input.Player.SecondaryFingerPos.performed -= OnSecondaryMoved;
        Input.Player.Hold.canceled += OnHoldCanceled;
    }

    private Vector2 GetScreenPos() => Input.Player.ScreenPos.ReadValue<Vector2>();

    public void Tick()
    {
        Debug.Log($"interactable {currentTarget}");
    }
}