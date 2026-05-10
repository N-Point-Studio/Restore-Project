using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonInputInstructionUI : InputInstructionUI, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected Button button;
    public Button Button => button;

    public Action OnClick;

    [Header("Text Animation Settings")]
    [SerializeField] private RectTransform animatedTextRect; 
    [SerializeField] private TextColorChanger textDescriptionColorChanger; 
    [SerializeField] private TextFontChanger textDescriptionFontChanger; 
    [SerializeField] private SpriteColorChanger spriteInstructionBgColorChanger; 
    [SerializeField] private TextColorChanger textInstructionColorChanger; 
    [SerializeField] private TextFontChanger textInstructionFontChanger; 

    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScaleModifier = 0.95f;
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    [Header("Style Settings")]
    [Tooltip("If enabled, the button will use an alternate visual dark mode/light mode style")]
    [SerializeField] private bool useAlternateStyle = false;
    [SerializeField] private bool applyAlternateStyleOnStart = true;

    private Vector3 originalScale;

    protected override void Awake()
    {
        base.Awake();
        
        if (button != null)
        {
            button.onClick.AddListener(HandleOnButtonClicked);
        }

        if (animatedTextRect == null && textInfo != null)
        {
            animatedTextRect = textInfo.GetComponent<RectTransform>();
        }

        if (animatedTextRect != null)
        {
            originalScale = animatedTextRect.localScale;
        }

        if (applyAlternateStyleOnStart)
        {
            SetAlternateStyle(useAlternateStyle);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (button != null) button.onClick.RemoveListener(HandleOnButtonClicked);
        if (!gameObject.scene.isLoaded) return;
        if (animatedTextRect != null) animatedTextRect.DOKill();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void HandleOnButtonClicked()
    {
        if (button != null && !button.interactable) return;
        OnClick?.Invoke();
    }

    public void SetAlternateStyle(bool isAlternate)
    {
        useAlternateStyle = isAlternate;
        ApplyVisualState(false, false); 
    }

    #region Pointer Events (Hover & Press)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        ApplyVisualState(true, false);
        CursorController.instance?.SetCursorState(CursorState.Hover); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        ApplyVisualState(false, false);
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded); 
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        ApplyVisualState(true, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        bool isStillHovering = eventData.pointerCurrentRaycast.gameObject != null && 
                               (eventData.pointerCurrentRaycast.gameObject == gameObject || 
                                eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform));
        ApplyVisualState(isStillHovering, false);
    }
    #endregion

    private void ApplyVisualState(bool isHovered, bool isPressed)
    {
        if (animatedTextRect == null) return;

        int stateIndex = useAlternateStyle ? 1 : 0; 
        
        if (textDescriptionColorChanger != null) textDescriptionColorChanger.ChangeColorSmooth(stateIndex, tweenDuration);
        if (textDescriptionFontChanger != null) textDescriptionFontChanger.ChangeFont(stateIndex);
        
        if (spriteInstructionBgColorChanger != null) spriteInstructionBgColorChanger.ChangeColorSmooth(stateIndex, tweenDuration);
        if (textInstructionColorChanger != null) textInstructionColorChanger.ChangeColorSmooth(stateIndex, tweenDuration);
        if (textInstructionFontChanger != null) textInstructionFontChanger.ChangeFont(stateIndex);

        animatedTextRect.DOKill();
        Vector3 targetScale = originalScale;
        if (isHovered) targetScale = originalScale * hoverScale;
        if (isPressed) targetScale *= pressScaleModifier;

        animatedTextRect.DOScale(targetScale, tweenDuration).SetEase(isPressed ? Ease.OutBack : tweenEase);
    }
}