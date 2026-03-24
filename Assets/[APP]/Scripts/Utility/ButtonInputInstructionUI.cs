using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonInputInstructionUI : InputInstructionUI, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected Button button;
    public Button Button => button;

    [Header("Text Animation Settings")]
    [SerializeField] private RectTransform animatedTextRect;
    [SerializeField] private TextColorChanger textColorChanger;
    [SerializeField] private TextFontChanger textFontChanger;

    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScaleModifier = 0.95f;
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    private Vector3 originalScale;

    public Action OnClick;

    protected override void Awake()
    {
        base.Awake();
        button.onClick.AddListener(HandleOnButtonClicked);

        if (animatedTextRect == null && textInfo != null)
        {
            animatedTextRect = textInfo.GetComponent<RectTransform>();
        }

        if (animatedTextRect != null)
        {
            originalScale = animatedTextRect.localScale;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        button.onClick.RemoveListener(HandleOnButtonClicked);
        if (animatedTextRect != null) animatedTextRect.DOKill();
    }

    private void HandleOnButtonClicked()
    {
        OnClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        ApplyVisualState(true, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;
        ApplyVisualState(false, false);
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

    private void ApplyVisualState(bool isHovered, bool isPressed)
    {
        if (animatedTextRect == null) return;

        animatedTextRect.DOKill();

        Vector3 targetScale = originalScale;
        if (isHovered) targetScale = originalScale * hoverScale;
        if (isPressed) targetScale *= pressScaleModifier;

        animatedTextRect.DOScale(targetScale, tweenDuration).SetEase(isPressed ? Ease.OutBack : tweenEase);

        int stateIndex = isHovered ? 1 : 0;
        
        if (textColorChanger != null) textColorChanger.ChangeColorSmooth(stateIndex, tweenDuration);
        if (textFontChanger != null) textFontChanger.ChangeFont(stateIndex);
    }
}
