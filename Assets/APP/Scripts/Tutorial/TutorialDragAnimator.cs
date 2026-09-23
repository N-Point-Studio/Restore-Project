using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialDragAnimator : TutorialAnimatorBase
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

    private Transform artefactStartTarget;
    private Sequence dragSequence;
    private ObjectDetectionService detectionService;
    private FragmentService fragmentService;

    [Inject]
    public void ConstructChild(ObjectDetectionService detectionService, FragmentService fragmentService)
    {
        this.detectionService = detectionService;
        this.fragmentService = fragmentService;
    }

    private void Awake()
    {
        if (dottedGameObject != null) dottedRect = dottedGameObject.GetComponent<RectTransform>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        detectionService.OnInteractDetected += HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated += HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed += HandleAssemblyPerformed;

        HideUIElements();
        FindExistingArtefact();

        if (artefactStartTarget != null)
        {
            StartTutorialSequence(true);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        detectionService.OnInteractDetected -= HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated -= HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed -= HandleAssemblyPerformed;
    }

    protected override void Update()
    {
        if (artefactStartTarget == null) return;
        base.Update();
    }

    private void FindExistingArtefact()
    {
        var targetPiece = fragmentService.GetFirstAvailablePiece();
        if (targetPiece != null) artefactStartTarget = targetPiece.transform;
    }

    private void HandlePieceSpawned(ArtefactPieceStateMachine piece)
    {
        if (artefactStartTarget == null)
        {
            artefactStartTarget = piece.transform;
            if (gameObject.activeInHierarchy && !isAnimationPlaying && !isTaskCompleted && isFirstPhase)
            {
                StartTutorialSequence(true);
            }
        }
    }

    private void HandleObjectHover(IInteractObject interactable)
    {
        if (interactable is IArtefactPart) RecordActivity();
    }

    private void HandleAssemblyPerformed() => CompleteTutorial();

    public override void StartTutorialSequence(bool blockInput)
    {
        if (artefactStartTarget == null || isTaskCompleted) return;
        base.StartTutorialSequence(blockInput);
    }

    protected override void SetupUIAndPlayAnimation()
    {
        fakeCursor.gameObject.SetActive(true);
        cursorImage.sprite = defaultCursorSprite;
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (dottedGameObject != null) dottedGameObject.SetActive(false);

        if (overlayController != null) overlayController.ShowOverlay(artefactStartTarget);

        PlayDragAnimation();
    }

    protected override void HideUIElements()
    {
        if (fakeCursor != null) fakeCursor.gameObject.SetActive(false);
        if (dottedGameObject != null) dottedGameObject.SetActive(false);
    }

    protected override void KillSequence() => dragSequence?.Kill();

    private void PlayDragAnimation()
    {
        KillSequence();
        dragSequence = DOTween.Sequence();

        Vector2 startPos = mainCam.WorldToScreenPoint(artefactStartTarget.position);
        Vector2 endPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
        fakeCursor.position = startPos + new Vector2(150, -150);

        if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);

        // 1. Move to Artefact
        dragSequence.Append(fakeCursor.DOMove(startPos, 1f).SetEase(Ease.OutQuad));

        if (dottedRect != null)
        {
            dottedRect.position = startPos;
            dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, 0);
            Vector2 direction = endPos - startPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // 2. Hover
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        });
        dragSequence.AppendInterval(0.3f);

        // 3. Grab
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        dragSequence.AppendInterval(0.2f);

        // 4. Start Drag
        dragSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(true);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        
        dragSequence.Append(fakeCursor.DOMove(endPos, 1.5f).SetEase(Ease.InOutSine));
        if (dottedRect != null)
        {
            float totalDistance = Vector2.Distance(startPos, endPos);
            dragSequence.Join(dottedRect.DOSizeDelta(new Vector2(dottedRect.sizeDelta.x, totalDistance), 1.5f).SetEase(Ease.InOutSine));
        }

        // 5. Release
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(false);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });

        dragSequence.AppendInterval(0.2f);
        dragSequence.AppendCallback(() => {
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        
        dragSequence.AppendInterval(0.3f);
        dragSequence.OnComplete(OnSequenceLoopComplete);
    }
}