using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public interface IGestureState { void Enter(); void Exit(); }

public class ObjectInteractionManager : IInitializable, IDisposable
{
    private enum GestureState { Idle, InteractingWithObject }
    private GestureState currentState = GestureState.Idle;
    private readonly ObjectDetectionService detectionService;
    private readonly ObjectPressService pressService;
    private readonly ObjectDragService dragService;
    private readonly ObjectHoldService holdService;
    private readonly CleaningService cleaningService;
    private IInteractObject currentInteract;

    [Inject]
    public ObjectInteractionManager(ObjectDetectionService detectionService, ObjectPressService press,
        ObjectDragService swipe, ObjectHoldService hold, CleaningService cleaningService)
    {
        this.detectionService = detectionService;
        this.pressService = press;
        this.dragService = swipe;
        this.holdService = hold;
        this.cleaningService = cleaningService;
    }

    public void Initialize()
    {
        detectionService.OnInteractDetected += HandleInteractDetected;
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
        detectionService.OnInteractDetected -= HandleInteractDetected;
        pressService.OnPressStarted -= HandlePressStarted;
        pressService.OnPressEnded -= HandlePressEnded;
        dragService.OnDragStarted -= HandleDragStart;
        dragService.OnDragPerformed -= HandleDragPerformed;
        dragService.OnDragEnded -= HandleDragEnded;
        holdService.OnHoldPerformed -= HandleHoldPerformed;
        holdService.OnHoldCompleted -= HandleHoldCompleted;
        holdService.OnHoldCanceled -= HandleHoldCanceled;
    }

    private void HandleInteractDetected(IInteractObject interact) { currentInteract = interact; }

    private bool IsInteractValid()
    {
        if (currentInteract == null) return false;
        if (currentInteract is MonoBehaviour mono && mono == null) return false;
        return true;
    }

    private void HandlePressStarted()
    {
        InteractionEvents.OnPressStart?.Invoke();

        currentState = GestureState.InteractingWithObject;
        detectionService.SetInteractObjectUsed(true);
    }

    private void HandlePressEnded()
    {
        InteractionEvents.OnPressEnd?.Invoke();

        currentState = GestureState.Idle;
        detectionService.SetInteractObjectUsed(false);
    }

    private void HandleDragStart(Vector2 vector)
    {
        if (!IsInteractValid()) return;

        currentState = GestureState.InteractingWithObject;
        detectionService.SetInteractObjectUsed(true);

        detectionService.CacheDragDepth(currentInteract);

        if (currentInteract is not IDraggableTool)
        {
            Vector3 worldPos = detectionService.ScreenToWorld(vector);
            InteractionEvents.OnDragStarted?.Invoke(currentInteract, worldPos);
        }
        else
        {
            Vector3 worldPos = detectionService.ScreenToWorld(vector, currentInteract);
            InteractionEvents.OnDragStarted?.Invoke(currentInteract, worldPos);
        }

    }

    private void HandleDragPerformed(Vector2 vector)
    {
        if (!IsInteractValid()) return;

        if (currentInteract is not IDraggableTool)
        {
            Vector3 worldPos = detectionService.ScreenToWorld(vector);
            InteractionEvents.OnDragPerformed?.Invoke(currentInteract, worldPos);
        }
        else
        {
            Vector3 worldPos = detectionService.ScreenToWorld(vector, currentInteract);
            InteractionEvents.OnDragPerformed?.Invoke(currentInteract, worldPos);
        }
    }

    private void HandleDragEnded(Vector2 vector)
    {
        if (IsInteractValid())
        {
            Vector3 worldPos = detectionService.GetCachedDragWorldPos(vector);
            InteractionEvents.OnDragEnded?.Invoke(currentInteract, worldPos);
        }

        currentState = GestureState.Idle;
        detectionService.SetInteractObjectUsed(false);
    }

    private void HandleHoldPerformed(float val, Vector2 position)
    {
        if (cleaningService.isCleaning || !IsInteractValid()) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldPerformed?.Invoke(currentInteract, val, position);
    }

    private void HandleHoldCompleted(Vector2 position)
    {
        if (cleaningService.isCleaning || !IsInteractValid()) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldCompleted?.Invoke(currentInteract, position);
    }

    private void HandleHoldCanceled(Vector2 position)
    {
        if (cleaningService.isCleaning) return;
        if (IsInteractValid()) InteractionEvents.OnHoldCanceled?.Invoke(currentInteract, position);
        currentState = GestureState.Idle;
    }
}