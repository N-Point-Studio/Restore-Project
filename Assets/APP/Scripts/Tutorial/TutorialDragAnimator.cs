using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialDragAnimator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform fakeCursor;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite defaultCursorSprite;
    [SerializeField] private Sprite hoverCursorSprite;
    [SerializeField] private Sprite grabCursorSprite;

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

    private void OnEnable()
    {
        InteractionEvents.OnMouseMoved += ResetIdleTimer;
        InteractionEvents.OnPressStart += HandleAnyInput;
        detectionService.OnInteractDetected += HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated += HandlePieceSpawned;
        AssembleEvents.OnAssemblePerformed += HandleAssemblyPerformed; 
        
        fakeCursor.gameObject.SetActive(false);
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
        isAnimationPlaying = true;

        if (overlayController != null) overlayController.SetActive(true);

        PlayDragAnimation();
    }

    private void StopAnimation()
    {
        isAnimationPlaying = false;
        fakeCursor.gameObject.SetActive(false);
        dragSequence?.Kill();
        
        if (overlayController != null) overlayController.SetActive(false);
    }

    private void PlayDragAnimation()
    {
        dragSequence?.Kill();
        dragSequence = DOTween.Sequence();

        Vector2 startPos = mainCam.WorldToScreenPoint(artefactStartTarget.position);
        Vector2 endPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);

        fakeCursor.position = startPos + new Vector2(150, -150);
        cursorImage.sprite = defaultCursorSprite;

        dragSequence.Append(fakeCursor.DOMove(startPos, 1f).SetEase(Ease.OutQuad));
        dragSequence.AppendCallback(() => cursorImage.sprite = hoverCursorSprite);
        dragSequence.AppendInterval(0.3f);
        dragSequence.AppendCallback(() => cursorImage.sprite = grabCursorSprite);
        dragSequence.AppendInterval(0.2f);
        dragSequence.Append(fakeCursor.DOMove(endPos, 1.5f).SetEase(Ease.InOutSine));
        dragSequence.AppendCallback(() => cursorImage.sprite = defaultCursorSprite);
        dragSequence.AppendInterval(0.5f);

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