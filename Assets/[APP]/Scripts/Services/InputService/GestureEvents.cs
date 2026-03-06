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
    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;
}