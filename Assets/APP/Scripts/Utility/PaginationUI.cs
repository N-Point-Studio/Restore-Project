using UnityEngine;
using UnityEngine.UI;

public class PaginationUI : MonoBehaviour
{
    [SerializeField] private Toggle dotIndicatorPrefab;
    [SerializeField] private ToggleGroup dotIndicatorToggleGroup;
    [SerializeField] private Transform paginationContainer;

    public void AddIndicator()
    {
        if (dotIndicatorPrefab == null || paginationContainer == null) return;

        Toggle toggle = Instantiate(dotIndicatorPrefab, paginationContainer);
        toggle.group = dotIndicatorToggleGroup;
        toggle.transform.localScale = Vector3.one;
        
        toggle.name = $"Dot_{paginationContainer.childCount - 1}";
    }

    public void RemoveIndicator(int index)
    {
        if (paginationContainer.childCount > index)
        {
            Destroy(paginationContainer.GetChild(index).gameObject);
        }
    }

    public void ClearAll()
    {
        foreach (Transform child in paginationContainer)
        {
            Destroy(child.gameObject);
        }
    }
}