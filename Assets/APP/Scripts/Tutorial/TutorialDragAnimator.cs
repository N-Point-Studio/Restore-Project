using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;
using System.Collections; // Needed for Coroutines

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

    private RectTransform canvasRect;
    private Canvas parentCanvas;
    
    private bool isAdvancing = false;

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
        mainCam = Camera.main;

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();

        detectionService.OnInteractDetected += HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated += HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed += HandleAssemblyPerformed;
        
        HideUIElements();
        isAdvancing = false;
        FindExistingArtefact();
        
        if (artefactStartTarget != null && !isTaskCompleted && !isAdvancing)
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

    // --- NEW: Safe Advancement Logic ---
    private void TriggerSafeAdvancement()
    {
        if (isAdvancing || isTaskCompleted) return;
        isAdvancing = true;
        StartCoroutine(AdvanceWhenReadyRoutine());
    }

    private IEnumerator AdvanceWhenReadyRoutine()
    {
        // Wait patiently if the TutorialService is currently locked/loading
        while (tutorialService != null && tutorialService.IsProcessing)
        {
            yield return null;
        }

        // Now that it's ready, trigger the advancement!
        if (tutorialService != null) 
        {
            tutorialService.CompleteAndAdvance(true);
        }
        CompleteTutorial();
    }
    // ------------------------------------

    private void FindExistingArtefact()
    {
        var targetPiece = fragmentService.GetFirstAvailablePiece();
        if (targetPiece != null) 
        {
            artefactStartTarget = targetPiece.transform;
            
            float distance = Vector3.Distance(artefactStartTarget.position, assemblyService.GetInspectPoint().position);
            if (distance <= 0.1f) 
            {
                TriggerSafeAdvancement();
            }
        }
    }

    private void HandlePieceSpawned(ArtefactPieceStateMachine piece)
    {
        if (artefactStartTarget == null)
        {
            artefactStartTarget = piece.transform;

            float distance = Vector3.Distance(artefactStartTarget.position, assemblyService.GetInspectPoint().position);
            if (distance <= 0.1f) 
            {
                TriggerSafeAdvancement();
                return;
            }

            if (gameObject.activeInHierarchy && !isAnimationPlaying && !isTaskCompleted && isFirstPhase && !isAdvancing)
            {
                StartTutorialSequence(true);
            }
        }
    }

    private void HandleObjectHover(IInteractObject interactable)
    {
        if (interactable is IArtefactPart) RecordActivity();
    }

    private void HandleAssemblyPerformed()
    {
        TriggerSafeAdvancement();
    }

    public override void StartTutorialSequence(bool blockInput)
    {
        if (artefactStartTarget == null || isTaskCompleted || isAdvancing) return;
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
        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence() => dragSequence?.Kill();

    private void PlayDragAnimation()
    {
        KillSequence();
        if (artefactStartTarget == null || mainCam == null || parentCanvas == null) return;

        dragSequence = DOTween.Sequence();

        Camera uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam;
        
        Vector2 startScreen = mainCam.WorldToScreenPoint(artefactStartTarget.position);
        Vector2 endScreen = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, startScreen, uiCam, out Vector2 startLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, endScreen, uiCam, out Vector2 endLocal);

        fakeCursor.anchoredPosition = startLocal + new Vector2(150, -150);
        if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);

        dragSequence.Append(fakeCursor.DOAnchorPos(startLocal, 1f).SetEase(Ease.OutQuad));

        if (dottedRect != null)
        {
            dottedRect.anchoredPosition = startLocal;
            dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, 0);
            
            Vector2 direction = endLocal - startLocal;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        dragSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        });
        dragSequence.AppendInterval(0.3f);

        dragSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        dragSequence.AppendInterval(0.2f);

        dragSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(true);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });

        dragSequence.Append(DOVirtual.Float(0f, 1f, 1.5f, (val) => {
            if (artefactStartTarget == null) return;
            
            Vector2 currentStartScreen = mainCam.WorldToScreenPoint(artefactStartTarget.position);
            Vector2 currentEndScreen = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, currentStartScreen, uiCam, out Vector2 dynamicStartLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, currentEndScreen, uiCam, out Vector2 dynamicEndLocal);

            fakeCursor.anchoredPosition = Vector2.Lerp(dynamicStartLocal, dynamicEndLocal, val);

            if (dottedRect != null)
            {
                dottedRect.anchoredPosition = dynamicStartLocal;
                
                Vector2 direction = dynamicEndLocal - dynamicStartLocal;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                dottedRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                
                float currentDistance = direction.magnitude * val;
                dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, currentDistance);
            }
        }).SetEase(Ease.InOutSine));

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