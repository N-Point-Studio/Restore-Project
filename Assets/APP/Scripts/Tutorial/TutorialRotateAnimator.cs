using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialRotateAnimator : TutorialAnimatorBase
{
    [Header("Physical Mouse Indicator")]
    [SerializeField] private RectTransform mouseRect;
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseRightClickSprite;
    
    [Header("Click Indicator Lines")]
    [SerializeField] private GameObject clickIndicator; 

    [Header("Top Rotate Icon")]
    [SerializeField] private RectTransform rotateIconRect;
    [SerializeField] private Vector2 rotateIconOffset = new Vector2(0, 150);
    
    [Header("Dotted Line Setup")]
    [SerializeField] private RectTransform lineMask;
    [SerializeField] private float lineMaxWidth = 400f; 

    [Header("Mouse Path Setup")]
    [SerializeField] private Vector2 mouseOffset = new Vector2(0, -150); 
    [SerializeField] private float travelDistance = 200f;
    [SerializeField] private float moveDuration = 1.5f;

    private Sequence rotateSequence;
    private Vector2 maskStartSize;
    private Vector2 cachedCenterPos; 
    private Quaternion iconStartRotation;

    private void Awake()
    {
        if (lineMask != null) maskStartSize = new Vector2(0, lineMask.sizeDelta.y);
        if (rotateIconRect != null) iconStartRotation = rotateIconRect.localRotation;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InteractionEvents.OnRotatePerformed += HandleRotatePerformed;
        HideUIElements();
        StartTutorialSequence(true); 
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InteractionEvents.OnRotatePerformed -= HandleRotatePerformed;
    }

    private void HandleRotatePerformed(Vector2 delta) => CompleteTutorial();

    protected override void SetupUIAndPlayAnimation()
    {
        if (mouseRect != null) mouseRect.gameObject.SetActive(true);
        if (rotateIconRect != null) rotateIconRect.gameObject.SetActive(true);
        if (lineMask != null) lineMask.gameObject.SetActive(true);

        if (overlayController != null && assemblyService != null) 
            overlayController.ShowOverlay(assemblyService.GetInspectPoint());

        if (assemblyService != null && mainCam != null)
        {
            Vector2 screenPoint = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
            RectTransform parentRect = (RectTransform)lineMask.parent;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, screenPoint, null, out Vector2 localPos);

            cachedCenterPos = localPos;
            
            if (rotateIconRect != null) rotateIconRect.anchoredPosition = localPos + rotateIconOffset;
        }

        PlayRotateAnimation();
    }

    protected override void HideUIElements()
    {
        if (mouseRect != null) mouseRect.gameObject.SetActive(false);
        if (rotateIconRect != null) rotateIconRect.gameObject.SetActive(false);
        if (lineMask != null) lineMask.gameObject.SetActive(false);
    }

    protected override void KillSequence() => rotateSequence?.Kill();

    private void PlayRotateAnimation()
    {
        KillSequence();
        
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (clickIndicator != null) clickIndicator.SetActive(false);
        if (lineMask != null) lineMask.sizeDelta = maskStartSize;
        if (rotateIconRect != null) rotateIconRect.localRotation = iconStartRotation;

        // PERFECT CENTER MATH: Starts exactly half the width to the left of the artifact
        Vector2 lineStartPos = cachedCenterPos + new Vector2(-lineMaxWidth / 2f, 0);
        
        Vector2 mouseStartPos = cachedCenterPos + new Vector2(-travelDistance, mouseOffset.y);
        Vector2 mouseEndPos = cachedCenterPos + new Vector2(travelDistance, mouseOffset.y);
        
        mouseRect.anchoredPosition = mouseStartPos;
        if (lineMask != null) lineMask.anchoredPosition = lineStartPos; 

        rotateSequence = DOTween.Sequence();

        // Pause before doing anything so the user registers the default mouse state
        rotateSequence.AppendInterval(0.5f);

        // 1. PRESS DOWN
        rotateSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseRightClickSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });
        
        // Slightly longer pause so the click is clearly visible before it starts moving
        rotateSequence.AppendInterval(0.4f); 

        // 2. START DRAGGING
        rotateSequence.AppendCallback(() => {
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        rotateSequence.Append(mouseRect.DOAnchorPos(mouseEndPos, moveDuration).SetEase(Ease.InOutSine)); 
        
        if (lineMask != null)
        {
            rotateSequence.Join(lineMask.DOSizeDelta(new Vector2(lineMaxWidth, maskStartSize.y), moveDuration).SetEase(Ease.InOutSine));
        }

        if (rotateIconRect != null)
        {
            rotateSequence.Join(rotateIconRect.DORotate(new Vector3(0, 0, -180f), moveDuration, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.InOutSine));
        }

        // 3. RELEASE
        rotateSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });

        rotateSequence.AppendInterval(0.2f);

        // 4. CLEAN UP
        rotateSequence.AppendCallback(() => {
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        if (lineMask != null) rotateSequence.Append(lineMask.DOSizeDelta(maskStartSize, 0.2f));

        rotateSequence.AppendInterval(0.5f);
        rotateSequence.OnComplete(OnSequenceLoopComplete);
    }
}