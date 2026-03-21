using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class StickyNote3DItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverOpacity = 0.7f;
    [SerializeField] private TMP_Text text3D; 
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Color originalColor;
    private Artefact3DItem brain;
    
    private Collider col;
    
    public static Action<StickyNote3DItem> OnNotePeeled;

    private void Awake()
    {
        originalColor = spriteRenderer.color;
        
        col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void Initialize(string noteText, Artefact3DItem boss)
    {
        brain = boss;
        if (text3D != null) text3D.text = noteText;
        
        spriteRenderer.color = originalColor;
        gameObject.SetActive(true);
    }

    public void ToggleCollider(bool isOn)
    {
        if (col != null) col.enabled = isOn;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (brain == null || !brain.IsInDetailMode) return;

        Color hoverColor = originalColor;
        hoverColor.a = hoverOpacity;
        spriteRenderer.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (brain == null || !brain.IsInDetailMode) return;

        spriteRenderer.color = originalColor;
        gameObject.SetActive(false);
        OnNotePeeled?.Invoke(this);
    }
}