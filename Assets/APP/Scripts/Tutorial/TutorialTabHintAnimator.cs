using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class TutorialTabHintAnimator : TutorialAnimatorBase
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform tabKeyIcon;
    [SerializeField] private RectTransform radiatingLines;
    [SerializeField] private CanvasGroup radiatingLinesCanvasGroup;

    private Sequence hintSequence;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        InteractionEvents.OnTabPerformed += HandleTabPerformed;
        
        StartTutorialSequence(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InteractionEvents.OnTabPerformed -= HandleTabPerformed;
    }

    private void HandleTabPerformed()
    {
        CompleteTutorial();
    }

    protected override void SetupUIAndPlayAnimation()
    {
        if (tabKeyIcon != null) tabKeyIcon.gameObject.SetActive(true);
        if (radiatingLines != null) radiatingLines.gameObject.SetActive(true);

        if (overlayController != null) 
        {
            overlayController.ShowOverlay(null);
        }

        PlayTabAnimation();
    }

    protected override void HideUIElements()
    {
        if (tabKeyIcon != null) tabKeyIcon.gameObject.SetActive(false);
        if (radiatingLines != null) radiatingLines.gameObject.SetActive(false);
        
        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence()
    {
        hintSequence?.Kill();
    }

    private void PlayTabAnimation()
    {
        KillSequence();

        // Reset starting states
        tabKeyIcon.localScale = Vector3.one;
        radiatingLines.localScale = Vector3.one * 0.8f;
        if (radiatingLinesCanvasGroup != null) radiatingLinesCanvasGroup.alpha = 0f;

        hintSequence = DOTween.Sequence();

        // 1. Pause briefly before the press
        hintSequence.AppendInterval(0.5f);

        // 2. "Press" the key down
        hintSequence.Append(tabKeyIcon.DOScale(new Vector3(0.85f, 0.85f, 1f), 0.15f).SetEase(Ease.OutQuad));
        
        // 3. Lines burst outward and fade in simultaneously
        hintSequence.Join(radiatingLines.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack));
        if (radiatingLinesCanvasGroup != null)
        {
            hintSequence.Join(radiatingLinesCanvasGroup.DOFade(1f, 0.1f));
        }

        // 4. "Release" the key back up
        hintSequence.Append(tabKeyIcon.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBounce));

        // 5. Lines fade away and shrink back down
        if (radiatingLinesCanvasGroup != null)
        {
            hintSequence.Join(radiatingLinesCanvasGroup.DOFade(0f, 0.3f));
        }
        hintSequence.Join(radiatingLines.DOScale(0.8f, 0.3f));

        // Loop this sequence forever until they press Tab
        hintSequence.SetLoops(-1).SetUpdate(true);
    }
}