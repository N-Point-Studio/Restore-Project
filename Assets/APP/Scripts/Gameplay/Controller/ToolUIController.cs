using System;
using System.Collections.Generic;
using UnityEngine;
using Modules;

public class ToolUIController : BaseMenuController
{
    [Header("Tool UI List")]
    [SerializeField] private List<ToolUI> tools = new List<ToolUI>();

    public event Action<ToolType> OnActiveToolChanged;

    private ToolUI currentActiveTool;

    protected override void Awake()
    {
        base.Awake();
        if (tools.Count == 0)
        {
            tools.AddRange(GetComponentsInChildren<ToolUI>(true));
        }

        foreach (var tool in tools)
        {
            tool.OnToolClicked += HandleToolClicked;
            tool.SetActiveState(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var tool in tools)
        {
            tool.OnToolClicked -= HandleToolClicked;
        }
    }

    private void HandleToolClicked(ToolUI clickedTool, bool requestedState)
    {
        if (currentActiveTool == clickedTool)
        {
            DeselectAllTools();
            return;
        }

        if (currentActiveTool != null)
        {
            currentActiveTool.SetActiveState(false);
        }

        clickedTool.SetActiveState(true);
        currentActiveTool = clickedTool;
        OnActiveToolChanged?.Invoke(currentActiveTool.ToolType);
    }

    public void DeselectAllTools()
    {
        if (currentActiveTool != null)
        {
            currentActiveTool.SetActiveState(false);
            currentActiveTool = null;
            OnActiveToolChanged?.Invoke(ToolType.None);
        }
    }
}