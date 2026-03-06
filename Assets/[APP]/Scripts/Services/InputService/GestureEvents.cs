using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public static class GestureEvents
{
    public static Action<IInteract> OnInteractionStart;

    public static Action<IInspectable, Vector3> OnDropPerformed;
    public static Action<IInteract, Vector2> OnClickPerformed;
    public static Action<IInteract> OnHoldPerformed;
    public static Action<IInteract, Vector3> OnDragPerformed;

    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;
    public static Action<IInteract> OnInteractionEnd;
}

public static class InteractionEvents
{
    public static Action<IInteractObject> OnPressStarted;
    public static Action<IInteractObject> OnPressEnded;

    public static Action<IInteractObject> OnDragStarted;
    public static Action<IInteractObject, Vector3> OnDragPerformed;
    public static Action<IInteractObject> OnDragEnded;

    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;

    public static Action<float> OnHoldPerformed;
    public static Action OnHoldCanceled;
    public static Action OnHoldCompleted;
}