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
    private ITool currentTool;

    [Inject]
    public ToolManager(InputSystemService inputSystemService)
    {
        this.inputSystemService = inputSystemService;
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
        if (interact is ITool tool) currentTool = tool;
    }

    private void HandlePressEnded(IInteractObject interact)
    {

    }

    private void HandleMouseMoved(Vector2 vector)
    {
        if (currentTool == null) return;
    }
}

public interface ITool
{
    void FollowMouse(Vector2 screenPos);
    void Use();
    void Return();
}

// public interface