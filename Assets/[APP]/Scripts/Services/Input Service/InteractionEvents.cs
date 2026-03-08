using System;
using UnityEngine;

public static class InteractionEvents
{
    public static Action<Vector2> OnMouseMoved;

    public static Action<IInteractObject> OnPressStarted;
    public static Action<IInteractObject> OnPressEnded;

    public static Action<IInteractObject, Vector3> OnDragStarted;
    public static Action<IInteractObject, Vector3> OnDragPerformed;
    public static Action<IInteractObject, Vector3> OnDragEnded;

    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;

    public static Action<IInteractObject, float> OnHoldPerformed;
    public static Action<IInteractObject> OnHoldCanceled;
    public static Action<IInteractObject> OnHoldCompleted;
}