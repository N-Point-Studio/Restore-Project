using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public interface IGestureState
{
    void Enter();
    void Exit();
}

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

        pressService = press;
        dragService = swipe;
        holdService = hold;

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

    private void HandleInteractDetected(IInteractObject interact)
    {
        // if (currentState == GestureState.InteractingWithObject) return;
        // if (currentInteract == interact) return;
        currentInteract = interact;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnPressStarted?.Invoke(currentInteract);
    }

    public IInteractObject GetCurrentInteract()
    {
        return currentInteract;
    }

    private void HandlePressStarted()
    {
        if (currentInteract == null) return;
        // currentState = GestureState.InteractingWithObject;
        // Debug.Log("Hit pertama");
        // InteractionEvents.OnPressStarted?.Invoke(currentInteract);
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
        Vector3 worldPos = detectionService.ScreenToWorld(vector, currentInteract);
        InteractionEvents.OnDragStarted?.Invoke(currentInteract, worldPos);
    }

    // private void HandleDragPerformed(Vector2 vector)
    // {
    //     if (currentInteract == null) return;
    //     // currentState = GestureState.InteractingWithObject;
    //     // Debug.Log("Current position " + vector);
    //     Vector3 worldPos = detectionService.ScreenToWorld(vector, currentInteract);
    //     InteractionEvents.OnDragPerformed?.Invoke(currentInteract, worldPos);
    // }

    private void HandleDragPerformed(Vector2 vector)
    {
        if (currentInteract == null) return;

        Vector3 worldPos = detectionService.ScreenToWorld(vector);
        InteractionEvents.OnDragPerformed?.Invoke(currentInteract, worldPos);
    }

    private void HandleDragEnded(Vector2 vector)
    {
        if (currentInteract == null) return;
        currentState = GestureState.Idle;

        Vector3 worldPos = detectionService.ScreenToWorld(vector, currentInteract);
        InteractionEvents.OnDragEnded?.Invoke(currentInteract, worldPos);
    }

    private void HandleHoldPerformed(float obj)
    {
        if (cleaningService.isCleaning) return;
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldPerformed?.Invoke(currentInteract, obj);
        // Debug.Log("object interaction hold performed");
    }

    private void HandleHoldCompleted()
    {
        if (cleaningService.isCleaning) return;
        if (currentInteract == null) return;
        currentState = GestureState.InteractingWithObject;
        InteractionEvents.OnHoldCompleted?.Invoke(currentInteract);
        // Debug.Log("object interaction hold completed");

    }

    private void HandleHoldCanceled()
    {
        if (cleaningService.isCleaning) return;
        if (currentInteract == null) return;
        currentState = GestureState.Idle;
        InteractionEvents.OnHoldCanceled?.Invoke(currentInteract);
        // Debug.Log("object interaction hold canceled");

    }
}