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

    [Header("Animation Settings")]
    [SerializeField] private float hideAnimDuration = 0.4f;

    [Header("Target Scene")]
    [SerializeField] private string targetScene;

    private ActiveArtefactData activeArtefactData;
    private PlayerProgressionData playerProgressionData;
    private SceneLoader sceneLoader;
    private InputSystemService input;

    private Artefact3DItem clickedItem;
    private bool isCameraMoving = false;

    [Inject]
    public void Construct(
        ActiveArtefactData activeArtefactData, 
        PlayerProgressionData playerProgressionData,
        SceneLoader sceneLoader,
        InputSystemService input        
        )
    {
        this.activeArtefactData = activeArtefactData;
        this.playerProgressionData = playerProgressionData;
        this.sceneLoader = sceneLoader;
        this.input = input;
        artefactDetailController.SetInputSystemService(this.input);
    }

    protected override void Awake()
    {
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
        _ = sceneLoader.LoadSceneAsync(targetScene, 2f, "Unboxing Artefact...");
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
                artefact3DItems[i].AnimateItem(true, 0.5f);
        }

        if (emptyPedestals != null)
        {
            for (int i = 0; i < emptyPedestals.Length; i++)
            {
                PedestalAnimator empty = emptyPedestals[i];
                if (empty != null) empty.AnimatePedestal(true, 0.5f);
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
        yield return new WaitForSeconds(0.5f);

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
                // 1. Suruh kamera maju ke Artefak
                isCameraMoving = true;
                MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                
                // 2. Tunggu sampai event Blend kamera selesai! (Tidak hardcode lagi)
                yield return new WaitUntil(() => !isCameraMoving);

                AppLogger.Log($"[Animation] Playing REVEAL animation for {data.artefactId}");
                targetItem.PlayCompletionAnimation();
                
                // Tunggu animasi partikel/ledakan selesai (ini tetap hardcode)
                yield return new WaitForSeconds(2.0f); 
                
                activeArtefactData.MarkCompletionAnimSeen(data.artefactId);

                ArtefactData aData = activeArtefactData.GetArtefactDatabase().GetItem(data.artefactId);
                if (aData != null) OpenArtefactDetailAutomatically(aData);
            }
            
            yield break; // Berhenti di sini, tunggu pemain close jurnal
        }

        yield return StartCoroutine(PlayPendingUnlocksSequence());
    }

    private IEnumerator PlayPendingUnlocksSequence()
    {
        List<ArtefactProgressData> pendingUnlocks = activeArtefactData.GetPendingUnlockAnimations();
        
        if (pendingUnlocks.Count > 0)
        {
            yield return new WaitForSeconds(0.3f); // Jeda kecil biar rapi setelah jurnal ditutup
            
            for (int i = 0; i < pendingUnlocks.Count; i++)
            {
                ArtefactProgressData data = pendingUnlocks[i];
                Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
                
                if (targetItem != null)
                {
                    // 1. Kamera pindah ke meja Box Baru
                    isCameraMoving = true;
                    MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                    
                    yield return new WaitUntil(() => !isCameraMoving);

                    AppLogger.Log($"[Animation] Playing UNLOCK animation for {data.artefactId}");
                    targetItem.PlayUnlockAnimation();
                    
                    // Tunggu animasi box jatuh
                    yield return new WaitForSeconds(1.5f); 
                }
                
                activeArtefactData.MarkUnlockAnimSeen(data.artefactId);
            }
            
            // Setelah semua box baru jatuh, kamera mundur ke Overview
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