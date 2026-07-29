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
    private bool isPlayingSequence = false;

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
        GameConfigData config,
        ProjectSavingSystem projectSavingSystem
        )
    {
        this.activeArtefactData = activeArtefactData;
        this.playerProgressionData = playerProgressionData;
        this.sceneLoader = sceneLoader;
        this.input = input;
        this.config = config;
        artefactDetailController.SetServices(this.input, this.playerProgressionData, projectSavingSystem);
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

        isPlayingSequence = true;
        if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(false);

        StartCoroutine(PlayPendingAnimationsSequence());
    }

    public void OpenLevelSelectionAndShowDetail(string artefactId)
    {
        RefreshUI();
        SetActive(true);
        SetArtefactsInteractable(false);
        
        StartCoroutine(PlayReplayReturnSequence(artefactId));
    }

    private IEnumerator PlayReplayReturnSequence(string artefactId)
    {
        yield return waitInitialSequence;

        ArtefactData aData = activeArtefactData.GetArtefactDatabase().GetItem(artefactId);
        if (aData != null)
        {
            Artefact3DItem foundItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == aData.BaseData.Id);

            if (foundItem != null)
            {
                Transform targetTransform = foundItem.ArtefactTransform != null && foundItem.ArtefactTransform.gameObject.activeSelf 
                    ? foundItem.ArtefactTransform 
                    : foundItem.ArtefactCameraTransform;

                isCameraMoving = true;
                MainMenuEvents.TriggerCameraFocusToArtefact(targetTransform);

                yield return new WaitUntil(() => !isCameraMoving);
                
                yield return new WaitForSeconds(0.25f);
            }

            MainMenuEvents.TriggerShowBackground(true);
            OpenArtefactDetailAutomatically(aData, false);
        }
        else
        {
            MainMenuEvents.TriggerCameraToLevelSelection();
            SetArtefactsInteractable(true);
            if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(true);
        }
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
        
        _ = sceneLoader.LoadSceneAsync(
            targetScene, 
            config.minLoadingScreenDuration, 
            config.unboxingArtefactString.GetLocalizedString()
        );
    }

    private void OnRequestArtefactDetail(ArtefactData data)
    {
        OpenArtefactDetailAutomatically(data, false); 
    }

    private void OpenArtefactDetailAutomatically(ArtefactData data, bool forceShowBook)
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

        artefactDetailController.OpenDetail(data, activeArtefactData, clickedItem, forceShowBook);
    }

    private void OnCloseArtefactDetail()
    {
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

        if (activeArtefactData.GetPendingUnlockAnimations().Count > 0 && !isPlayingSequence)
        {
            isPlayingSequence = true;
            if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(false);

            StartCoroutine(PlayPendingUnlocksSequence());
        }
        else
        {
            if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(true);

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
            isPlayingSequence = false;
            if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(true);
            yield break;
        }

        List<ArtefactProgressData> pendingCompletions = activeArtefactData.GetPendingCompletionAnimations();

        if (pendingCompletions.Count > 0)
        {
            ArtefactProgressData data = pendingCompletions[0];
            Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);

            if (targetItem != null)
            {
                isCameraMoving = true;
                MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.ArtefactCameraTransform);

                yield return new WaitUntil(() => !isCameraMoving);
                yield return new WaitForSeconds(0.25f);

                MainMenuEvents.TriggerShowBackground(true);

                AppLogger.Log($"[Animation] Playing REVEAL animation for {data.artefactId}");
                targetItem.PlayCompletionAnimation();

                yield return waitCompletionReveal;

                activeArtefactData.MarkCompletionAnimSeen(data.artefactId);

                isPlayingSequence = false;

                ArtefactData aData = activeArtefactData.GetArtefactDatabase().GetItem(data.artefactId);
                // True = paksa buka Journal untuk menampilkan cerita pertamakalinya
                if (aData != null) OpenArtefactDetailAutomatically(aData, true); 
            }
            else
            {
                activeArtefactData.MarkCompletionAnimSeen(data.artefactId);
                isPlayingSequence = false;
                if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(true);
                SetArtefactsInteractable(true);
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
            isCameraMoving = true;
            MainMenuEvents.TriggerCameraToLevelSelection();
            yield return new WaitUntil(() => !isCameraMoving);

            yield return waitUnlockSequence; 
            
            ArtefactProgressData lastUnlockedData = null;

            for (int i = 0; i < pendingUnlocks.Count; i++)
            {
                ArtefactProgressData data = pendingUnlocks[i];
                Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
                
                if (targetItem != null)
                {
                    if (targetItem.BoxTransform != null && !targetItem.BoxTransform.gameObject.activeInHierarchy) 
                    {
                        targetItem.BoxTransform.localScale = Vector3.zero; 
                        targetItem.BoxTransform.gameObject.SetActive(true); 
                    }

                    // Fokus kamera ke box yang baru muncul
                    isCameraMoving = true;
                    MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.BoxTransform != null ? targetItem.BoxTransform : targetItem.ArtefactCameraTransform);
                    
                    yield return new WaitUntil(() => !isCameraMoving);
                    
                    yield return new WaitForSeconds(0.25f);

                    AppLogger.Log($"[Animation] Playing UNLOCK animation for {data.artefactId}");
                    targetItem.PlayUnlockAnimation();
                    
                    yield return waitUnlockDrop;
                    lastUnlockedData = data;
                }
                
                activeArtefactData.MarkUnlockAnimSeen(data.artefactId);
            }
            
            if (lastUnlockedData != null)
            {
                ArtefactData aData = activeArtefactData.GetArtefactDatabase().GetItem(lastUnlockedData.artefactId);
                if (aData != null)
                {
                    AppLogger.Log($"[Unlock Flow] Auto-opening detail for {lastUnlockedData.artefactId}");
                    
                    RefreshUI();
                    isPlayingSequence = false;

                    MainMenuEvents.TriggerShowBackground(true);

                    OpenArtefactDetailAutomatically(aData, false); 
                    
                    yield break; 
                }
            }
            
            isCameraMoving = true;
            MainMenuEvents.TriggerCameraToLevelSelection();
            yield return new WaitUntil(() => !isCameraMoving);
        }
        
        RefreshUI();
        SetArtefactsInteractable(true);

        isPlayingSequence = false;
        if (backButtonItemUI != null) backButtonItemUI.gameObject.SetActive(true);
    }

    private void OnBackButtonClick()
    {
        if (isPlayingSequence) return;

        CloseLevelSelection();
        MainMenuEvents.TriggerCloseLevelSelection();
        MainMenuEvents.TriggerCameraToMainMenu();
    }

    private void OnUIKeycodeEscapePerformed()
    {
        if (isPlayingSequence) return;

        if (artefactDetailController != null && artefactDetailController.IsOpen)
        {
            return;
        }

        OnBackButtonClick();
    }
}