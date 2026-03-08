using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public interface IPressHandler
{
    void OnPressStart();
    void OnPressEnd();
}

public class ToolManager : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly ObjectDetectionService objectDetectionService;
    private ITool currentTool;

    [Inject]
    public ToolManager(InputSystemService inputSystemService, ObjectDetectionService objectDetectionService)
    {
        this.inputSystemService = inputSystemService;
        this.objectDetectionService = objectDetectionService;
    }

    public void Initialize()
    {
        InteractionEvents.OnPressStarted += HandlePressStarted;
        inputSystemService.OnMouseMoved += HandleMouseMoved;
        InteractionEvents.OnPressEnded += HandlePressEnded;
    }

    public void Dispose()
    {
        InteractionEvents.OnPressStarted -= HandlePressStarted;
        inputSystemService.OnMouseMoved -= HandleMouseMoved;
        InteractionEvents.OnPressEnded -= HandlePressEnded;
    }

    private void HandlePressStarted(IInteractObject interact)
    {

    }

    private void HandlePressEnded(IInteractObject interact)
    {
        if (interact is ITool tool)
        {
            if (currentTool != tool)
            {
                currentTool?.Return();
                currentTool = tool;
                currentTool?.Use();
            }
        }
    }

    private void HandleMouseMoved(Vector2 vector)
    {
        if (currentTool == null) return;
        if (currentTool is IInteractObject interact)
        {
            var worldPos = objectDetectionService.ScreenToWorld(vector, interact);
            currentTool.FollowMouse(worldPos);
        }
    }
}

public interface ITool
{
    void FollowMouse(Vector3 worldPos);
    void Use();
    void Return();
}

// public interface