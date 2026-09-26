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

    [Header("Hold Indicator")]
    [SerializeField] private Image holdFillImage; 
    [SerializeField] private GameObject holdIndicatorRoot;

    [Header("Arrow/Line Indicator")]
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;

    private Transform targetPiece;
    private Sequence detachSequence;
    private FragmentService fragmentService;
    private Inspection inspection;

    [Inject]
    public void ConstructChild(FragmentService fragmentService, Inspection inspection)
    {
        this.fragmentService = fragmentService;
        this.inspection = inspection;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        mainCam = Camera.main;
        
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

        if (targetPiece == null) 
        {
            CompleteTutorial();
        }
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
        if (targetPiece == null || mainCam == null) return;

        detachSequence = DOTween.Sequence();

        // 1. Reset states and calculate positions dynamically at the start of each loop
        detachSequence.AppendCallback(() => {
            if (tutorialCanvasGroup != null) tutorialCanvasGroup.alpha = 0f;
            
            Vector2 targetPos = mainCam.WorldToScreenPoint(targetPiece.position);
            fakeCursor.position = targetPos + new Vector2(100, -100);
            
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            
            if (holdFillImage != null) holdFillImage.fillAmount = 0f;
            if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
            
            if (arrowRect != null) 
            {
                arrowRect.gameObject.SetActive(false);
                arrowRect.sizeDelta = new Vector2(0, arrowRect.sizeDelta.y);
            }
        });

        // 2. Fade in and move cursor to the piece
        if (tutorialCanvasGroup != null) detachSequence.Append(tutorialCanvasGroup.DOFade(1f, 0.3f));
        
        detachSequence.Append(DOVirtual.Float(0f, 1f, 0.8f, (val) => {
            Vector2 targetPos = mainCam.WorldToScreenPoint(targetPiece.position);
            Vector2 startCursorPos = targetPos + new Vector2(100, -100);
            fakeCursor.position = Vector2.Lerp(startCursorPos, targetPos, val);
        }).SetEase(Ease.OutQuad));

        // 3. Press and Hold
        detachSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            
            if (holdIndicatorRoot != null)
            {
                holdIndicatorRoot.SetActive(true);
                holdIndicatorRoot.transform.position = mainCam.WorldToScreenPoint(targetPiece.position);
            }
        });

        // 4. Animate the circular fill over 1.5 seconds
        if (holdFillImage != null)
        {
            detachSequence.Append(DOVirtual.Float(0f, 1f, 1.5f, (val) => {
                holdFillImage.fillAmount = val;
                
                // Keep it anchored to the piece in case the camera is moving
                if (holdIndicatorRoot != null) 
                {
                    holdIndicatorRoot.transform.position = mainCam.WorldToScreenPoint(targetPiece.position);
                }
            }).SetEase(Ease.Linear));
        }
        else
        {
            detachSequence.AppendInterval(1.5f);
        }

        // 5. Hide hold ring, show dynamic arrow pointing to the tray
        detachSequence.AppendCallback(() => {
            if (holdIndicatorRoot != null) holdIndicatorRoot.SetActive(false);
            
            if (arrowRect != null)
            {
                Vector2 targetPos = mainCam.WorldToScreenPoint(targetPiece.position);
                
                // Use the real tray transform if assigned, otherwise fallback to an offset
                Vector2 trayPos = artefactTray != null 
                    ? (Vector2)mainCam.WorldToScreenPoint(artefactTray.position) 
                    : targetPos + new Vector2(-200, 200);

                arrowRect.gameObject.SetActive(true);
                arrowRect.position = targetPos;
                
                Vector2 direction = trayPos - targetPos;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrowRect.rotation = Quaternion.Euler(0, 0, angle);
                
                // Tween the stretch instantly in the callback so it renders correctly next frame
                arrowRect.DOSizeDelta(new Vector2(direction.magnitude, arrowRect.sizeDelta.y), 0.5f).SetEase(Ease.OutBack);
            }
        });

        // 6. Give the arrow stretch time to finish, then briefly pause
        detachSequence.AppendInterval(1.0f);
        
        // 7. Fade out
        if (tutorialCanvasGroup != null) detachSequence.Append(tutorialCanvasGroup.DOFade(0f, 0.3f));
        detachSequence.AppendInterval(0.5f);

        detachSequence.SetLoops(-1).SetUpdate(true);
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