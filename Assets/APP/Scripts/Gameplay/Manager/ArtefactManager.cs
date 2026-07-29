using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ArtefactManager : IInitializable, IDisposable
{
    private readonly ToolService toolService;
    private readonly AssemblyService assemblyService;
    private readonly HoldProgressUI holdProgressUI;
    private readonly GameConfigData config;
    private readonly Inspection inspection;
    private bool isHoldingUI = false;
    private bool isGameFinished = false;
    private bool isTutorialShown = false;

    private IArtefactPart currentDraggedPart;
    private IInteractObject currentHoldInteract;
    private IArtefactPart currentHoldPart;

    private GameplayManager gameplayManager;
    private TutorialService tutorialService;

    [Inject]
    public ArtefactManager(ToolService toolService,
    AssemblyService assemblyService,
    HoldProgressUI holdProgressUI,
    GameConfigData config,
    GameplayManager gameplayManager,
    TutorialService tutorialService,
    Inspection inspection)
    {
        this.assemblyService = assemblyService;
        this.toolService = toolService;
        this.holdProgressUI = holdProgressUI;
        this.config = config;
        this.gameplayManager = gameplayManager;
        this.tutorialService = tutorialService;
        this.inspection = inspection;
    }

    public void Initialize()
    {
        InteractionEvents.OnHoldPerformed += HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted += HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled += HandleHoldCanceled;

        InteractionEvents.OnDragStarted += HandleDragStarted;
        InteractionEvents.OnDragPerformed += HandleDragPerformed;
        InteractionEvents.OnDragEnded += HandleDragEnded;

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        InteractionEvents.OnHoldPerformed -= HandleHoldPerformed;
        InteractionEvents.OnHoldCompleted -= HandleHoldCompleted;
        InteractionEvents.OnHoldCanceled -= HandleHoldCanceled;

        InteractionEvents.OnDragStarted -= HandleDragStarted;
        InteractionEvents.OnDragPerformed -= HandleDragPerformed;
        InteractionEvents.OnDragEnded -= HandleDragEnded;

        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
    }

    private void HandleGameWrapped()
    {
        isGameFinished = true;
    }

    private IArtefactPart ResolveArtefactPart(IInteractObject interact)
    {
        if (interact == null) return null;

        if (interact is MonoBehaviour mono)
        {
            if (mono == null) return null;
            return mono.GetComponentInParent<IArtefactPart>();
        }

        if (interact is IArtefactPart part) return part;
        return null;
    }

    private void HandleDragStarted(IInteractObject interact, Vector3 worldPos)
    {
        currentDraggedPart = ResolveArtefactPart(interact);

        // Prevent an already assembled piece from being flagged as a dragged piece for assembly
        if (currentDraggedPart != null && currentDraggedPart.CurrentState == ArtefactPieceState.Assembled)
        {
            currentDraggedPart = null;
        }

        if (interact is IDragObject drag) { drag.OnDragStarted(worldPos); }
    }

    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IDragObject drag) { drag.OnDragPerformed(worldPos); }
        if (currentDraggedPart != null) assemblyService.TryCheckSlot(currentDraggedPart, worldPos);

        if (assemblyService.IsInspectEmpty() && currentDraggedPart != null)
        {
            inspection.SetSphereRenderer(true);
        }
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
        inspection.SetSphereRenderer(false);
        IArtefactPart artefactPart = currentDraggedPart;
        currentDraggedPart = null;

        if (toolService.IsOnToolMode || artefactPart == null) return;

        if (assemblyService.isArtefactSlotAvailable && assemblyService.TryAssemble(artefactPart))
        {
            if (gameplayManager.CheckTutorialAvailability(2))
            {
                if (!isTutorialShown)
                {
                    if (assemblyService.TotalCurrentParts() == 1)
                    {
                        tutorialService.StartInstantTutorial(3, 0);
                        isTutorialShown = true;
                    }
                }

                else if (assemblyService.TotalCurrentParts() > 1)
                {
                    if (tutorialService.CurrentStage == 3 && tutorialService.CurrentModule == 0)
                    {
                        tutorialService.CompleteAndAdvance(true);
                    }
                }
            }

            return;
        }

        if (interact is IDragObject drag) drag.OnDragEnded(worldPos);

        assemblyService.DismissCheckSlot();
    }

    private void HandleHoldPerformed(IInteractObject interact, float holdTime, Vector2 position)
    {
        if (toolService.IsOnToolMode || isGameFinished) return;

        if (currentHoldInteract != interact)
        {
            currentHoldInteract = interact;
            currentHoldPart = ResolveArtefactPart(interact);
        }

        if (currentHoldPart == null) return;

        if (!isHoldingUI)
        {
            isHoldingUI = true;
            ShowHoldProgress(currentHoldPart, position);
        }

        float normalized = Mathf.Clamp01(holdTime / config.holdDuration);
        UpdateHoldProgress(currentHoldPart, normalized, position);
    }

    private void HandleHoldCompleted(IInteractObject interact, Vector2 position)
    {
        if (toolService.IsOnToolMode || isGameFinished) return;

        IArtefactPart partToDetach = currentHoldPart;

        currentHoldInteract = null;
        currentHoldPart = null;

        HideHoldProgress(partToDetach);
        isHoldingUI = false;

        if (partToDetach != null)
        {
            assemblyService.Detach(partToDetach);
            if (tutorialService.CurrentStage == 3 && tutorialService.CurrentModule == 1)
            {
                tutorialService.CompleteStage();
            }
        }
    }

    private void HandleHoldCanceled(IInteractObject interact, Vector2 position)
    {
        if (toolService.IsOnToolMode || isGameFinished) return;

        HideHoldProgress(currentHoldPart);
        isHoldingUI = false;

        currentHoldInteract = null;
        currentHoldPart = null;
    }

    private void ShowHoldProgress(IArtefactPart part, Vector2 position)
    {
        if (part == null) return;
        if (part.CurrentState != ArtefactPieceState.Assembled) return;
        holdProgressUI.Show(position);
    }

    private void UpdateHoldProgress(IArtefactPart part, float normalized, Vector2 position)
    {
        if (part == null) return;
        if (part.CurrentState != ArtefactPieceState.Assembled) return;
        holdProgressUI.UpdateProgress(normalized, position);
    }

    private void HideHoldProgress(IArtefactPart part)
    {
        if (part == null) return;
        if (part.CurrentState != ArtefactPieceState.Assembled) return;
        holdProgressUI.Hide();
    }
}