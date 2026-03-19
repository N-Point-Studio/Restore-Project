using System;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class StickyNote3DItem : MonoBehaviour
{
    [Header("Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverOpacity = 0.7f;
    [SerializeField] private TMP_Text text3D;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    public static Action<StickyNote3DItem> OnNotePeeled;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Initialize(string noteText)
    {
        if (text3D != null) text3D.text = noteText;
        
        spriteRenderer.color = originalColor;
        gameObject.SetActive(true);
    }

    private void OnMouseEnter()
    {
        Color hoverColor = originalColor;
        hoverColor.a = hoverOpacity;
        spriteRenderer.color = hoverColor;
    }

    private void OnMouseExit()
    {
        spriteRenderer.color = originalColor;
    }

    private void OnMouseDown()
    {
        spriteRenderer.color = originalColor;
        gameObject.SetActive(false);
        OnNotePeeled?.Invoke(this);
    }
}