using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public static class GestureEvents
{
    public static Action OnInteractionStart;
    public static Action<Vector3> OnDragPerformed;
    public static Action<Vector2> OnRotatePerformed;
    public static Action<float> OnZoomPerformed;
    public static Action OnInteractionEnd;
    public static Action<Vector2> OnClickPerformed;
    public static Action<Vector2> OnHoldPerformed;
}