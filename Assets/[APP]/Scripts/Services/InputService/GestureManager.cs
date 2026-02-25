using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GestureManager : IInitializable, IDisposable
{
    private enum GestureState { Idle, InteractingWithObject, Hold, Rotating, Zooming }
    private GestureState currentState = GestureState.Idle;
    private readonly PointService point;
    private readonly InputService input;
    private readonly RectTransform inspectZone;
    private readonly ClickService clickService;
    private readonly SwipeService swipeService;
    private readonly HoldService holdService;
    private readonly ZoomService zoomService;
    private IInteract currentInteract;

    [Inject]
    public GestureManager(PointService point, InputService input, ClickService click,
        RectTransform inspectZone, SwipeService swipe, HoldService hold, ZoomService zoom)
    {
        this.point = point;
        this.input = input;
        this.inspectZone = inspectZone;
        clickService = click;
        swipeService = swipe;
        holdService = hold;
        zoomService = zoom;
    }

    public void Initialize()
    {
        swipeService.OnInteractionStart += HandleInteractionStart;
        clickService.OnClickPerformed += HandleClick;
        swipeService.OnDragPerformed += HandleDrag;
        swipeService.OnRotatePerformed += HandleRotate;
        holdService.OnHoldPerformed += HandleHold;
        zoomService.OnZoomPerformed += HandleZoom;
        swipeService.OnInteractionEnd += HandleInteractionEnd;

        point.OnInteractDetected += HandleInteract;
    }

    public void Dispose()
    {
        swipeService.OnInteractionStart -= HandleInteractionStart;
        clickService.OnClickPerformed -= HandleClick;
        swipeService.OnDragPerformed -= HandleDrag;
        swipeService.OnRotatePerformed -= HandleRotate;
        holdService.OnHoldPerformed -= HandleHold;
        zoomService.OnZoomPerformed -= HandleZoom;
        swipeService.OnInteractionEnd -= HandleInteractionEnd;

        point.OnInteractDetected -= HandleInteract;
    }

    private void HandleInteract(IInteract interact)
    {
        currentInteract = interact;
    }

    private void HandleInteractionStart()
    {
        currentState = GestureState.Idle;
        currentInteract?.OnInteractStart();
    }

    private void HandleInteractionEnd()
    {
        if (currentInteract is IInspectable sm && currentState == GestureState.InteractingWithObject)
        {
            GestureEvents.OnDropPerformed?.Invoke(sm, point.ScreenToWorld(input.GetPrimaryPos(), currentInteract));
        }

        currentInteract?.OnInteractEnd();
        currentState = GestureState.Idle;
        currentInteract = null;
    }

    private bool IsInsideInspectZone(Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(inspectZone, screenPos);
    }

    private void HandleClick(Vector2 screenPos)
    {
        var target = currentInteract;
        Debug.Log($"Click performed on {target}");

        if (target is IClick click)
        {
            currentState = GestureState.InteractingWithObject;
            GestureEvents.OnClickPerformed?.Invoke(target as IInspectable, screenPos);
            click.OnClick();
        }
    }

    private void HandleHold(Vector2 screenPos)
    {
        var target = currentInteract;
        if (target is IHold holdable)
        {
            // currentState = GestureState.Hold;
            GestureEvents.OnHoldPerformed?.Invoke(target as IInteract);
            holdable.OnHoldPerformed();
        }
    }

    private void HandleDrag(Vector3 screenPos)
    {
        if (currentState == GestureState.Rotating || currentState == GestureState.Hold) return;
        var target = currentInteract;
        if (target is IDrag draggable)
        {
            currentState = GestureState.InteractingWithObject;
            var parent = GetTopParent(target as ArtefactPieceStateMachine);
            currentInteract = parent;
            Vector3 worldPos = point.ScreenToWorld(screenPos, parent);

            draggable.OnDragPerformed(worldPos);
            GestureEvents.OnDragPerformed?.Invoke(draggable, worldPos);
        }
    }

    ArtefactPieceStateMachine GetTopParent(ArtefactPieceStateMachine current)
    {
        while (current.parent != null)
        {
            // current = current.parent;
        }
        // Debug.Log("Drag Top parent: " + current.name);
        return current;
    }

    private void HandleRotate(Vector2 delta)
    {
        if (currentState == GestureState.InteractingWithObject || currentState == GestureState.Hold) return;
        // if (!IsInsideInspectZone(input.GetPrimaryPos())) return;
        currentState = GestureState.Rotating;
        // Debug.Log($"Rotate performed");
        GestureEvents.OnRotatePerformed?.Invoke(delta);
    }

    private void HandleZoom(float delta)
    {
        if (currentState == GestureState.InteractingWithObject || currentState == GestureState.Hold) return;
        // if (!IsInsideInspectZone(input.GetPrimaryPos())) return;
        GestureEvents.OnZoomPerformed?.Invoke(delta);
    }
}