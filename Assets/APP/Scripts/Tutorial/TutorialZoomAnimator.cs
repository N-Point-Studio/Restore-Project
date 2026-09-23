using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialZoomAnimator : TutorialAnimatorBase
{
    [Header("Physical Mouse Indicator")]
    [SerializeField] private RectTransform mouseRect;
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseScrollUpSprite;
    [SerializeField] private Sprite mouseScrollDownSprite;

    [Header("Concentric Rings Effect")]
    [SerializeField] private CanvasGroup ringsCanvasGroup;
    [SerializeField] private RectTransform ringsRect;

    private Sequence zoomSequence;

    protected override void OnEnable()
    {
        base.OnEnable();
        InteractionEvents.OnZoomPerformed += HandleZoomPerformed;
        HideUIElements();
        StartTutorialSequence(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InteractionEvents.OnZoomPerformed -= HandleZoomPerformed;
    }

    private void HandleZoomPerformed(float zoomDelta) => CompleteTutorial();

    protected override void SetupUIAndPlayAnimation()
    {
        if (mouseRect != null) mouseRect.gameObject.SetActive(true);

        if (overlayController != null && assemblyService != null)
        {
            overlayController.ShowOverlay(assemblyService.GetInspectPoint());
        }

        if (assemblyService != null && mainCam != null)
        {
            Vector2 centerPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
            if (ringsRect != null) ringsRect.position = centerPos;
            if (mouseRect != null) mouseRect.position = centerPos + new Vector2(250, -200);
        }

        PlayZoomAnimation();
    }

    protected override void HideUIElements()
    {
        if (mouseRect != null) mouseRect.gameObject.SetActive(false);
        if (ringsCanvasGroup != null) ringsCanvasGroup.alpha = 0f;
    }

    protected override void KillSequence() => zoomSequence?.Kill();

    private void PlayZoomAnimation()
    {
        KillSequence();
        zoomSequence = DOTween.Sequence();

        // 1. Simulate Scroll Up
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseScrollUpSprite;
            if (ringsRect != null && ringsCanvasGroup != null)
            {
                ringsRect.localScale = Vector3.one * 1.5f;
                ringsCanvasGroup.alpha = 0f;
                ringsRect.DOScale(Vector3.one * 0.5f, 1f).SetEase(Ease.OutQuad);
                ringsCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);
            }
        });
        
        zoomSequence.AppendInterval(1.2f);

        // 2. Simulate Scroll Down
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseScrollDownSprite;
            if (ringsRect != null && ringsCanvasGroup != null)
            {
                ringsRect.localScale = Vector3.one * 0.5f;
                ringsCanvasGroup.alpha = 1f;
                ringsRect.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutQuad);
                ringsCanvasGroup.DOFade(0f, 1f).SetEase(Ease.InQuad);
            }
        });

        zoomSequence.AppendInterval(1.2f);
        
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (ringsCanvasGroup != null) ringsCanvasGroup.alpha = 0f;
        });

        zoomSequence.AppendInterval(0.5f);
        zoomSequence.OnComplete(OnSequenceLoopComplete);
    }
}