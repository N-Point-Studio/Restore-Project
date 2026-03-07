using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public interface IGestureState
{
    void Enter();
    void Exit();
}

public class GestureManager : IInitializable, IDisposable
{
    private enum GestureState { Idle, InteractingWithObject }
    private GestureState currentState = GestureState.Idle;
    private readonly PointService point;
    private readonly PressService pressService;
    private readonly DragService dragService;
    private readonly HoldService holdService;
    private IInteractObject currentInteract;

    [Inject]
    public GestureManager(PointService point, InputService input, PressService press,
        DragService swipe, HoldService hold, ZoomService zoom)
    {
        this.point = point;

        pressService = press;
        dragService = swipe;
        holdService = hold;
    }

    public void Initialize()
    {
        point.OnInteractDetected += HandleInteractDetected;

        pressService.OnPressStarted += HandlePressStarted;
        pressService.OnPressEnded += HandlePressEnded;

        dragService.OnDragStarted += HandleDragStart;
        dragService.OnDragPerformed += HandleDragPerformed;
        dragService.OnDragEnded += HandleDragEnded;

        holdService.OnHoldPerformed += HandleHoldPerformed;
        holdService.OnHoldCompleted += HandleHoldCompleted;
        holdService.OnHoldCanceled += HandleHoldCanceled;
    }

    public void Dispose()
    {
        point.OnInteractDetected -= HandleInteractDetected;

        pressService.OnPressStarted -= HandlePressStarted;
        pressService.OnPressEnded -= HandlePressEnded;

        dragService.OnDragStarted -= HandleDragStart;
        dragService.OnDragPerformed -= HandleDragPerformed;
        dragService.OnDragEnded -= HandleDragEnded;

        holdService.OnHoldPerformed -= HandleHoldPerformed;
        holdService.OnHoldCompleted -= HandleHoldCompleted;
        holdService.OnHoldCanceled -= HandleHoldCanceled;
    }

    private void HandleInteractDetected(IInteractObject interact)
    {
        if (currentState == GestureState.InteractingWithObject) return;
        if (currentInteract == interact) return;
        currentInteract = interact;
    }

    public IInteractObject GetCurrentInteract()
    {
        return currentInteract;
    }

    private void HandlePressStarted()
    {
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnPressStarted?.Invoke(currentInteract);
    }

    private void HandlePressEnded()
    {
        if (currentInteract == null) return;
        currentState = GestureState.Idle;
        InteractionEvents.OnPressEnded?.Invoke(currentInteract);
    }

    private void HandleDragStart(Vector2 vector)
    {
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        Vector3 worldPos = point.ScreenToWorld(vector, currentInteract);
        InteractionEvents.OnDragStarted?.Invoke(currentInteract, worldPos);
    }

    private void HandleDragPerformed(Vector2 vector)
    {
        if (currentInteract == null) return;
        // currentState = GestureState.InteractingWithObject;
        // Debug.Log("Current position " + vector);
        Vector3 worldPos = point.ScreenToWorld(vector, currentInteract);
        InteractionEvents.OnDragPerformed?.Invoke(currentInteract, worldPos);
    }

    private void HandleDragEnded(Vector2 vector)
    {
        if (currentInteract == null) return;
        currentState = GestureState.Idle;

        Vector3 worldPos = point.ScreenToWorld(vector, currentInteract);
        InteractionEvents.OnDragEnded?.Invoke(currentInteract, worldPos);
    }

    private void HandleHoldPerformed(float obj)
    {
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldPerformed?.Invoke(currentInteract, obj);
    }

    private void HandleHoldCompleted()
    {
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldCompleted?.Invoke(currentInteract);
    }

    private void HandleHoldCanceled()
    {
        if (currentInteract == null) return;
        currentState = GestureState.Idle;
        InteractionEvents.OnHoldCanceled?.Invoke(currentInteract);
    }
}