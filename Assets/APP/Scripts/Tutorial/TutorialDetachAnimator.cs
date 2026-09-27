using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialDetachAnimator : TutorialAnimatorBase
{
    [Header("World Targets")]
    [SerializeField] private Transform artefactTray;

    [Header("Fake Hand Cursor")]
    [SerializeField] private RectTransform fakeCursor;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite hoverCursorSprite;
    [SerializeField] private Sprite grabCursorSprite;

    [Header("Physical Mouse Indicator")]
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseLeftClickSprite;
    [SerializeField] private GameObject clickIndicatorObject;

    [Header("Hold Indicator")]
    [SerializeField] private Image holdFillImage; 
    [SerializeField] private GameObject holdIndicatorRoot;

    [Header("Arrow/Line Indicator")]
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;

    private Transform targetPiece;
    private Sequence detachSequence;
    private ObjectDetectionService detectionService;
    private FragmentService fragmentService;
    private Inspection inspection;
    
    // Canvas fields for safe UI positioning
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    [Inject]
    public void ConstructChild(ObjectDetectionService detectionService, FragmentService fragmentService, Inspection inspection)
    {
        this.detectionService = detectionService;
        this.fragmentService = fragmentService;
        this.inspection = inspection;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        // Cache the canvas components for coordinate conversion
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
        
        InteractionEvents.OnHoldCompleted += HandleHoldCompleted;
        
        HideUIElements();
        FindAttachedPiece();
        
        if (targetPiece != null && !isTaskCompleted)
        {
            StartTutorialSequence(true); 
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InteractionEvents.OnHoldCompleted -= HandleHoldCompleted;
    }

    private void HandleHoldCompleted(IInteractObject interactable, Vector2 position)
    {
        CompleteTutorial();
    }

    private void FindAttachedPiece()
    {
        if (inspection == null || fragmentService == null) return;
        
        Transform assemblyRoot = inspection.GetAssemblyRoot();

        foreach (var piece in fragmentService.GetAllPieces())
        {
            if (piece.transform.parent == assemblyRoot)
            {
                targetPiece = piece.transform;
                break;
            }
        }

        if (targetPiece == null) CompleteTutorial();
    }

    protected override void SetupUIAndPlayAnimation()
    {
        fakeCursor.gameObject.SetActive(true);
        cursorImage.sprite = hoverCursorSprite;
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        
        if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
        if (arrowRect != null) arrowRect.gameObject.SetActive(false);

        if (overlayController != null && targetPiece != null)
        {
            overlayController.ShowOverlay(targetPiece);
        }

        PlayDetachAnimation();
    }

    private void PlayDetachAnimation()
    {
        KillSequence();
        if (targetPiece == null || mainCam == null || parentCanvas == null) return;

        detachSequence = DOTween.Sequence();
        RectTransform holdRect = holdIndicatorRoot != null ? holdIndicatorRoot.GetComponent<RectTransform>() : null;

        // 1. Reset states
        detachSequence.AppendCallback(() => {
            if (tutorialCanvasGroup != null) tutorialCanvasGroup.alpha = 0f;
            
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            
            if (holdFillImage != null) holdFillImage.fillAmount = 0f;
            if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
            
            if (arrowRect != null) 
            {
                arrowRect.gameObject.SetActive(false);
                // Reset the Y size (length) to 0
                arrowRect.sizeDelta = new Vector2(arrowRect.sizeDelta.x, 0); 
            }
        });

        // 2. Fade in
        if (tutorialCanvasGroup != null) detachSequence.Append(tutorialCanvasGroup.DOFade(1f, 0.3f));
        
        // Dynamically move cursor using safe Canvas Local coordinates
        detachSequence.Append(DOVirtual.Float(0f, 1f, 0.8f, (val) => {
            if (targetPiece == null) return;
            Vector2 targetScreenPos = mainCam.WorldToScreenPoint(targetPiece.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreenPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam, out Vector2 targetLocalPos);
            
            Vector2 startCursorPos = targetLocalPos + new Vector2(100, -100);
            
            // Assign to anchoredPosition instead of position!
            fakeCursor.anchoredPosition = Vector2.Lerp(startCursorPos, targetLocalPos, val);
        }).SetEase(Ease.OutQuad));

        // 3. Press and Hold
        // 3. Press and Hold (Trigger the click flash)
        detachSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(true);
            
            // Turn ON the click flash
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });

        // Wait just 0.1 seconds so the click flash is visible to the human eye
        detachSequence.AppendInterval(0.1f);

        // Turn OFF the click flash right before the ring starts filling
        detachSequence.AppendCallback(() => {
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });

        // 4. Animate the circular fill over 1.5 seconds
        if (holdFillImage != null)
        {
            detachSequence.Append(DOVirtual.Float(0f, 1f, 1.5f, (val) => {
                holdFillImage.fillAmount = val;
                
                if (holdRect != null && targetPiece != null) 
                {
                    Vector2 targetScreen = mainCam.WorldToScreenPoint(targetPiece.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreen, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam, out Vector2 localPos);
                    holdRect.anchoredPosition = localPos;
                }
            }).SetEase(Ease.Linear));
        }

        // 5. Hide hold ring, show dynamically rotating and stretching arrow
        detachSequence.AppendCallback(() => {
            if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
            
            if (arrowRect != null && targetPiece != null)
            {
                Vector2 targetScreen = mainCam.WorldToScreenPoint(targetPiece.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreen, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam, out Vector2 targetLocal);
                
                Vector2 trayLocal = targetLocal + new Vector2(-200, 200);
                if (artefactTray != null) {
                    Vector2 trayScreen = mainCam.WorldToScreenPoint(artefactTray.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, trayScreen, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam, out trayLocal);
                }

                arrowRect.gameObject.SetActive(true);
                arrowRect.anchoredPosition = targetLocal;
                
                Vector2 direction = trayLocal - targetLocal;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                // Subtract 90 degrees so the Up-pointing sprite aligns correctly
                arrowRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                
                // Stretch the Y-axis (height) instead of X-axis
                arrowRect.DOSizeDelta(new Vector2(arrowRect.sizeDelta.x, direction.magnitude), 0.5f).SetEase(Ease.OutBack);
            }
        });

        // 6. Generous Pause before looping so it looks like a clear instruction
        detachSequence.AppendInterval(2.0f);
        
        // 7. Fade out
        if (tutorialCanvasGroup != null) detachSequence.Append(tutorialCanvasGroup.DOFade(0f, 0.3f));
        detachSequence.AppendInterval(0.5f);

        detachSequence.OnComplete(OnSequenceLoopComplete);
    }

    protected override void HideUIElements()
    {
        if (fakeCursor != null) fakeCursor.gameObject.SetActive(false);
        if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
        if (arrowRect != null) arrowRect.gameObject.SetActive(false);
        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence() => detachSequence?.Kill();
}