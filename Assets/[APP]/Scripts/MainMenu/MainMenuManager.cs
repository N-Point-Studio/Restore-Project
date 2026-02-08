using UnityEditor.VisionOS;
using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    // Dependency
    private ActiveArtefactData activeArtefactData;

    [Inject]
    public void Construct(ActiveArtefactData activeArtefactData)
    {
        this.activeArtefactData = activeArtefactData;
    }

    private void Start()
    {
        // Example usage of activeArtefactData
        Debug.Log("Active Artefact Data initialized with " + activeArtefactData.GetType().Name);
        var artefactList = activeArtefactData.GetArtefactDatabase().GetAllItems();
        for (int i = 0; i < artefactList.Length; i++)
        {
            Debug.Log($"Artefact {i}: {artefactList[i].BaseData.ItemName}");
        }
    }
}
