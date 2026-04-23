using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class StickyNote3DItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverOpacity = 0.7f;
    [SerializeField] private TMP_Text text3D; 
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float hoverRotationZ = 5f;
    [SerializeField] private float hoverAnimDuration = 0.2f;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player peelFeedback;
    
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Artefact3DItem brain;
    
    private Collider col;
    
    public static Action<StickyNote3DItem> OnNotePeeled;

    private bool isInitialized = false;

    private void SetupOriginalValues()
    {
        if (isInitialized) return;
        
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        
        col = GetComponent<Collider>();
        
        isInitialized = true;
    }

    private void Awake()
    {
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        
        col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private void OnDisable()
    {
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    public void Initialize(string noteText, Artefact3DItem boss)
    {
        SetupOriginalValues();
        
        brain = boss;
        if (text3D != null) text3D.text = noteText;
        
        spriteRenderer.color = originalColor;
        transform.DOKill(); 
        transform.localScale = originalScale; 
        transform.localEulerAngles = originalRotation;
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

        transform.DOKill();
        transform.DOScale(originalScale * hoverScaleMultiplier, hoverAnimDuration).SetEase(Ease.OutBack);
        transform.DOLocalRotate(originalRotation + new Vector3(0, 0, hoverRotationZ), hoverAnimDuration).SetEase(Ease.OutBack);

        // Change to Grab Open cursor since it's a sticky note
        CursorController.instance?.SetCursorState(CursorState.GrabOpen); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = originalColor;

        transform.DOKill();
        transform.DOScale(originalScale, hoverAnimDuration).SetEase(Ease.OutQuad);
        transform.DOLocalRotate(originalRotation, hoverAnimDuration).SetEase(Ease.OutQuad);

        CursorController.instance?.SetCursorState(CursorState.DefaultRounded); // Reset
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (brain == null || !brain.IsInDetailMode) return;

        spriteRenderer.color = originalColor;

        transform.DOKill();
        
        // Change cursor to grabbed on click!
        CursorController.instance?.SetCursorState(CursorState.GrabClose);

        if (peelFeedback != null)
        {
            ToggleCollider(false); 
            
            peelFeedback.Events.OnComplete.AddListener(OnPeelAnimationFinished);
            peelFeedback.PlayFeedbacks();
        }
        else
        {
            gameObject.SetActive(false);
            OnNotePeeled?.Invoke(this);
        }
    }

    private void OnPeelAnimationFinished()
    {
        peelFeedback.Events.OnComplete.RemoveListener(OnPeelAnimationFinished);
        gameObject.SetActive(false);
        OnNotePeeled?.Invoke(this);
        
        // Reset after animation
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }
}