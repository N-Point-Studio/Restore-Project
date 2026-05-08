using System;
using UnityEngine;

public static class InteractionEvents
{
    public static Action<Vector2> OnMouseMoved;

    public static Action OnTabPerformed;
    public static Action OnTabCanceled;

    public static Action<IInteractObject> OnPressStarted;
    public static Action OnPressStart;
    public static Action<IInteractObject> OnPressEnded;
    public static Action OnPressEnd;

    public static Action<IInteractObject, Vector3> OnDragStarted;
    public static Action<IInteractObject, Vector3> OnDragPerformed;
    public static Action<IInteractObject, Vector3> OnDragEnded;

    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;

    public static Action<IInteractObject, float, Vector2> OnHoldPerformed;
    public static Action<IInteractObject, Vector2> OnHoldCanceled;
    public static Action<IInteractObject, Vector2> OnHoldCompleted;
}