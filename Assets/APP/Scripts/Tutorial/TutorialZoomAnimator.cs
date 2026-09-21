using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class TutorialZoomAnimator : MonoBehaviour
{
    [Header("Physical Mouse Indicator")]
    [SerializeField] private RectTransform mouseRect;
    [SerializeField] private Image physicalMouseImage;
    [SerializeField] private Sprite mouseDefaultSprite;
    [SerializeField] private Sprite mouseScrollUpSprite;
    [SerializeField] private Sprite mouseScrollDownSprite;

    [Header("Concentric Rings Effect")]
    [Tooltip("The UI Image containing the dotted concentric rings.")]
    [SerializeField] private CanvasGroup ringsCanvasGroup;
    [SerializeField] private RectTransform ringsRect;

    [Header("Settings")]
    [SerializeField] private float idleTimeout = 10f; 

    private Sequence zoomSequence;
    
    private int currentLoopCount = 0;
    private bool isFirstPhase = true; 
    private bool isTaskCompleted = false;
    private bool isAnimationPlaying = false;
    private float lastActivityTime;

    private TutorialService tutorialService;
    private AssemblyService assemblyService;
    private TutorialOverlayUI overlayController;
    private Camera mainCam;

    [Inject]
    public void Construct(
        TutorialService tutorialService, 
        AssemblyService assemblyService, 
        TutorialOverlayUI overlayController,
        Camera cam)
    {
        this.tutorialService = tutorialService;
        this.assemblyService = assemblyService;
        this.overlayController = overlayController;
        this.mainCam = cam;
    }

    private void OnEnable()
    {
        InteractionEvents.OnMouseMoved += ResetIdleTimer;
        InteractionEvents.OnPressStart += HandleAnyInput;
        InteractionEvents.OnZoomPerformed += HandleZoomPerformed;

        if (mouseRect != null) mouseRect.gameObject.SetActive(false);
        if (ringsCanvasGroup != null) ringsCanvasGroup.alpha = 0f;

        isFirstPhase = true;
        currentLoopCount = 0;

        StartTutorialSequence(true); 
    }

    private void OnDisable()
    {
        InteractionEvents.OnMouseMoved -= ResetIdleTimer;
        InteractionEvents.OnPressStart -= HandleAnyInput;
        InteractionEvents.OnZoomPerformed -= HandleZoomPerformed;
        
        StopAnimation();
        
        if (tutorialService != null) tutorialService.SetInputBlock(false); 
    }

    private void Update()
    {
        if (isTaskCompleted || isFirstPhase || isAnimationPlaying) return;

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

    private void HandleZoomPerformed(float zoomDelta)
    {
        // If the user scrolls their mouse wheel AT ALL, the tutorial is beaten!
        isTaskCompleted = true;
        StopAnimation();
        if (tutorialService != null) tutorialService.SetInputBlock(false);
        gameObject.SetActive(false); 
    }

    public void StartTutorialSequence(bool blockInput)
    {
        if (isTaskCompleted) return;

        if (blockInput && tutorialService != null) tutorialService.SetInputBlock(true); 
        
        isAnimationPlaying = true;
        if (mouseRect != null) mouseRect.gameObject.SetActive(true);

        if (overlayController != null && assemblyService != null) 
        {
            overlayController.ShowOverlay(assemblyService.GetInspectPoint());
        }

        if (assemblyService != null && mainCam != null)
        {
            Vector2 centerPos = mainCam.WorldToScreenPoint(assemblyService.GetInspectPoint().position);
            if (ringsRect != null) ringsRect.position = centerPos;
            if (mouseRect != null) mouseRect.position = centerPos + new Vector2(250, -200);
        }

        PlayZoomAnimation();
    }

    private void StopAnimation()
    {
        isAnimationPlaying = false;
        if (mouseRect != null) mouseRect.gameObject.SetActive(false);
        if (ringsCanvasGroup != null) ringsCanvasGroup.alpha = 0f;
        zoomSequence?.Kill();
        
        if (overlayController != null) overlayController.HideOverlay();
    }

    private void PlayZoomAnimation()
    {
        zoomSequence?.Kill();
        zoomSequence = DOTween.Sequence();

        // 1. Simulate Scroll Up (Rings Minimize/Contract)
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseScrollUpSprite;
            
            if (ringsRect != null && ringsCanvasGroup != null)
            {
                // Start big and faded out
                ringsRect.localScale = Vector3.one * 1.5f;
                ringsCanvasGroup.alpha = 0f;
                
                // Contract inward and fade in
                ringsRect.DOScale(Vector3.one * 0.5f, 1f).SetEase(Ease.OutQuad);
                ringsCanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);
            }
        });
        
        zoomSequence.AppendInterval(1.2f);

        // 2. Simulate Scroll Down (Rings Maximize/Expand)
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseScrollDownSprite;
            
            if (ringsRect != null && ringsCanvasGroup != null)
            {
                // Start small and fully visible
                ringsRect.localScale = Vector3.one * 0.5f;
                ringsCanvasGroup.alpha = 1f;
                
                // Expand outward and fade out
                ringsRect.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutQuad);
                ringsCanvasGroup.DOFade(0f, 1f).SetEase(Ease.InQuad);
            }
        });

        zoomSequence.AppendInterval(1.2f);
        
        // Reset to default
        zoomSequence.AppendCallback(() => {
            if (physicalMouseImage != null) physicalMouseImage.sprite = mouseDefaultSprite;
            if (ringsCanvasGroup != null) ringsCanvasGroup.alpha = 0f;
        });

        zoomSequence.AppendInterval(0.5f);

        zoomSequence.OnComplete(() =>
        {
            if (isFirstPhase)
            {
                currentLoopCount++;
                if (currentLoopCount < 2) 
                {
                    PlayZoomAnimation(); 
                }
                else
                {
                    isFirstPhase = false;
                    if (tutorialService != null) tutorialService.SetInputBlock(false); 
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