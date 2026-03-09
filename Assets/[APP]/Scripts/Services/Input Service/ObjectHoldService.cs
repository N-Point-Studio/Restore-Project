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
    private const float HOLD_DURATION = 1.5f;
    private const float MOVE_TOLERANCE = 5f;

    public event Action<float> OnHoldPerformed;
    public event Action OnHoldCompleted;
    public event Action OnHoldCanceled;

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

        OnHoldPerformed?.Invoke(holdTime);
        // Debug.Log("holding " + holdTime);

        if (holdTime >= HOLD_DURATION)
        {
            OnHoldCompleted?.Invoke();
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

        OnHoldCanceled?.Invoke();
    }
}