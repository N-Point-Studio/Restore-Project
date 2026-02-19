using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArtefactGroupItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textTitle;
    [SerializeField] private TMP_Text textDescription;
    [SerializeField] private ArtefactItemUI artefactItemUI; 
    [SerializeField] private Transform[] artefactItemContainers;

    private ArtefactGroupData artefactGroupData;
    public ArtefactGroupData ArtefactGroupData => artefactGroupData;
    
    public void Initialize(ArtefactGroupData artefactGroupData, ActiveArtefactData activeArtefactData)
    {
        this.artefactGroupData = artefactGroupData;

        if (textTitle) textTitle.text = artefactGroupData.BaseData.ItemName;
        if (textDescription) textDescription.text = artefactGroupData.BaseData.ItemDescription;

        SpawnArtefacts(activeArtefactData);
    }

    private void SpawnArtefacts(ActiveArtefactData activeArtefactData)
    {
        if (artefactGroupData == null || activeArtefactData == null) return;

        List<ArtefactData> items = artefactGroupData.ArtefactDatas;

        // Iterate through all available containers
        for (int i = 0; i < artefactItemContainers.Length; i++)
        {
            Transform container = artefactItemContainers[i];

            // Case 1: There is data for this slot
            if (i < items.Count)
            {
                ArtefactData data = items[i];
                ArtefactItemUI itemInstance = null;

                // Optimization: Try to reuse existing object in the container
                if (container.childCount > 0)
                {
                    itemInstance = container.GetChild(0).GetComponent<ArtefactItemUI>();

                    // If the existing child is not the correct component, destroy it
                    if (itemInstance == null)
                    {
                        foreach (Transform child in container) Destroy(child.gameObject);
                    }
                }

                // Instantiate only if no valid object exists
                if (itemInstance == null && artefactItemUI != null)
                {
                    itemInstance = Instantiate(artefactItemUI, container);
                }

                // Initialize with new data
                if (itemInstance != null)
                {
                    bool isUnlocked = activeArtefactData.IsArtefactUnlocked(data.BaseData.Id);
                    bool isCompleted = activeArtefactData.IsArtefactCompleted(data.BaseData.Id);
                    
                    itemInstance.Initialize(
                        data, 
                        !isUnlocked, 
                        isCompleted,
                        (clickedItem) => OnItemClicked(clickedItem)
                    );
                    
                    // Ensure object is active (in case it was pooled/disabled)
                    itemInstance.gameObject.SetActive(true);
                }
            }
            // Case 2: No data for this slot (Clean up leftover objects from previous page)
            else
            {
                for (int j = 0; j < container.childCount; j++)
                {
                    Destroy(container.GetChild(j).gameObject);
                }
            }
        }
    }

    private void OnItemClicked(ArtefactItemUI itemUI)
    {
        MainMenuEvents.OnRequestArtefactDetail?.Invoke(itemUI, artefactGroupData);
    }
}