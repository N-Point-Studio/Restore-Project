using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ToolUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Tool Identity")]
    [SerializeField] private ToolType toolType;

    [Header("DOTween Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float tweenDuration = 0.15f;

    public event Action<ToolUI, bool> OnToolClicked;

    public ToolType ToolType => toolType;
    public bool IsActive { get; private set; }

    private RectTransform rectTransform;
    private Image toolImage;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        toolImage = GetComponent<Image>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool newState = !IsActive;
        OnToolClicked?.Invoke(this, newState);
    }

    public void SetActiveState(bool state)
    {
        IsActive = state;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * hoverScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        rectTransform.DOKill();
    }

    public void SetVisibility(bool isShown)
    {
        gameObject.SetActive(isShown);
    }
}