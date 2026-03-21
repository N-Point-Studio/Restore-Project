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
    [SerializeField] private ButtonItemUI backButtonItemUI;

    [Header("3D Scene References")]
    [SerializeField] private Artefact3DItem[] artefact3DItems;

    [Header("Target Scene")]
    [SerializeField] private string targetScene;

    private ActiveArtefactData activeArtefactData;
    private PlayerProgressionData playerProgressionData;
    private SceneLoader sceneLoader;
    private Artefact3DItem clickedItem;

    [Inject]
    public void Construct(
        ActiveArtefactData activeArtefactData, 
        PlayerProgressionData playerProgressionData,
        SceneLoader sceneLoader)
    {
        this.activeArtefactData = activeArtefactData;
        this.playerProgressionData = playerProgressionData;
        this.sceneLoader = sceneLoader;
    }

    protected override void Awake()
    {
        base.Awake();

        MainMenuEvents.OnArtefactPlay += OnRequestArtefactPlay;
        MainMenuEvents.OnOpenArtefactDetail += OnRequestArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail += OnCloseArtefactDetail;
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
        backButtonItemUI.OnClick -= OnBackButtonClick;
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
        SetArtefactsInteractable(true);
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

    private IEnumerator PlayPendingAnimationsSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (artefact3DItems == null || artefact3DItems.Length == 0) yield break;

        List<ArtefactProgressData> pendingCompletions = activeArtefactData.GetPendingCompletionAnimations();
        for (int i = 0; i < pendingCompletions.Count; i++)
        {
            ArtefactProgressData data = pendingCompletions[i];
            Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
            
            if (targetItem != null)
            {
                MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                yield return new WaitForSeconds(1.0f);

                AppLogger.Log($"[Animation] Playing REVEAL animation for {data.artefactId}");
                
                // targetItem.PlayCompletionAnimation();
                
                yield return new WaitForSeconds(2.0f); 
            }
            
            activeArtefactData.MarkCompletionAnimSeen(data.artefactId);
        }

        if (pendingCompletions.Count > 0)
        {
            MainMenuEvents.TriggerCameraToLevelSelection();
            yield return new WaitForSeconds(1.0f);
        }

        List<ArtefactProgressData> pendingUnlocks = activeArtefactData.GetPendingUnlockAnimations();
        for (int i = 0; i < pendingUnlocks.Count; i++)
        {
            ArtefactProgressData data = pendingUnlocks[i];
            Artefact3DItem targetItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.artefactId);
            
            if (targetItem != null)
            {
                MainMenuEvents.TriggerCameraFocusToArtefact(targetItem.transform);
                yield return new WaitForSeconds(1.0f); 

                AppLogger.Log($"[Animation] Playing UNLOCK animation for {data.artefactId}");
                
                // targetItem.PlayUnlockAnimation();
                
                yield return new WaitForSeconds(1.5f); 
            }
            
            activeArtefactData.MarkUnlockAnimSeen(data.artefactId);
        }
        
        if (pendingUnlocks.Count > 0)
        {
            MainMenuEvents.TriggerCameraToLevelSelection();
        }

        RefreshUI();

        AppLogger.Log("[Animation] All animation sequences complete!");
    }

    private void OnRequestArtefactPlay(ArtefactData data)
    {
        playerProgressionData.SetCurrentActiveArtefact(data.BaseData.Id);
        _ = sceneLoader.LoadSceneAsync(targetScene, LoadingType.ProgressBar);
    }

    private void OnRequestArtefactDetail(ArtefactData data)
    {        
        backButtonItemUI.gameObject.SetActive(false);
        clickedItem = Array.Find(artefact3DItems, x => x != null && x.ArtefactId == data.BaseData.Id);

        if (clickedItem == null)
        {
            AppLogger.LogWarning($"[LevelSelection] Artefact3DItem with ID {data.BaseData.Id} was not found in the Scene!");
        }

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null && artefact3DItems[i] != clickedItem)
            {
                artefact3DItems[i].SetVisibility(false);
                artefact3DItems[i].ShowPedestal(false);
            }
        }

        artefactDetailController.OpenDetail(data, activeArtefactData, clickedItem);
    }

    private void OnCloseArtefactDetail()
    {
        backButtonItemUI.gameObject.SetActive(true);
        artefactDetailController.CloseDetail(); 
        MainMenuEvents.TriggerCameraToLevelSelection();

        for (int i = 0; i < artefact3DItems.Length; i++)
        {
            if (artefact3DItems[i] != null && artefact3DItems[i] != clickedItem)
            {
                artefact3DItems[i].SetVisibility(false);
                artefact3DItems[i].ShowPedestal(true);
            }
        }
    }

    private void OnBackButtonClick()
    {
        CloseLevelSelection();
        MainMenuEvents.TriggerCloseLevelSelection();
        MainMenuEvents.TriggerCameraToMainMenu();
    }
}