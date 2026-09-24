using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialChiselAnimator : TutorialAnimatorBase
{
    [Header("UI Hand Cursor")]
    [SerializeField] private RectTransform uiCursorRect; 
    [SerializeField] private Image uiCursorImage;
    [SerializeField] private Sprite cursorDefaultSprite;
    [SerializeField] private Sprite cursorPointSprite;
    [SerializeField] private Sprite cursorGrabSprite; 
    
    [Header("Physical Mouse Indicator")]
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseLeftClickSprite;
    [SerializeField] private GameObject clickIndicator; 

    [Header("3D Targets (Dynamic Positioning)")]
    [SerializeField] private Transform chisel3DTransform;
    [Tooltip("Moves the dot to the left! (-40 is slightly left of center)")]
    [SerializeField] private Vector2 targetDirtOffset = new Vector2(-40, 0);
    
    [Header("UI Elements")]
    [Tooltip("Pivot MUST be X:0, Y:0.5 for the stretch to work!")]
    [SerializeField] private RectTransform dottedLine; 
    [SerializeField] private RectTransform targetCircleDot; 
    
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1.2f;

    private Sequence chiselSequence;
    private float defaultLineHeight;
    private Vector2 cachedStartPos;
    private Vector2 cachedEndPos;

    private void Awake()
    {
        if (dottedLine != null) defaultLineHeight = dottedLine.sizeDelta.y;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        HideUIElements();
        StartTutorialSequence(true); 
    }

    protected override void SetupUIAndPlayAnimation()
    {
        if (uiCursorRect != null) uiCursorRect.gameObject.SetActive(true);
        if (dottedLine != null) dottedLine.gameObject.SetActive(true);
        
        if (overlayController != null && assemblyService != null) 
            overlayController.ShowOverlay(assemblyService.GetInspectPoint());

        if (mainCam != null && dottedLine != null)
        {
            RectTransform parentRect = (RectTransform)dottedLine.parent;

            if (chisel3DTransform != null)
            {
                Vector2 chiselScreen = mainCam.WorldToScreenPoint(chisel3DTransform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, chiselScreen, null, out cachedStartPos);
            }

            if (assemblyService != null)
            {
                Vector2 coinScreen = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, coinScreen, null, out cachedEndPos);
                
                // Applies the left offset automatically!
                cachedEndPos += targetDirtOffset; 
            }
        }

        if (targetCircleDot != null)
        {
            targetCircleDot.gameObject.SetActive(true);
            targetCircleDot.anchoredPosition = cachedEndPos;
            targetCircleDot.localScale = Vector3.zero;
        }

        PlayChiselAnimation();
    }

    protected override void HideUIElements()
    {
        if (uiCursorRect != null) uiCursorRect.gameObject.SetActive(false);
        if (dottedLine != null) dottedLine.gameObject.SetActive(false);
        if (clickIndicator != null) clickIndicator.SetActive(false);
        if (targetCircleDot != null) targetCircleDot.gameObject.SetActive(false);
    }

    protected override void KillSequence() => chiselSequence?.Kill();

    private void PlayChiselAnimation()
    {
        KillSequence();

        // Setup Initial State
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (clickIndicator != null) clickIndicator.SetActive(false);
        if (targetCircleDot != null) targetCircleDot.localScale = Vector3.zero;
        if (uiCursorImage != null) uiCursorImage.sprite = cursorDefaultSprite;
        
        uiCursorRect.anchoredPosition = cachedStartPos;
        
        if (dottedLine != null)
        {
            // Instantly reset line size at the very start of the loop
            dottedLine.anchoredPosition = cachedStartPos;
            dottedLine.sizeDelta = new Vector2(0, defaultLineHeight);
            
            Vector2 direction = cachedEndPos - cachedStartPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedLine.localRotation = Quaternion.Euler(0, 0, angle);
        }

        chiselSequence = DOTween.Sequence();
        chiselSequence.AppendInterval(0.3f);

        // 1. CHANGE TO POINT CURSOR
        chiselSequence.AppendCallback(() => {
            if (uiCursorImage != null) uiCursorImage.sprite = cursorPointSprite;
        });
        
        chiselSequence.AppendInterval(0.3f);

        // 2. CLICK AND GRAB THE CHISEL
        chiselSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (uiCursorImage != null) uiCursorImage.sprite = cursorGrabSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });
        
        chiselSequence.AppendInterval(0.2f);

        // 3. SHOW THE TARGET DOT
        if (targetCircleDot != null)
        {
            chiselSequence.Append(targetCircleDot.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        }

        // 4. START MOVE
        chiselSequence.AppendCallback(() => {
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        chiselSequence.Append(uiCursorRect.DOAnchorPos(cachedEndPos, moveDuration).SetEase(Ease.InOutSine));
        
        if (dottedLine != null)
        {
            float totalDistance = Vector2.Distance(cachedStartPos, cachedEndPos);
            chiselSequence.Join(dottedLine.DOSizeDelta(new Vector2(totalDistance, defaultLineHeight), moveDuration).SetEase(Ease.InOutSine));
        }

        // 5. ARRIVE AT COIN & TAP
        chiselSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });

        chiselSequence.Append(uiCursorRect.DOPunchAnchorPos(new Vector2(15, -15), 0.35f, 15, 1f));

        chiselSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        // 6. CLEANUP 
        // Notice we do NOT shrink the dotted line anymore! It stays stretched until the loop restarts.
        if (targetCircleDot != null) chiselSequence.Append(targetCircleDot.DOScale(Vector3.zero, 0.2f));

        chiselSequence.AppendInterval(0.5f);
        chiselSequence.OnComplete(OnSequenceLoopComplete);
    }
}