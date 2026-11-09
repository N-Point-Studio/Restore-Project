using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ToolUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Tool Identity")]
    [SerializeField] private ToolType toolType;

    [Header("Visual References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;

    [Header("Color Settings")]
    [SerializeField] private Color inactiveBgColor = Color.white;
    [SerializeField] private Color activeBgColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color inactiveIconColor = Color.white;
    [SerializeField] private Color activeIconColor = Color.black;

    [Header("DOTween Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float activeScale = 1.1f;
    [SerializeField] private float tweenDuration = 0.15f;

    public event Action<ToolUI, bool> OnToolClicked;

    public ToolType ToolType => toolType;
    public bool IsActive { get; private set; }

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (isInitialized) return;

        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();

        isInitialized = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool newState = !IsActive;
        OnToolClicked?.Invoke(this, newState);
    }

    public void SetActiveState(bool state)
    {
        IsActive = state;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        InitializeIfNeeded();

        rectTransform.DOKill();
        if (backgroundImage != null) backgroundImage.DOKill();
        if (iconImage != null) iconImage.DOKill();

        if (IsActive)
        {
            rectTransform.DOScale(originalScale * activeScale, tweenDuration).SetEase(Ease.OutQuad);
            if (backgroundImage != null) backgroundImage.DOColor(activeBgColor, tweenDuration);
            if (iconImage != null) iconImage.DOColor(activeIconColor, tweenDuration);
        }
        else
        {
            rectTransform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutQuad);
            if (backgroundImage != null) backgroundImage.DOColor(inactiveBgColor, tweenDuration);
            if (iconImage != null) iconImage.DOColor(inactiveIconColor, tweenDuration);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsActive) return;
        InitializeIfNeeded();
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * hoverScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsActive) return;
        InitializeIfNeeded();
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        if (rectTransform != null) rectTransform.DOKill();
        if (backgroundImage != null) backgroundImage.DOKill();
        if (iconImage != null) iconImage.DOKill();
    }

    public void SetVisibility(bool isShown)
    {
        Debug.Log($"SetVisibility called for {toolType} with isShown: {isShown}");
        if (gameObject.activeSelf != isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}