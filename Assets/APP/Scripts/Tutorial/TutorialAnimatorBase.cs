using UnityEngine;
using VContainer;

public abstract class TutorialAnimatorBase : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float idleTimeout = 10f;

    protected int currentLoopCount = 0;
    protected bool isFirstPhase = true;
    protected bool isTaskCompleted = false;
    protected bool isAnimationPlaying = false;
    protected float lastActivityTime;

    protected TutorialService tutorialService;
    protected AssemblyService assemblyService;
    protected TutorialOverlayUI overlayController;
    protected Camera mainCam;

    [Inject]
    public void ConstructBase(
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

    protected virtual void OnEnable()
    {
        InteractionEvents.OnMouseMoved += ResetIdleTimer;
        InteractionEvents.OnPressStart += HandleAnyInput;

        isFirstPhase = true;
        currentLoopCount = 0;
        isTaskCompleted = false;
    }

    protected virtual void OnDisable()
    {
        InteractionEvents.OnMouseMoved -= ResetIdleTimer;
        InteractionEvents.OnPressStart -= HandleAnyInput;

        StopAnimation();
        if (tutorialService != null) tutorialService.SetInputBlock(false);
    }

    protected virtual void Update()
    {
        if (isTaskCompleted || isFirstPhase || isAnimationPlaying) return;

        if (Time.time - lastActivityTime >= idleTimeout)
        {
            StartTutorialSequence(false);
        }
    }

    protected void RecordActivity()
    {
        lastActivityTime = Time.time;
        if (!isFirstPhase && isAnimationPlaying)
        {
            StopAnimation();
        }
    }

    private void ResetIdleTimer(Vector2 pos) => RecordActivity();
    private void HandleAnyInput() => RecordActivity();

    public virtual void StartTutorialSequence(bool blockInput)
    {
        if (isTaskCompleted) return;

        if (blockInput && tutorialService != null) tutorialService.SetInputBlock(true);
        isAnimationPlaying = true;

        SetupUIAndPlayAnimation();
    }

    protected virtual void StopAnimation()
    {
        isAnimationPlaying = false;
        if (overlayController != null) overlayController.HideOverlay();

        KillSequence();
        HideUIElements();
    }

    protected void CompleteTutorial()
    {
        isTaskCompleted = true;
        StopAnimation();
        if (tutorialService != null) tutorialService.SetInputBlock(false);
        gameObject.SetActive(false);
    }

    protected void OnSequenceLoopComplete()
    {
        if (isFirstPhase)
        {
            currentLoopCount++;
            if (currentLoopCount < 2)
            {
                SetupUIAndPlayAnimation();
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
    }

    // --- Abstract Methods (Child scripts MUST implement these) ---
    protected abstract void SetupUIAndPlayAnimation();
    protected abstract void HideUIElements();
    protected abstract void KillSequence();
}