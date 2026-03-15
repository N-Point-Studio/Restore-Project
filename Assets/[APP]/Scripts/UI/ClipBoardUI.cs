using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ClipBoardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 hoverPosition;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private RectTransform rect;
    private Vector2 startPosition;
    private Tween moveTween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPosition = rect.anchoredPosition;
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
