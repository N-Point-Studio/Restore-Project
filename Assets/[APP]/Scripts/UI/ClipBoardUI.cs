using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class ClipBoardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 hoverPosition;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float stayDuration = 0.5f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private RectTransform rect;
    private Vector2 startPosition;
    private Tween moveTween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPosition = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        ProgressBarUI.OnProgressBarCompleted += ProgressCompleted;
    }

    private void OnDisable()
    {
        ProgressBarUI.OnProgressBarCompleted -= ProgressCompleted;
    }

    private void ProgressCompleted()
    {
        moveTween?.Kill();

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOAnchorPos(hoverPosition, duration).SetEase(ease));
        seq.AppendInterval(stayDuration);
        seq.Append(rect.DOAnchorPos(startPosition, duration).SetEase(ease));

        moveTween = seq;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MoveTo(hoverPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MoveTo(startPosition);
    }

    private void MoveTo(Vector2 target)
    {
        moveTween?.Kill();

        moveTween = rect.DOAnchorPos(target, duration)
            .SetEase(ease);
    }
}