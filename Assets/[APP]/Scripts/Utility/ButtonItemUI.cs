using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ButtonItemUI : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Base Settings")]
    [SerializeField] private string label;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rectTransform;

    [Header("Changers (Index 0: Normal, Index 1: Hover/Selected)")]
    [SerializeField] private TextFontChanger fontChangerLabel;
    [SerializeField] private TextColorChanger fontColorChangerLabel;
    [SerializeField] private SpriteColorChanger spriteColorChanger;
    [SerializeField] private SpriteChanger spriteChanger;

    [Header("Size & Scale Animation")]
    [SerializeField] private bool changeSizeWhenSelected;
    [SerializeField] private Vector2 selectedSizeDelta;
    [SerializeField] private Vector3 selectedScale = Vector3.one;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool enableHoverEffects = true;
    [Tooltip("Scale modifier when button is pressed down (e.g., 0.95 means shrinks by 5%)")]
    [SerializeField] private float pressScaleModifier = 0.95f; 
    
    [Space(10)]
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    public Action OnClick;
    
    private Vector2 originalSizeDelta;
    private Vector3 originalScale;
    private bool isSelected = false;
    private bool isInteractable = true;

    protected override void Awake()
    {
        base.Awake();
        if (button == null) button = GetComponent<Button>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        originalSizeDelta = rectTransform.sizeDelta;
        originalScale = rectTransform.localScale;

        button.onClick.AddListener(HandleOnClick);
    }

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!string.IsNullOrEmpty(label) && textLabel != null)
        {
            textLabel.text = label;
        }
    }
    #endif

    protected override void OnDestroy()
    {
        base.OnDestroy();
        button.onClick.RemoveAllListeners();
        rectTransform.DOKill(); 
    }

    #region Pointer Events (Hover & Press)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || !enableHoverEffects) return;
        ApplyVisualState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || !enableHoverEffects) return;
        ApplyVisualState(isSelected);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable || !enableHoverEffects) return;

        rectTransform.DOKill();
        Vector3 pressScale = (changeSizeWhenSelected ? selectedScale : originalScale) * pressScaleModifier;
        rectTransform.DOScale(pressScale, tweenDuration / 2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable || !enableHoverEffects) return;

        bool isStillHovering = eventData.pointerCurrentRaycast.gameObject == gameObject;
        ApplyVisualState(isStillHovering || isSelected);
    }
    #endregion

    #region Logic & Setters
    private void HandleOnClick()
    {
        if (!isInteractable) return;
        OnClick?.Invoke();
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
        ApplyVisualState(isSelected);
    }

    public void SetLabel(string newLabel)
    {
        label = newLabel;
        if (textLabel != null) textLabel.text = label;
    }

    private void ApplyVisualState(bool showAsSelected)
    {
        rectTransform.DOKill();

        if (changeSizeWhenSelected)
        {
            rectTransform.DOSizeDelta(showAsSelected ? selectedSizeDelta : originalSizeDelta, tweenDuration).SetEase(tweenEase).SetUpdate(true);
            rectTransform.DOScale(showAsSelected ? selectedScale : originalScale, tweenDuration).SetEase(tweenEase).SetUpdate(true);
        }
        else
        {
            rectTransform.DOScale(originalScale, tweenDuration).SetEase(tweenEase).SetUpdate(true); 
        }

        int stateIndex = showAsSelected ? 1 : 0;

        if (spriteColorChanger != null) spriteColorChanger.ChangeColorSmooth(stateIndex, tweenDuration);
        if (fontColorChangerLabel != null) fontColorChangerLabel.ChangeColorSmooth(stateIndex, tweenDuration);
        if (spriteChanger != null) spriteChanger.ChangeSprite(stateIndex);
        if (fontChangerLabel != null) fontChangerLabel.ChangeFont(stateIndex);
    }
    #endregion
}