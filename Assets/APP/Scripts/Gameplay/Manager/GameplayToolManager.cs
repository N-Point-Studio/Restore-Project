using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
class GameplayToolData
{
    public ToolType toolType;
    
    [Header("Scene References")]
    [Tooltip("Drag the tool that is ALREADY in the scene for Desktop here")]
    public GameObject desktopTool;

    [Tooltip("Drag the tool that is ALREADY in the scene for Mobile here")]
    public GameObject mobileTool;
    
    [HideInInspector] public GameObject activeTool; 
}

public class GameplayToolManager : MonoBehaviour
{
    [SerializeField] private List<GameplayToolData> gameplayToolDataArray;
    
    [Header("Tool Parents")]
    [SerializeField] private GameObject desktopToolParent;
    [SerializeField] private GameObject mobileToolParent;

    private bool isInitialized = false;

    private void Awake()
    {
        InitializePlatformTools();
    }

    private void InitializePlatformTools()
    {
        if (isInitialized) return;
        isInitialized = true;

        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif

        // Turn on the correct parent container
        if (mobileToolParent != null) mobileToolParent.SetActive(isMobile);
        if (desktopToolParent != null) desktopToolParent.SetActive(!isMobile);

        foreach (var data in gameplayToolDataArray)
        {
            if (isMobile)
            {
                data.activeTool = data.mobileTool;
                if (data.desktopTool != null) data.desktopTool.SetActive(false); 
            }
            else
            {
                data.activeTool = data.desktopTool;
                if (data.mobileTool != null) data.mobileTool.SetActive(false); 
            }

            if (data.activeTool != null)
            {
                data.activeTool.SetActive(false);
            }
        }
    }

    public void SetToolVisibility(ToolType typeTarget, bool isVisible)
    {
        InitializePlatformTools();

        foreach (var toolData in gameplayToolDataArray)
        {
            if (toolData.activeTool != null)
            {
                if (typeTarget.HasFlag(toolData.toolType))
                {
                    Debug.Log($"Setting visibility of {toolData.activeTool.name} to {isVisible}");
                    toolData.activeTool.SetActive(isVisible);
                }
            }
        }
    }
}