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

    [Header("Rules Settings")]
    [SerializeField] private float idleTimeout = 5f;
    [SerializeField] private string targetPieceId = "Artefact_Coin";

    private Transform artefactStartTarget; 
    private int currentLoopCount = 0;
    private Sequence dragSequence;
    private float lastInputTime;
    
    // Global flag to block input in ObjectInteractionManager
    public static bool IsInputBlockedByTutorial { get; private set; } 

    private ObjectDetectionService detectionService;
    private AssemblyService assemblyService;
    private Camera mainCam;

    [Inject]
    public void Construct(ObjectDetectionService detectionService, AssemblyService assemblyService, Camera cam)
    {
        this.detectionService = detectionService;
        this.assemblyService = assemblyService;
        this.mainCam = cam;
    }

    private void OnEnable()
    {
        // Listen to standard input events to reset idle timer
        InteractionEvents.OnMouseMoved += ResetIdleTimer;
        InteractionEvents.OnPressStart += HandleAnyInput;
        
        // Listen to hover to dismiss the tutorial (Rule 3)
        detectionService.OnInteractDetected += HandleObjectHover;
        
        // Dynamically capture the artefact when it spawns
        ArtefactPieceStateMachine.OnCreated += HandlePieceSpawned;
        
        lastInputTime = Time.time;
        fakeCursor.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        InteractionEvents.OnMouseMoved -= ResetIdleTimer;
        InteractionEvents.OnPressStart -= HandleAnyInput;
        detectionService.OnInteractDetected -= HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated -= HandlePieceSpawned;
        
        StopTutorialSequence();
    }

    private void OnDestroy()
    {
        InteractionEvents.OnMouseMoved -= ResetIdleTimer;
        InteractionEvents.OnPressStart -= HandleAnyInput;
        detectionService.OnInteractDetected -= HandleObjectHover;
        ArtefactPieceStateMachine.OnCreated -= HandlePieceSpawned;
    }

    private void Update()
    {
        // Rule 4: If no movement for 5 seconds, enter tutorial mode again
        if (!IsInputBlockedByTutorial && artefactStartTarget != null && (Time.time - lastInputTime) >= idleTimeout)
        {
            StartTutorialSequence();
        }
    }

    private void ResetIdleTimer(Vector2 pos) => lastInputTime = Time.time;
    private void HandleAnyInput() => lastInputTime = Time.time;

    private void HandlePieceSpawned(ArtefactPieceStateMachine piece)
    {
        // Capture the transform dynamically based on the target ID
        if (piece.PieceId == targetPieceId)
        {
            artefactStartTarget = piece.transform;
        }
    }

    private void HandleObjectHover(IInteractObject interactable)
    {
        // Rule 3: When user hovers over the target object, dismiss tutorial
        if (IsInputBlockedByTutorial && interactable != null)
        {
            if (interactable is IArtefactPart part && part.PieceId == targetPieceId)
            {
                StopTutorialSequence();
            }
        }
    }

    public void StartTutorialSequence()
    {
        if (IsInputBlockedByTutorial || artefactStartTarget == null) return;

        IsInputBlockedByTutorial = true; // Rule 1: User input is blocked
        currentLoopCount = 0;
        fakeCursor.gameObject.SetActive(true);
        
        PlayDragAnimation();
    }

    private void StopTutorialSequence()
    {
        IsInputBlockedByTutorial = false;
        fakeCursor.gameObject.SetActive(false);
        dragSequence?.Kill();
        lastInputTime = Time.time; 
    }

    private void PlayDragAnimation()
    {
        dragSequence?.Kill();
        dragSequence = DOTween.Sequence();

        // Dynamically map the world positions to the screen
        Vector2 startPos = mainCam.WorldToScreenPoint(artefactStartTarget.position);
        Vector2 endPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);

        // Reset cursor to a starting offset
        fakeCursor.position = startPos + new Vector2(150, -150);
        cursorImage.sprite = defaultCursorSprite;

        // 1. Move to Artefact
        dragSequence.Append(fakeCursor.DOMove(startPos, 1f).SetEase(Ease.OutQuad));

        // 2. Hover (Change Sprite to Hand Open)
        dragSequence.AppendCallback(() => cursorImage.sprite = hoverCursorSprite);
        dragSequence.AppendInterval(0.3f);

        // 3. Grab (Change Sprite to Hand Closed)
        dragSequence.AppendCallback(() => cursorImage.sprite = grabCursorSprite);
        dragSequence.AppendInterval(0.2f);

        // 4. Drag to Inspection Center
        dragSequence.Append(fakeCursor.DOMove(endPos, 1.5f).SetEase(Ease.InOutSine));

        // 5. Release
        dragSequence.AppendCallback(() => cursorImage.sprite = defaultCursorSprite);
        dragSequence.AppendInterval(0.5f);

        // Evaluation
        dragSequence.OnComplete(() =>
        {
            currentLoopCount++;
            if (currentLoopCount < 2) 
            {
                // Rule 2: Loop 2x
                PlayDragAnimation();
            }
            else
            {
                StopTutorialSequence();
            }
        });
    }
}