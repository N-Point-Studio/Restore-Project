using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ClipBoardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private RectTransform animatedRect; 

    [Header("Animation Settings")]
    [SerializeField] private Vector2 hoverPosition;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float stayDuration = 0.5f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private Vector2 startPosition;
    private Tween moveTween;
    private bool isHovering = false;

    private void Awake()
    {
        if (animatedRect == null) 
            animatedRect = GetComponent<RectTransform>();

        startPosition = animatedRect.anchoredPosition;
    }

    private void OnEnable()
    {
        ProgressBarUI.OnProgressBarCompleted += ProgressCompleted;
    }

    private void OnDisable()
    {
        ProgressBarUI.OnProgressBarCompleted -= ProgressCompleted;
        
        // Safety: Reset cursor if the clipboard is disabled while hovering
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void ProgressCompleted()
    {
        if (isHovering) return; 

        moveTween?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Append(animatedRect.DOAnchorPos(hoverPosition, duration).SetEase(ease));
        seq.AppendInterval(stayDuration);
        seq.Append(animatedRect.DOAnchorPos(startPosition, duration).SetEase(ease));

        moveTween = seq;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        MoveTo(hoverPosition);
        
        // Set cursor to Hover state
        CursorController.instance?.SetCursorState(CursorState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        MoveTo(startPosition);
        
        // Reset cursor back to default
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void MoveTo(Vector2 target)
    {
        moveTween?.Kill();

        moveTween = animatedRect.DOAnchorPos(target, duration)
            .SetEase(ease);
    }
}