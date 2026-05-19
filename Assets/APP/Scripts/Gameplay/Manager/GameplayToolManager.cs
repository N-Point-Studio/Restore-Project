using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct GameplayToolData
{
    [SerializeField] public ToolType toolType;
    [SerializeField] public GameObject toolPrefab;
}

public class GameplayToolManager : MonoBehaviour
{
    [SerializeField] private List<GameplayToolData> gameplayToolDataArray;

    public void SetToolVisibility(ToolType typeTarget, bool isVisible)
    {
        foreach (var toolData in gameplayToolDataArray)
        {
            if (toolData.toolPrefab != null)
            {
                if (typeTarget.HasFlag(toolData.toolType))
                {
                    Debug.Log($"Setting visibility of {toolData.toolPrefab.name} to {isVisible}");
                    toolData.toolPrefab.SetActive(isVisible);
                }
            }
        }
    }
}