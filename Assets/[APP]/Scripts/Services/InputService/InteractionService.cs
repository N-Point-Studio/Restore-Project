using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InteractionService : IInitializable, IDisposable, ITickable
{
    private readonly InputService gesture;
    private readonly PointService point;
    private readonly RectTransform inspectZone;
    private const float ClickThreshold = 20f;
    private const float ClickDuration = 0.2f;
    private const float HoldDuration = 0.5f;
    private Vector2 startPosition;
    private Vector2 lastPosition;
    private float startTime;
    private bool isDragging;
    private bool isRotating;
    private bool holdTriggered;
    private bool isPressing;
    private bool isEnd;
    private bool isZooming;
    private float lastPinchDistance;

    public event Action<IInteract> OnClick;
    public event Action<IInteract, Vector3> OnDrag;
    public event Action<IInteract> OnHold;

    [Inject]
    public InteractionService(InputService gesture, PointService point, RectTransform inspectZone)
    {
        Debug.Log("Interaction Service Constructed");
        this.gesture = gesture;
        this.point = point;
        this.inspectZone = inspectZone;
    }

    public void Initialize()
    {
        // Debug.Log("Interaction Service Initialized");

        gesture.OnPrimaryStarted += PressStart;
        gesture.OnPrimaryEnded += PressEnd;
        gesture.OnPrimaryMoved += PressMove;

        gesture.OnSecondaryStarted += SecondPressStart;
        gesture.OnSecondaryEnded += SecondPressEnd;
        gesture.OnSecondaryMoved += SecondPressMove;
    }

    public void Dispose()
    {
        gesture.OnPrimaryStarted -= PressStart;
        gesture.OnPrimaryEnded -= PressEnd;
        gesture.OnPrimaryMoved -= PressMove;

        gesture.OnSecondaryStarted -= SecondPressStart;
        gesture.OnSecondaryEnded -= SecondPressEnd;
        gesture.OnSecondaryMoved -= SecondPressMove;
    }

    public void Tick()
    {
        if (isPressing && !isDragging && !holdTriggered && !isZooming)
        {
            float duration = Time.time - startTime;

            if (duration >= HoldDuration)
            {
                var interactable = point.GetInteractObject();
                if (interactable is IHold holdable)
                {
                    holdTriggered = true;
                    holdable.OnHoldPerformed();
                    OnHold?.Invoke(point.GetInteractObject());
                }
            }
        }
    }

    private void PressStart(Vector2 position)
    {
        startPosition = position;
        lastPosition = position;
        startTime = Time.time;
        isDragging = false;
        holdTriggered = false;
        isPressing = true;
        isRotating = false;
        isEnd = false;

        point.GetInteractObject()?.OnStart();
    }

    private void PressMove(Vector2 position)
    {
        if (isEnd || isZooming) return;
        float distance = Vector2.Distance(startPosition, position);
        var interactable = point.GetInteractObject();
        if (interactable == null) return;
        Vector2 deltaPosition = position - lastPosition;
        lastPosition = position;

        if (!isDragging && distance > ClickThreshold)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            if (interactable is IRotate rotatable)
            {
                rotatable.OnRotatePerformed(deltaPosition);
                isRotating = true;
            }

            if (interactable is IDrag draggabe)
            {
                Vector3 worldPos = point.ScreenToWorld(position, interactable);
                draggabe.OnDragPerformed(worldPos);
            }
        }
    }

    private void PressEnd(Vector2 position)
    {
        isEnd = true;
        isPressing = false;
        float distance = Vector2.Distance(startPosition, position);
        float duration = Time.time - startTime;

        // Debug.Log($"isDragging {isDragging} && isRotating {isRotating}");
        if (isDragging)
        {
            Vector3 worldPos = point.ScreenToWorld(position, point.GetInteractObject());
            OnDrag?.Invoke(point.GetInteractObject(), worldPos);
        }

        if (!isDragging && !holdTriggered && distance <= ClickThreshold && duration <= ClickDuration)
        {
            (point.GetInteractObject() as IClick)?.OnClick();
            OnClick?.Invoke(point.GetInteractObject());
        }

        point.GetInteractObject()?.OnEnd();

        isRotating = false;
        isDragging = false;
        holdTriggered = false;
    }

    private void SecondPressStart(Vector2 position)
    {
        var interactable = point.GetInteractObject();
        if (interactable is not IZoom zoomable) return;

        isZooming = true;
        isDragging = false;
        isRotating = false;
        holdTriggered = false;

        Vector2 primaryPos = gesture.GetPrimaryPos();
        lastPinchDistance = Vector2.Distance(primaryPos, position);
    }

    private void SecondPressMove(Vector2 position)
    {
        if (!isZooming) return;

        var interactable = point.GetInteractObject();
        if (interactable is not IZoom zoomable) return;

        Vector2 primaryPos = gesture.GetPrimaryPos();
        float currentDistance = Vector2.Distance(primaryPos, position);

        float delta = currentDistance - lastPinchDistance;

        lastPinchDistance = currentDistance;

        zoomable.OnZoomPerformed(delta);
    }

    private void SecondPressEnd(Vector2 position)
    {
        isZooming = false;
    }
}