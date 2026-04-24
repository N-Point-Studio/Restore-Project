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
    private bool isHoldingUI = false;
    private bool isGameFinished = false;

    // --- OPTIMIZATION CACHES ---
    private IArtefactPart currentDraggedPart;
    private IInteractObject currentHoldInteract;
    private IArtefactPart currentHoldPart;

    [Inject]
    public ArtefactManager(ToolService toolService, AssemblyService assemblyService, HoldProgressUI holdProgressUI, GameConfigData config)
    {
        this.assemblyService = assemblyService;
        this.toolService = toolService;
        this.holdProgressUI = holdProgressUI;
        this.config = config;
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
        
        // FIX: Safely check if the underlying object was destroyed!
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
        if (interact is IDragObject drag) { drag.OnDragStarted(worldPos); }
    }

    private void HandleDragPerformed(IInteractObject interact, Vector3 worldPos)
    {
        if (interact is IDragObject drag) { drag.OnDragPerformed(worldPos); }
        if (currentDraggedPart != null) assemblyService.TryCheckSlot(currentDraggedPart, worldPos);
    }

    private void HandleDragEnded(IInteractObject interact, Vector3 worldPos)
    {
        IArtefactPart artefactPart = currentDraggedPart;
        currentDraggedPart = null; 

        if (toolService.IsOnToolMode || artefactPart == null) return;

        float distance = Vector3.Distance(worldPos, assemblyService.GetInspectPoint().position);
        bool isCloseEnough = distance < config.assembleSnapDistance;

        if (isCloseEnough && assemblyService.TryAssemble(artefactPart)) return;
        if (interact is IDragObject drag) drag.OnDragEnded(worldPos);
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