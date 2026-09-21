using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialDragAnimator : MonoBehaviour
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
    [Tooltip("UI Image with a dotted sprite set to Tiled. Pivot should be (0, 0.5) so it stretches outward.")]
    [SerializeField] private GameObject dottedGameObject;
    [Tooltip("The impact/action lines Image that appears when clicking or releasing.")]
    private RectTransform dottedRect;

    [Header("Settings")]
    [SerializeField] private float idleTimeout = 10f; 

    private Transform artefactStartTarget; 
    private Sequence dragSequence;
    
    private int currentLoopCount = 0;
    private bool isFirstPhase = true; 
    private bool isTaskCompleted = false;
    private bool isAnimationPlaying = false;
    private float lastActivityTime;

    private ObjectDetectionService detectionService;
    private AssemblyService assemblyService;
    private TutorialService tutorialService;
    private FragmentService fragmentService;
    private TutorialOverlayUI overlayController;
    private Camera mainCam;

    [Inject]
    public void Construct(
        ObjectDetectionService detectionService, 
        AssemblyService assemblyService, 
        TutorialService tutorialService, 
        FragmentService fragmentService,
        TutorialOverlayUI overlayController,
        Camera cam)
    {
        this.detectionService = detectionService;
        this.assemblyService = assemblyService;
        this.tutorialService = tutorialService;
        this.fragmentService = fragmentService;
        this.overlayController = overlayController;
        this.mainCam = cam;
    }

    private void Awake()
    {
        if (dottedGameObject != null)
        {
            dottedRect = dottedGameObject.GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        InteractionEvents.OnMouseMoved += ResetIdleTimer;
        InteractionEvents.OnPressStart += HandleAnyInput;
        detectionService.OnInteractDetected += HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated += HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed += HandleAssemblyPerformed; 
        
        fakeCursor.gameObject.SetActive(false);
        if (dottedGameObject != null) dottedGameObject.SetActive(false);

        isFirstPhase = true;
        currentLoopCount = 0;

        FindExistingArtefact();
        
        if (artefactStartTarget != null)
        {
            StartTutorialSequence(true); 
        }
    }

    private void OnDisable()
    {
        InteractionEvents.OnMouseMoved -= ResetIdleTimer;
        InteractionEvents.OnPressStart -= HandleAnyInput;
        detectionService.OnInteractDetected -= HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated -= HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed -= HandleAssemblyPerformed;
        
        StopAnimation();
        tutorialService.SetInputBlock(false); 
    }

    private void Update()
    {
        if (isTaskCompleted || isFirstPhase || isAnimationPlaying || artefactStartTarget == null) return;

        if (Time.time - lastActivityTime >= idleTimeout)
        {
            StartTutorialSequence(false); 
        }
    }

    private void RecordActivity()
    {
        lastActivityTime = Time.time;
        if (!isFirstPhase && isAnimationPlaying) 
        {
            StopAnimation();
        }
    }

    private void ResetIdleTimer(Vector2 pos) => RecordActivity();
    private void HandleAnyInput() => RecordActivity();

    private void FindExistingArtefact()
    {
        var targetPiece = fragmentService.GetFirstAvailablePiece();
        if (targetPiece != null)
        {
            artefactStartTarget = targetPiece.transform;
        }
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
        if (interactable != null && interactable is IArtefactPart)
        {
            RecordActivity();
        }
    }

    private void HandleAssemblyPerformed()
    {
        isTaskCompleted = true;
        StopAnimation();
        tutorialService.SetInputBlock(false);
        gameObject.SetActive(false); 
    }

    public void StartTutorialSequence(bool blockInput)
    {
        if (artefactStartTarget == null || isTaskCompleted) return;

        if (blockInput) tutorialService.SetInputBlock(true); 
        
        fakeCursor.gameObject.SetActive(true);
        
        // Reset sprites to default before starting
        cursorImage.sprite = defaultCursorSprite;
        if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        if (dottedGameObject != null) dottedGameObject.SetActive(false);
        
        isAnimationPlaying = true;

        if (overlayController != null) overlayController.ShowOverlay(artefactStartTarget);

        PlayDragAnimation();
    }

    private void StopAnimation()
    {
        isAnimationPlaying = false;
        fakeCursor.gameObject.SetActive(false);
        if (dottedGameObject != null) dottedGameObject.SetActive(false);
        dragSequence?.Kill();
        
        if (overlayController != null) overlayController.HideOverlay();
    }

    private void PlayDragAnimation()
    {
        dragSequence?.Kill();
        dragSequence = DOTween.Sequence();

        Vector2 startPos = mainCam.WorldToScreenPoint(artefactStartTarget.position);
        Vector2 endPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);

        fakeCursor.position = startPos + new Vector2(150, -150);
        
        // Ensure indicator is off at the start
        if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        
        // 1. Move to Artefact
        dragSequence.Append(fakeCursor.DOMove(startPos, 1f).SetEase(Ease.OutQuad));
        
        // Setup Dotted Line Orientation
        if (dottedRect != null)
        {
            dottedRect.position = startPos;
            dottedRect.sizeDelta = new Vector2(dottedRect.sizeDelta.x, 0); 
            Vector2 direction = endPos - startPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dottedRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // 2. Hover (Change Hand to Open)
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
        });
        dragSequence.AppendInterval(0.3f);
        
        // 3. Grab (Change Hand to Closed, Change Mouse to Left-Click, SHOW INDICATOR)
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = grabCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        dragSequence.AppendInterval(0.2f);
        
        // 4. Start Drag (Change mouse to hold, enable dotted line, HIDE INDICATOR)
        dragSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseLeftClickSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(true);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });

        // Drag cursor and stretch dotted line simultaneously
        dragSequence.Append(fakeCursor.DOMove(endPos, 1.5f).SetEase(Ease.InOutSine));
        if (dottedRect != null)
        {
            float totalDistance = Vector2.Distance(startPos, endPos);
            dragSequence.Join(dottedRect.DOSizeDelta(new Vector2(dottedRect.sizeDelta.x, totalDistance), 1.5f).SetEase(Ease.InOutSine));
        }
        
        // 5. Release (Reset sprites, hide line, SHOW INDICATOR for unclick)
        dragSequence.AppendCallback(() => {
            cursorImage.sprite = hoverCursorSprite;
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (dottedGameObject != null) dottedGameObject.SetActive(false);
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(true);
        });
        
        // Wait a tiny bit, then hide the indicator again
        dragSequence.AppendInterval(0.2f);
        dragSequence.AppendCallback(() => {
            if (clickIndicatorObject != null) clickIndicatorObject.SetActive(false);
        });
        dragSequence.AppendInterval(0.3f);

        dragSequence.OnComplete(() =>
        {
            if (isFirstPhase)
            {
                currentLoopCount++;
                if (currentLoopCount < 2) 
                {
                    PlayDragAnimation(); 
                }
                else
                {
                    isFirstPhase = false;
                    tutorialService.SetInputBlock(false); 
                    StopAnimation();
                    lastActivityTime = Time.time; 
                }
            }
            else
            {
                StopAnimation();
                lastActivityTime = Time.time;
            }
        });
    }
}