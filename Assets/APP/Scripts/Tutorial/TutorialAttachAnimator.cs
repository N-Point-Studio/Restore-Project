using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialAttachAnimator : TutorialAnimatorBase
{
    [Header("Fake Hand Cursor")]
    [SerializeField] private RectTransform fakeCursor;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite defaultCursorSprite;
    [SerializeField] private Sprite hoverCursorSprite;
    [SerializeField] private Sprite grabCursorSprite;

    [Header("Physical Mouse Indicator")]
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseLeftClickSprite;
    [SerializeField] private GameObject clickIndicatorObject;

    [Header("Dotted Line")]
    [SerializeField] private GameObject dottedGameObject;
    private RectTransform dottedRect;

    private Transform sourcePiece;
    private Transform targetPiece;
    private Sequence attachSequence;
    private ObjectDetectionService detectionService;
    private FragmentService fragmentService;
    private Inspection inspection;
    
    // Added for safe UI positioning
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    [Inject]
    public void ConstructChild(ObjectDetectionService detectionService, FragmentService fragmentService, Inspection inspection)
    {
        this.detectionService = detectionService;
        this.fragmentService = fragmentService;
        this.inspection = inspection;
    }

    private void Awake()
    {
        if (dottedGameObject != null) dottedRect = dottedGameObject.GetComponent<RectTransform>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        mainCam = Camera.main;
        
        // Cache canvas components
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
        
        detectionService.OnInteractDetected += HandleObjectHover;
        AssembleEvents.OnAssembleFinished += HandleAssembleFinished;
        
        HideUIElements();
        FindPieces();
        
        if (sourcePiece != null && targetPiece != null && !isTaskCompleted)
        {
            StartTutorialSequence(true); 
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        detectionService.OnInteractDetected -= HandleObjectHover;
        AssembleEvents.OnAssembleFinished -= HandleAssembleFinished;
    }

    private void HandleObjectHover(IInteractObject interactable)
    {
        if (interactable is IArtefactPart) RecordActivity();
    }

    private void HandleAssembleFinished()
    {
        if (assemblyService != null && assemblyService.TotalCurrentParts() > 1)
        {
            CompleteTutorial();
        }
    }

    private void FindPieces()
    {
        if (inspection == null || fragmentService == null) return;
        
        Transform assemblyRoot = inspection.GetAssemblyRoot();

        foreach (var piece in fragmentService.GetAllPieces())
        {
            if (piece.transform.parent == assemblyRoot)
            {
                targetPiece = piece.transform;
            }
            else
            {
                sourcePiece = piece.transform;
            }
        }

        if (sourcePiece == null) 
        {
            CompleteTutorial();
        }
    }

    public override void StartTutorialSequence(bool blockInput)
    {
        if (sourcePiece == null || targetPiece == null || isTaskCompleted) return;
        base.StartTutorialSequence(blockInput);
    }

    protected override void SetupUIAndPlayAnimation()
    {
        fakeCursor.gameObject.SetActive(true);
        cursorImage.sprite = defaultCursorSprite;
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (dottedGameObject != null) dottedGameObject.SetActive(false);

        if (overlayController != null) overlayController.ShowOverlay(sourcePiece);

        PlayAttachAnimation();
    }

    protected override void HideUIElements()
    {
        if (fakeCursor != null) fakeCursor.gameObject.SetActive(false);
        if (dottedGameObject != null) dottedGameObject.SetActive(false);
        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence() => attachSequence?.Kill();

    private void PlayAttachAnimation()
    {
        KillSequence();
        if (sourcePiece == null || targetPiece == null || mainCam == null || parentCanvas == null) return;

        attachSequence = DOTween.Sequence();

        // 1. Convert Screen Pixels to safe Canvas Coordinates
        Vector2 startScreen = mainCam.WorldToScreenPoint(sourcePiece.position);
        Vector2 endScreen = mainCam.WorldToScreenPoint(targetPiece.position);
        
        Camera uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, startScreen, uiCam, out Vector2 startLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, endScreen, uiCam, out Vector2 endLocal);

        // Use anchoredPosition instead of position!
        fakeCursor.anchoredPosition = startLocal + new Vector2(150, -150);
        if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);

        // 2. Move to Source Piece (using DOAnchorPos)
        attachSequence.Append(fakeCursor.DOAnchorPos(startLocal, 1f).SetEase(Ease.OutQuad));

        if (dottedRect != null)
        {
            dottedRect.anchoredPosition = startLocal;
            dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, 0);
            
            // Calculate distance and rotation based on CANVAS coordinates, not screen pixels
            Vector2 direction = endLocal - startLocal;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // 3. Hover
        attachSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        });
        attachSequence.AppendInterval(0.3f);

        // 4. Grab
        attachSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        attachSequence.AppendInterval(0.2f);

        // 5. Start Drag to Target Piece
        attachSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(true);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        
        attachSequence.Append(DOVirtual.Float(0f, 1f, 1.5f, (val) => {
            if (sourcePiece == null || targetPiece == null) return;
            
            Camera uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam;
            
            // Recalculate safe local positions every single frame
            Vector2 currentStartScreen = mainCam.WorldToScreenPoint(sourcePiece.position);
            Vector2 currentEndScreen = mainCam.WorldToScreenPoint(targetPiece.position);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, currentStartScreen, uiCam, out Vector2 dynamicStartLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, currentEndScreen, uiCam, out Vector2 dynamicEndLocal);

            // Move the hand
            fakeCursor.anchoredPosition = Vector2.Lerp(dynamicStartLocal, dynamicEndLocal, val);

            // Stretch and rotate the line dynamically
            if (dottedRect != null)
            {
                dottedRect.anchoredPosition = dynamicStartLocal;
                
                Vector2 direction = dynamicEndLocal - dynamicStartLocal;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                dottedRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                
                // Multiply total distance by 'val' so it visually grows alongside the hand
                float currentDistance = direction.magnitude * val;
                dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, currentDistance);
            }
        }).SetEase(Ease.InOutSine));

        // 6. Release at Target Piece
        attachSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(false);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });

        attachSequence.AppendInterval(0.2f);
        attachSequence.AppendCallback(() => {
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        
        attachSequence.AppendInterval(0.3f);
        
        attachSequence.OnComplete(OnSequenceLoopComplete);
    }
}