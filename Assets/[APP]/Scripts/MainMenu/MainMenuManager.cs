using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Toggle dotIndicatorPrefab;
    [SerializeField] private ToggleGroup dotIndicatorToggleGroup;
    [SerializeField] private SimpleScrollSnap scrollSnap;

    private ActiveArtefactData activeArtefactData;

    [Inject]
    public void Construct(ActiveArtefactData activeArtefactData)
    {
        this.activeArtefactData = activeArtefactData;
    }

    private void Start()
    {
        SpawnArtefactGroups();
    }

    private void SpawnArtefactGroups()
    {
        ArtefactDatabase database = activeArtefactData.GetArtefactDatabase();
        if (database == null) return;

        ArtefactGroupData[] allGroups = database.GetAllItems();

        for (int i = 0; i < allGroups.Length; i++)
        {
            ArtefactGroupData group = allGroups[i];

            // Validate to ensure the group and its prefab exist
            if (group == null || group.PagePrefab == null)
            {
                Debug.LogWarning($"Group index {i} is null or missing PagePrefab!");
                continue;
            }

            AddArtifactGroupItem(group, i);
        }
    }

    private void AddArtifactGroupItem(ArtefactGroupData groupData, int index)
    {
        // 1. Setup Pagination (Dot Indicator)
        if (dotIndicatorPrefab != null && scrollSnap.Pagination != null)
        {
            Toggle toggle = Instantiate(dotIndicatorPrefab, scrollSnap.Pagination.transform);
            toggle.group = dotIndicatorToggleGroup;
            toggle.transform.localScale = Vector3.one; 
        }

        // 2. Add to ScrollSnap
        // We pass the .gameObject because the data holds the Component reference
        scrollSnap.Add(groupData.PagePrefab.gameObject, index);

        // 3. Retrieve Instance & Inject Data
        // Access the newly created panel from SimpleScrollSnap's panel array
        if (index < scrollSnap.Panels.Length)
        {
            RectTransform panelInstance = scrollSnap.Panels[index];
            if (panelInstance != null)
            {
                var groupUI = panelInstance.GetComponent<ArtefactGroupItemUI>();
                if (groupUI != null)
                {
                    // Pass the data and the active state handler
                    groupUI.Initialize(groupData, activeArtefactData);
                }
                else
                {
                    Debug.LogError($"Prefab in Group '{groupData.name}' is missing ArtefactGroupItemUI component!");
                }
            }
        }
    }

    public void RemoveArtifactGroupItem(int index)
    {
        if (scrollSnap.NumberOfPanels > 0)
        {
            // Remove Pagination Dot safely
            if (scrollSnap.Pagination != null && scrollSnap.Pagination.transform.childCount > index)
            {
                Destroy(scrollSnap.Pagination.transform.GetChild(index).gameObject);
            }

            // Remove Panel
            scrollSnap.Remove(index);
        }
    }
}