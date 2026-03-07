// using System;
// using UnityEngine;
// using VContainer;
// using VContainer.Unity;

// public interface IPressHandler
// {
//     void OnPressStart();
//     void OnPressEnd();
// }

// public class ToolManager : IInitializable, IDisposable
// {
//     private readonly GestureManager gesture;
//     private readonly PointService point;

//     [Inject]
//     public ToolManager(GestureManager gesture, PointService point)
//     {
//         this.gesture = gesture;
//         this.point = point;
//     }

//     public void Initialize()
//     {
//         InteractionEvents.OnPressStarted += HandlePressStarted;
//         InteractionEvents.OnMouseMoved += HandleMouseMoved;
//         InteractionEvents.OnPressEnded += HandlePressEnded;
//     }

//     public void Dispose()
//     {
//         InteractionEvents.OnPressStarted -= HandlePressStarted;
//         InteractionEvents.OnMouseMoved -= HandleMouseMoved;
//         InteractionEvents.OnPressEnded -= HandlePressEnded;
//     }

//     private void HandlePressStarted(IInteractObject interact)
//     {
//         if (gesture.GetCurrentInteract() is not ITool) return;

//     // }

//     private void HandlePressEnded(IInteractObject interact)
//     {
//         if (gesture.GetCurrentInteract() is not ITool tool) return;
//         tool.Use();
//     }

//     private void HandleMouseMoved(Vector2 vector)
//     {
//         if (gesture.GetCurrentInteract() is not ITool) return;
//     }
// }

// public interface ITool
// {
//     void FollowMouse(Vector3 worldPos);
//     void Use();
//     void Return();
// }