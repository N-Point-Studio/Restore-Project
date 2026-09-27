using UnityEngine;
using UnityEngine.Localization;
using System.Collections;
using VContainer;

public class TutorialClipboardAnimator : TutorialAnimatorBase
{
    [Header("Highlight Settings")]
    [SerializeField] private Canvas clipboardCanvas;
    [SerializeField] private int tutorialSortingOrder = 100;

    [Header("Text Swap Settings")]
    [SerializeField] private LocalizedString successLocalizedText;
    [SerializeField] private TutorialLocalizationBridge tutorialLocalizationBridge;
    [SerializeField] private float readTime = 3f;

    private bool originalOverrideSorting;
    private int originalSortingOrder;
    private bool hasHovered = false;
    private GameplayManager gameplayManager;
    private ToolService toolService;

    [Inject]
    public void ConstructChild(
        GameplayManager gameplayManager, ToolService toolService)
    {
        this.gameplayManager = gameplayManager;
        this.toolService = toolService;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (toolService != null)
        {
            toolService.ForceReturnTool();
        }

        ClipBoardUI.OnClipboardHovered += HandleClipboardHovered;
        
        HideUIElements();
        StartTutorialSequence(false); 
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClipBoardUI.OnClipboardHovered -= HandleClipboardHovered;
    }

    private void HandleClipboardHovered()
    {
        if (!gameplayManager.isTutorialAvailable) 
            return;
        
        if (hasHovered) 
            return;
        hasHovered = true;

        HideUIElements();

        if (tutorialLocalizationBridge != null && successLocalizedText != null)
        {
            tutorialLocalizationBridge.SetLocalizedString(successLocalizedText);
        }

        StartCoroutine(WaitAndComplete());
    }

    private IEnumerator WaitAndComplete()
    {
        yield return new WaitForSecondsRealtime(readTime);

        if (tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 5)
        {
            tutorialService.CompleteStage();
        }

        CompleteTutorial();
    }

    protected override void SetupUIAndPlayAnimation()
    {
        TutorialEvents.OnShowClipBoardArrow?.Invoke(true);

        // 1. Pull the Clipboard UI above the dark overlay
        if (clipboardCanvas != null)
        {
            originalOverrideSorting = clipboardCanvas.overrideSorting;
            originalSortingOrder = clipboardCanvas.sortingOrder;

            clipboardCanvas.overrideSorting = true;
            clipboardCanvas.sortingOrder = tutorialSortingOrder;
        }

        // 2. Dim the background and dynamically highlight the Artefact at the inspect point!
        if (overlayController != null && assemblyService != null) 
        {
            overlayController.ShowOverlay(assemblyService.GetInspectPoint());
        }
    }

    protected override void HideUIElements()
    {
        TutorialEvents.OnShowClipBoardArrow?.Invoke(false);
        
        // Restore original clipboard sorting
        if (clipboardCanvas != null)
        {
            clipboardCanvas.overrideSorting = originalOverrideSorting;
            clipboardCanvas.sortingOrder = originalSortingOrder;
        }

        if (overlayController != null) overlayController.HideOverlay();
    }

    protected override void KillSequence() { }
}