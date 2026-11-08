using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToolData
{
    public ToolType toolType;
    public GameObject tool;
    public ToolUI toolUI;
}

public class GameplayToolUIManager : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] private List<ToolData> toolDataList;
    [SerializeField] private ToolUIController toolUIController;

    private GameObject currentActiveToolObj;
    public event Action<ToolType> OnActiveToolChanged;

    private void Start()
    {
        InitializeTools();

        if (toolUIController != null)
        {
            toolUIController.OnActiveToolChanged += HandleToolSelected;
        }
    }

    private void OnDestroy()
    {
        if (toolUIController != null)
        {
            toolUIController.OnActiveToolChanged -= HandleToolSelected;
        }
    }

    private void InitializeTools()
    {
        foreach (var data in toolDataList)
        {
            if (data.tool != null) data.tool.SetActive(false);
        }
    }

    private void HandleToolSelected(ToolType selectedType)
    {
        if (currentActiveToolObj != null)
        {
            currentActiveToolObj.SetActive(false);
            currentActiveToolObj = null;
        }

        if (selectedType == ToolType.None)
        {
            OnActiveToolChanged?.Invoke(ToolType.None);
            return;
        }

        foreach (var data in toolDataList)
        {
            if (data.toolType == selectedType && data.tool != null)
            {
                currentActiveToolObj = data.tool;
                currentActiveToolObj.SetActive(true);
                break;
            }
        }

        OnActiveToolChanged?.Invoke(selectedType);
    }

    public GameObject GetToolObject(ToolType type)
    {
        foreach (var data in toolDataList)
        {
            if (data.toolType == type) return data.tool;
        }
        return null;
    }

    public void SetToolVisibility(ToolType typeTarget, bool isVisible)
    {
        Debug.Log($"GameplayToolUIManager: Setting visibility of tools with type {typeTarget} to {isVisible}");
        foreach (var data in toolDataList)
        {
            if (data.toolUI != null && typeTarget.HasFlag(data.toolType))
            {
                data.toolUI.SetVisibility(isVisible);
            }
            else
            {
                data.toolUI.SetVisibility(!isVisible);
            }
        }
    }

    public void ReleaseAllTool()
    {
        toolUIController.DeselectAllTools();
    }
}