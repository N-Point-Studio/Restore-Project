using UnityEngine;
using System.Collections.Generic;

public class TutorialOverlayUI : BaseMenuController
{
    [Header("Overlay Camera Layer")]
    [Tooltip("The name of the layer rendered by your Overlay Camera (e.g., 'TutorialHighlight')")]
    [SerializeField] private string highlightLayerName = "TutorialHighlight";

    private int highlightLayerIndex;
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    private Transform currentTarget;

    protected override void Awake()
    {
        base.Awake();
        highlightLayerIndex = LayerMask.NameToLayer(highlightLayerName);
    }

    public void ShowOverlay(Transform target3DObject)
    {
        if (target3DObject != null)
        {
            currentTarget = target3DObject;
            originalLayers.Clear();
            SetLayerRecursive(currentTarget.gameObject, highlightLayerIndex);
        }
        
        SetActive(true); 
    }

    public void HideOverlay()
    {
        if (currentTarget != null)
        {
            RestoreLayers();
            currentTarget = null;
        }
        
        SetActive(false); 
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        // Cache original layer
        originalLayers[obj] = obj.layer;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }

    private void RestoreLayers()
    {
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null) kvp.Key.layer = kvp.Value;
        }
        originalLayers.Clear();
    }
}