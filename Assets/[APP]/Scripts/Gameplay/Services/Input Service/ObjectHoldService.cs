using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ObjectHoldService : IInitializable, IDisposable, ITickable
{
    private readonly InputSystemService inputSystemService;

    private bool isPressing;
    private bool isHolding;

    private float pressTime;

    private Vector2 pressPosition;
    private Vector2 currentMousePosition;

    private const float HOLD_DELAY = 0.2f;
    private const float HOLD_DURATION = .5f;
    private const float MOVE_TOLERANCE = 5f;

    public event Action<float, Vector2> OnHoldPerformed;
    public event Action<Vector2> OnHoldCompleted;
    public event Action<Vector2> OnHoldCanceled;

    [Inject]
    public ObjectHoldService(InputSystemService inputSystemService)
    {
        this.inputSystemService = inputSystemService;
    }

    public void Initialize()
    {
        inputSystemService.OnLeftPressStarted += HandlePressStarted;
        inputSystemService.OnMouseMoved += HandleMouseMoved;
        inputSystemService.OnLeftPressEnded += HandlePressEnded;
    }

    public void Dispose()
    {
        inputSystemService.OnLeftPressStarted -= HandlePressStarted;
        inputSystemService.OnMouseMoved -= HandleMouseMoved;
        inputSystemService.OnLeftPressEnded -= HandlePressEnded;
    }

    public void Tick()
    {
        if (!isPressing) return;

        if (Vector2.Distance(currentMousePosition, pressPosition) > MOVE_TOLERANCE)
        {
            CancelHold();
            return;
        }

        float heldTime = Time.time - pressTime;

        if (!isHolding && heldTime >= HOLD_DELAY)
        {
            isHolding = true;
        }

        if (!isHolding) return;

        float holdTime = heldTime - HOLD_DELAY;

        OnHoldPerformed?.Invoke(holdTime, inputSystemService.GetMousePosition());
        // Debug.Log("holding " + holdTime);

        if (holdTime >= HOLD_DURATION)
        {
            OnHoldCompleted?.Invoke(inputSystemService.GetMousePosition());
            CancelHold();
        }
    }

    private void HandlePressStarted(Vector2 currentPos)
    {
        isPressing = true;
        isHolding = false;

        pressTime = Time.time;

        pressPosition = inputSystemService.GetMousePosition();
        currentMousePosition = pressPosition;
    }

    private void HandleMouseMoved(Vector2 pos)
    {
        currentMousePosition = pos;
    }

    private void HandlePressEnded(Vector2 currentPos)
    {
        CancelHold();
    }

    private void CancelHold()
    {
        if (!isPressing) return;

        isPressing = false;
        isHolding = false;

        OnHoldCanceled?.Invoke(inputSystemService.GetMousePosition());
    }
}