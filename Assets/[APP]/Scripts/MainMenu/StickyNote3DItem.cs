using System;
using UnityEngine;
using TMPro; 

[RequireComponent(typeof(Collider))]
public class StickyNote3DItem : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private string label;
    [SerializeField] private TMP_Text text3D; 
    
    public static Action<StickyNote3DItem> OnNotePeeled;

    private void Awake()
    {
        if (outline == null) outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    private void OnMouseEnter()
    {
        if (outline != null) outline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (outline != null) outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (outline != null) outline.enabled = false;
        gameObject.SetActive(false);
        OnNotePeeled?.Invoke(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (text3D != null && !string.IsNullOrEmpty(label))
        {
            text3D.text = label;
        }
    }
#endif
}