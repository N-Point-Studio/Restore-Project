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
        detectionService.OnInteractDetected += HandleObjectHover;
        AssembleEvents.OnAssembleFinished += HandleAssembleFinished;
        
        HideUIElements();
        FindPieces();
        
        if (sourcePiece != null && targetPiece != null && !isTaskCompleted)
        {
            // Block input as per the rule document
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
        CompleteTutorial();
    }

    private void FindPieces()
    {
        if (inspection == null || fragmentService == null) return;
        
        Transform assemblyRoot = inspection.GetAssemblyRoot();

        foreach (ArtefactPieceStateMachine piece in fragmentService.GetAllPieces())
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

        // Highlight the piece still on the table so they know what to grab
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
        attachSequence = DOTween.Sequence();

        Vector2 startPos = mainCam.WorldToScreenPoint(sourcePiece.position);
        Vector2 endPos = mainCam.WorldToScreenPoint(targetPiece.position);
        
        fakeCursor.position = startPos + new Vector2(150, -150);
        if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);

        // 1. Move to Source Piece
        attachSequence.Append(fakeCursor.DOMove(startPos, 1f).SetEase(Ease.OutQuad));

        if (dottedRect != null)
        {
            dottedRect.position = startPos;
            dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, 0);
            Vector2 direction = endPos - startPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // 2. Hover
        attachSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        });
        attachSequence.AppendInterval(0.3f);

        // 3. Grab
        attachSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        attachSequence.AppendInterval(0.2f);

        // 4. Start Drag to Target Piece
        attachSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(true);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        
        attachSequence.Append(fakeCursor.DOMove(endPos, 1.5f).SetEase(Ease.InOutSine));
        
        if (dottedRect != null)
        {
            float totalDistance = Vector2.Distance(startPos, endPos);
            attachSequence.Join(dottedRect.DOSizeDelta(new Vector2(dottedRect.sizeDelta.x, totalDistance), 1.5f).SetEase(Ease.InOutSine));
        }

        // 5. Release at Target Piece
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
        
        // Loop the sequence using the base class method you used in TutorialDragAnimator
        attachSequence.OnComplete(OnSequenceLoopComplete);
    }
}