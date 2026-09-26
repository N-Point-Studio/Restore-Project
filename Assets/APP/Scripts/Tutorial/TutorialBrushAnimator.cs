using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialBrushAnimator : TutorialAnimatorBase
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
    [Tooltip("Drag the actual 3D Brush GameObject from your scene here")]
    [SerializeField] private Transform brush3DTransform;
    [Tooltip("Adjust this to nudge the target dot (0,0 is dead center)")]
    [SerializeField] private Vector2 targetDirtOffset = new Vector2(-40, 0);
    
    [Header("UI Elements")]
    [Tooltip("Pivot MUST be X:0, Y:0.5 for the stretch to work!")]
    [SerializeField] private RectTransform dottedLine; 
    [SerializeField] private RectTransform targetCircleDot; 
    
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1.2f;

    private ToolService toolService;
    private Sequence brushSequence;
    private float defaultLineHeight;
    private Vector2 cachedStartPos;
    private Vector2 cachedEndPos;

    // Inject the ToolService so we can force the player to drop the Chisel
    [Inject]
    public void ConstructChild(ToolService toolService)
    {
        this.toolService = toolService;
    }

    private void Awake()
    {
        if (dottedLine != null) defaultLineHeight = dottedLine.sizeDelta.y;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (toolService != null)
        {
            toolService.ForceReturnTool();
        }

        HideUIElements();
        StartTutorialSequence(true); 
    }

    protected override void SetupUIAndPlayAnimation()
    {
        if (uiCursorRect != null) uiCursorRect.gameObject.SetActive(true);
        if (dottedLine != null) dottedLine.gameObject.SetActive(true);
        
        if (overlayController != null)
        {
            if (assemblyService != null) 
                overlayController.ShowOverlay(assemblyService.GetInspectPoint());
                
            if (brush3DTransform != null)
                overlayController.ShowOverlay(brush3DTransform);
        }

        // Force the Brush outline to turn on
        TutorialService.OnTutorialHighlightOn?.Invoke(ToolType.Brush);

        if (mainCam != null && dottedLine != null)
        {
            RectTransform parentRect = (RectTransform)dottedLine.parent;

            if (brush3DTransform != null)
            {
                Vector2 brushScreen = mainCam.WorldToScreenPoint(brush3DTransform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, brushScreen, null, out cachedStartPos);
            }

            if (assemblyService != null)
            {
                Vector2 coinScreen = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, coinScreen, null, out cachedEndPos);
                
                cachedEndPos += targetDirtOffset; 
            }
        }

        if (targetCircleDot != null)
        {
            targetCircleDot.gameObject.SetActive(true);
            targetCircleDot.anchoredPosition = cachedEndPos;
            targetCircleDot.localScale = Vector3.zero;
        }

        PlayBrushAnimation();
    }

    protected override void HideUIElements()
    {
        if (uiCursorRect != null) uiCursorRect.gameObject.SetActive(false);
        if (dottedLine != null) dottedLine.gameObject.SetActive(false);
        if (clickIndicator != null) clickIndicator.SetActive(false);
        if (targetCircleDot != null) targetCircleDot.gameObject.SetActive(false);
        
        // Clean up the highlighted Coin and Brush!
        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence() => brushSequence?.Kill();

    private void PlayBrushAnimation()
    {
        KillSequence();

        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (clickIndicator != null) clickIndicator.SetActive(false);
        if (targetCircleDot != null) targetCircleDot.localScale = Vector3.zero;
        if (uiCursorImage != null) uiCursorImage.sprite = cursorDefaultSprite;
        
        uiCursorRect.anchoredPosition = cachedStartPos;
        
        if (dottedLine != null)
        {
            dottedLine.anchoredPosition = cachedStartPos;
            dottedLine.sizeDelta = new Vector2(0, defaultLineHeight);
            
            Vector2 direction = cachedEndPos - cachedStartPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedLine.localRotation = Quaternion.Euler(0, 0, angle);
        }

        brushSequence = DOTween.Sequence();
        brushSequence.AppendInterval(0.3f);

        // 1. CHANGE TO POINT CURSOR
        brushSequence.AppendCallback(() => {
            if (uiCursorImage != null) uiCursorImage.sprite = cursorPointSprite;
        });
        
        brushSequence.AppendInterval(0.3f);

        // 2. CLICK AND GRAB THE BRUSH
        brushSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (uiCursorImage != null) uiCursorImage.sprite = cursorGrabSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });
        
        brushSequence.AppendInterval(0.2f);

        // 3. SHOW THE TARGET DOT
        if (targetCircleDot != null)
        {
            brushSequence.Append(targetCircleDot.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        }

        // 4. START MOVE
        brushSequence.AppendCallback(() => {
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        brushSequence.Append(uiCursorRect.DOAnchorPos(cachedEndPos, moveDuration).SetEase(Ease.InOutSine));
        
        if (dottedLine != null)
        {
            float totalDistance = Vector2.Distance(cachedStartPos, cachedEndPos);
            brushSequence.Join(dottedLine.DOSizeDelta(new Vector2(totalDistance, defaultLineHeight), moveDuration).SetEase(Ease.InOutSine));
        }

        // 5. ARRIVE AT COIN & SCRUB (Horizontal brushing motion)
        brushSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicator != null) clickIndicator.SetActive(true);
        });

        // Horizontal back-and-forth punch to simulate sweeping dust
        brushSequence.Append(uiCursorRect.DOPunchAnchorPos(new Vector2(40, 0), 0.6f, 6, 0.5f));

        brushSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (clickIndicator != null) clickIndicator.SetActive(false);
        });

        // 6. CLEANUP 
        if (targetCircleDot != null) brushSequence.Append(targetCircleDot.DOScale(Vector3.zero, 0.2f));

        brushSequence.AppendInterval(0.5f);
        brushSequence.OnComplete(OnSequenceLoopComplete);
    }
}