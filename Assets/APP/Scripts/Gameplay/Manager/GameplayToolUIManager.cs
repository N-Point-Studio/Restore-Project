using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

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

    private TutorialService tutorialService;

    public bool IsTutorialAvailable { get; set; }


    [Inject]
    public void Construct(TutorialService tutorialService)
    {
        this.tutorialService = tutorialService;
    }

    private void OnEnable()
    {
        if (tutorialService != null) tutorialService.OnTutorialStateChanged += UpdateTrayVisibility;
    }
    private void OnDisable()
    {
        if (tutorialService != null) tutorialService.OnTutorialStateChanged -= UpdateTrayVisibility;
    }

    private void Start()
    {
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

    public void InitializeTools()
    {
        foreach (var data in toolDataList)
        {
            if (data.tool != null)
            {
                data.tool.SetActive(false);
                data.toolUI.SetVisibility(false);
            }
        }
    }

    private void UpdateTrayVisibility(int stage, int module)
    {
        if (!IsTutorialAvailable) return;
        SetToolVisibility(ToolType.Chisel, stage > 0 || module >= 3);
        SetToolVisibility(ToolType.Brush, stage > 0 || module >= 4);
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
        foreach (var data in toolDataList)
        {
            if (data.toolUI != null && typeTarget.HasFlag(data.toolType))
            {
                Debug.Log($"Setting visibility for {data.toolType} to {isVisible}");
                data.toolUI.SetVisibility(isVisible);
            }
        }
    }

    public void ReleaseAllTool()
    {
        toolUIController.DeselectAllTools();
    }
}