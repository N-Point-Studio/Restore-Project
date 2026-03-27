using System;
using System.Collections;
using System.Collections.Generic;
using Modules;
using UnityEngine;
using VContainer;

public class LevelSelectionController : BaseMenuController
{
    [Header("UI References")]
    [SerializeField] private ArtefactDetailController artefactDetailController;
    [SerializeField] private ButtonInputInstructionUI backButtonItemUI;

    [Header("3D Scene References")]
    [SerializeField] private Artefact3DItem[] artefact3DItems;
    [SerializeField] private PedestalAnimator[] emptyPedestals;

    [Header("Target Scene")]
    [SerializeField] private string targetScene;

    [Header("Animation Timings")]
    [SerializeField] private float hideAnimDuration = 0.4f;
    [SerializeField] private float initialSequenceDelay = 0.5f;
    [SerializeField] private float completionRevealDelay = 2.0f;
    [SerializeField] private float unlockSequenceDelay = 0.3f;
    [SerializeField] private float unlockDropDelay = 1.5f;
    [SerializeField] private float restoreAnimDuration = 0.5f;
    

    // Inject
    private ActiveArtefactData activeArtefactData;
    private PlayerProgressionData playerProgressionData;
    private SceneLoader sceneLoader;
    private InputSystemService input;
    private GameConfigData config;

    private Artefact3DItem clickedItem;
    private bool isCameraMoving = false;

    private WaitForSeconds waitInitialSequence;
    private WaitForSeconds waitCompletionReveal;
    private WaitForSeconds waitUnlockSequence;
    private WaitForSeconds waitUnlockDrop;

    [Inject]
    public void Construct(
        ActiveArtefactData activeArtefactData, 
        PlayerProgressionData playerProgressionData,
        SceneLoader sceneLoader,
        InputSystemService input,
        GameConfigData config        
        )
    {
        this.activeArtefactData = activeArtefactData;
        this.playerProgressionData = playerProgressionData;
        this.sceneLoader = sceneLoader;
        this.input = input;
        this.config = config;
        artefactDetailController.SetInputSystemService(this.input);
    }

    protected override void Awake()
    {
        waitInitialSequence = new WaitForSeconds(initialSequenceDelay);
        waitCompletionReveal = new WaitForSeconds(completionRevealDelay);
        waitUnlockSequence = new WaitForSeconds(unlockSequenceDelay);
        waitUnlockDrop = new WaitForSeconds(unlockDropDelay);

        base.Awake();

        MainMenuEvents.OnArtefactPlay += OnRequestArtefactPlay;
        MainMenuEvents.OnOpenArtefactDetail += OnRequestArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail += OnCloseArtefactDetail;
        MainMenuEvents.OnCameraMoveFinished += OnCameraMoveFinished;
        backButtonItemUI.OnClick += OnBackButtonClick;
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();        
        SetArtefactsInteractable(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MainMenuEvents.OnArtefactPlay -= OnRequestArtefactPlay;
        MainMenuEvents.OnOpenArtefactDetail -= OnRequestArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail -= OnCloseArtefactDetail;
        MainMenuEvents.OnCameraMoveFinished -= OnCameraMoveFinished;
        backButtonItemUI.OnClick -= OnBackButtonClick;
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);
        if (isActive) 
        {
            input.OnUIKeycodeEscapePerformed += OnUIKeycodeEscapePerformed; 
        }
        else 
        {
            input.OnUIKeycodeEscapePerformed -= OnUIKeycodeEscapePerformed; 
        }
    }

    public void OpenLevelSelection()
    {
        RefreshUI();
        SetActive(true);
        SetArtefactsInteractable(true);
        MainMenuEvents.TriggerCameraToLevelSelection();
    }

    public void CloseLevelSelection()
    {
        SetActive(false);
        SetArtefactsInteractable(false);
    }
    
    public void OpenLevelSelectionAndPlayAnimations()
    {
        RefreshUI();
        SetActive(true);
        SetArtefactsInteractable(false);
        MainMenuEvents.TriggerCameraToLevelSelection();
        
        StartCoroutine(PlayPendingAnimationsSequence());
    }

    private void SetArtefactsInteractable(bool active)
    {
        if (artefact3DItems == null) return;
        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null)
            {
                artefact3DItems[i].SetSelectionModeActive(active);
            }
        }
    }

    public void RefreshUI()
    {
        if (artefact3DItems == null || artefact3DItems.Length == 0) return;

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null)
            {
                artefact3DItems[i].Initialize(activeArtefactData);
            }
        }
    }

    private void OnRequestArtefactPlay(ArtefactData data)
    {
        playerProgressionData.SetCurrentActiveArtefact(data.BaseData.Id);
        _ = sceneLoader.LoadSceneAsync(targetScene, config.minLoadingScreenDuration, "Unboxing Artefact...");
    }

    private void OnRequestArtefactDetail(ArtefactData data)
    {        
        backButtonItemUI.gameObject.SetActive(false);
        clickedItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.BaseData.Id);

        if (clickedItem == null)
            AppLogger.LogWarning($"[LevelSelection] Artefact3DItem with ID {data.BaseData.Id} was not found in the Scene!");

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null && artefact3DItems[i] != clickedItem)
                artefact3DItems[i].AnimateItem(false, hideAnimDuration);
        }

        if (emptyPedestals != null)
        {
            for (int i = 0; i < emptyPedestals.Length; i++)
            {
                PedestalAnimator empty = emptyPedestals[i];
                if (empty != null) empty.AnimatePedestal(false, hideAnimDuration);
            }
        }

        artefactDetailController.OpenDetail(data, activeArtefactData, clickedItem, false);
    }

    private void OpenArtefactDetailAutomatically(ArtefactData data)
    {
        backButtonItemUI.gameObject.SetActive(false);
        clickedItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.BaseData.Id);

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null && artefact3DItems[i] != clickedItem)
                artefact3DItems[i].AnimateItem(false, hideAnimDuration);
        }

        if (emptyPedestals != null)
        {
            for (int i = 0; i < emptyPedestals.Length; i++)
            {
                PedestalAnimator empty = emptyPedestals[i];
                if (empty != null) empty.AnimatePedestal(false, hideAnimDuration);
            }
        }

        artefactDetailController.OpenDetail(data, activeArtefactData, clickedItem, true);
    }

    private void OnCloseArtefactDetail()
    {
        backButtonItemUI.gameObject.SetActive(true);
        artefactDetailController.CloseDetail(); 

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null && artefact3DItems[i] != clickedItem)
                artefact3DItems[i].AnimateItem(true, restoreAnimDuration);
        }

        if (emptyPedestals != null)
        {
            for (int i = 0; i < emptyPedestals.Length; i++)
            {
                PedestalAnimator empty = emptyPedestals[i];
                if (empty != null) empty.AnimatePedestal(true, restoreAnimDuration);
            }
        }

        if (activeArtefactData.GetPendingUnlockAnimations().Count > 0)
        {
            StartCoroutine(PlayPendingUnlocksSequence());
        }
        else
        {
            isCameraMoving = true;
            MainMenuEvents.TriggerCameraToLevelSelection();
            SetArtefactsInteractable(true);
        }
    }

    private void OnCameraMoveFinished()
    {
        isCameraMoving = false;
    }

    private IEnumerator PlayPendingAnimationsSequence()
    {
        yield return waitInitialSequence;

        if (artefact3DItems == null || artefact3DItems.Length == 0) 
        {
            SetArtefactsInteractable(true); 
            yield break;
        }

        List<ArtefactProgressData> pendingCompletions = activeArtefactData.GetPendingCompletionAnimations();
        
        if (pendingCompletions.Count > 0)
        {
            ArtefactProgressData data = pendingCompletions[0];
            Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
            
            if (targetItem != null)
            {
                // 1. Move camera forward to Artefact
                isCameraMoving = true;
                MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                
                // 2. Wait until the camera blend event finishes
                yield return new WaitUntil(() => !isCameraMoving);

                MainMenuEvents.TriggerShowBackground(true);

                AppLogger.Log($"[Animation] Playing REVEAL animation for {data.artefactId}");
                targetItem.PlayCompletionAnimation();
                
                yield return waitCompletionReveal;
                
                activeArtefactData.MarkCompletionAnimSeen(data.artefactId);

                ArtefactData aData = activeArtefactData.GetArtefactDatabase().GetItem(data.artefactId);
                if (aData != null) OpenArtefactDetailAutomatically(aData);
            }
            
            yield break;
        }

        yield return StartCoroutine(PlayPendingUnlocksSequence());
    }

    private IEnumerator PlayPendingUnlocksSequence()
    {
        List<ArtefactProgressData> pendingUnlocks = activeArtefactData.GetPendingUnlockAnimations();
        
        if (pendingUnlocks.Count > 0)
        {
            yield return waitUnlockSequence; // Small delay to keep it neat after the journal is closed
            
            for (int i = 0; i < pendingUnlocks.Count; i++)
            {
                ArtefactProgressData data = pendingUnlocks[i];
                Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
                
                if (targetItem != null)
                {
                    // 1. Move camera to the new Box pedestal
                    isCameraMoving = true;
                    MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                    
                    yield return new WaitUntil(() => !isCameraMoving);

                    AppLogger.Log($"[Animation] Playing UNLOCK animation for {data.artefactId}");
                    targetItem.PlayUnlockAnimation();
                    
                    // Wait for the box drop animation
                    yield return waitUnlockDrop;
                }
                
                activeArtefactData.MarkUnlockAnimSeen(data.artefactId);
            }
            
            // After all new boxes fall, move camera back to Overview
            isCameraMoving = true;
            MainMenuEvents.TriggerCameraToLevelSelection();
            yield return new WaitUntil(() => !isCameraMoving);
        }
        
        RefreshUI();

        SetArtefactsInteractable(true);
    }

    private void OnBackButtonClick()
    {
        CloseLevelSelection();
        MainMenuEvents.TriggerCloseLevelSelection();
        MainMenuEvents.TriggerCameraToMainMenu();
    }

    private void OnUIKeycodeEscapePerformed()
    {
        if (artefactDetailController != null && artefactDetailController.IsOpen) 
        {
            return; 
        }

        OnBackButtonClick();
    }
}