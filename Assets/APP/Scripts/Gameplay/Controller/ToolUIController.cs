using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolUIController : BaseMenuController
{
    [Header("Tool UI List")]
    [Tooltip("Kosongkan jika script ini ada di Parent objek yang sama dengan kumpulan ToolUI")]
    [SerializeField] private List<ToolUI> tools = new List<ToolUI>();

    public event Action<ToolType> OnActiveToolChanged;

    private ToolUI currentActiveTool;

    protected override void Start()
    {
        base.Start();
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
            currentActiveTool.SetActiveState(false);
            currentActiveTool = null;
            OnActiveToolChanged?.Invoke(ToolType.None);
        }
        else
        {
            if (currentActiveTool != null)
            {
                currentActiveTool.SetActiveState(false);
            }

            clickedTool.SetActiveState(true);
            currentActiveTool = clickedTool;
            OnActiveToolChanged?.Invoke(currentActiveTool.ToolType);
        }
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

    // Tambahkan ini di dalam ToolUIController.cs
    public override void SetActive(bool isActive)
    {
        if (root == null) return;

        // Panggil logikanya secara manual tanpa tween
        root.SetActive(isActive);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isActive ? 1f : 0f;
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;
        }
    }
}