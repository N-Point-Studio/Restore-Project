using System;
using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using Modules;
using UnityEngine;
using VContainer;

public class LevelSelectionController : BaseMenuController
{
    [SerializeField] private SimpleScrollSnap scrollSnap;
    [SerializeField] private PaginationUI paginationUI; 
    [SerializeField] private ArtefactDetailController artefactDetailController;
    [SerializeField] private ScrollTargetZoomController scrollZoomController;
    [SerializeField] private ButtonItemUI backButtonItemUI;

    [Header("Target Scene")]
    [SerializeField] private string targetScene;

    private int currentPanelIndex = 0;

    private ActiveArtefactData activeArtefactData;
    private ProjectSavingSystem savingSystem;
    private PlayerProgressionData playerProgressionData;
    private SceneLoader sceneLoader;

    [Inject]
    public void Construct(
        ActiveArtefactData activeArtefactData, 
        ProjectSavingSystem savingSystem, 
        PlayerProgressionData playerProgressionData,
        SceneLoader sceneLoader)
    {
        this.activeArtefactData = activeArtefactData;
        this.savingSystem = savingSystem;
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

        scrollSnap.OnPanelCentered.AddListener(OnPanelCentered);

        SpawnArtefactGroups();
    }

    protected override  void OnDestroy()
    {
        base.OnDestroy();

        if (scrollSnap) scrollSnap.OnPanelCentered.RemoveListener(OnPanelCentered);

        MainMenuEvents.OnArtefactPlay -= OnRequestArtefactPlay;
        MainMenuEvents.OnOpenArtefactDetail -= OnRequestArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail -= OnCloseArtefactDetail;
        backButtonItemUI.OnClick -= OnBackButtonClick;
    }

    public void OpenLevelSelection()
    {
        SetActive(true);
    }

    public void CloseLevelSelection()
    {
        SetActive(false);
    }
    
    public void OpenLevelSelectionAndPlayAnimations()
    {
        SetActive(true);
        
        StartCoroutine(PlayPendingAnimationsSequence());
    }

    private void SpawnArtefactGroups()
    {
        ArtefactDatabase database = activeArtefactData.GetArtefactDatabase();
        if (database == null) return;

        ArtefactGroupData[] allGroups = database.GetAllItems();

        if (paginationUI) paginationUI.ClearAll(); 

        for (int i = 0; i < allGroups.Length; i++)
        {
            ArtefactGroupData group = allGroups[i];

            if (group == null || group.PagePrefab == null)
            {
                AppLogger.LogWarning($"Group index {i} is null or missing PagePrefab!");
                continue;
            }

            AddArtifactGroupItem(group, i);
        }

        OnPanelCentered(0, 0);
    }

    private void AddArtifactGroupItem(ArtefactGroupData groupData, int index)
    {
        if (paginationUI != null)
        {
            paginationUI.AddIndicator();
        }

        scrollSnap.Add(groupData.PagePrefab.gameObject, index);

        if (index < scrollSnap.Panels.Length)
        {
            RectTransform panelInstance = scrollSnap.Panels[index];
            if (panelInstance != null)
            {
                ArtefactGroupItemUI groupUI = panelInstance.GetComponent<ArtefactGroupItemUI>();
                if (groupUI != null)
                {
                    groupUI.Initialize(groupData, activeArtefactData);
                }
            }
        }
    }

    private void RemoveArtifactGroupItem(int index)
    {
        if (scrollSnap.NumberOfPanels > 0)
        {
            if (paginationUI != null)
            {
                paginationUI.RemoveIndicator(index);
            }

            scrollSnap.Remove(index);
            
            int safeIndex = Mathf.Clamp(currentPanelIndex, 0, scrollSnap.NumberOfPanels - 1);
            OnPanelCentered(safeIndex, safeIndex);
        }
    }

    private void UpdateNavigationButtons()
    {
        if (scrollSnap.NumberOfPanels == 0)
        {
            if (scrollSnap.PreviousButton) scrollSnap.PreviousButton.gameObject.SetActive(false);
            if (scrollSnap.NextButton) scrollSnap.NextButton.gameObject.SetActive(false);
            return;
        }
        
        if (scrollSnap.PreviousButton != null)
        {
            bool shouldShow = currentPanelIndex > 0;
            if (scrollSnap.PreviousButton.gameObject.activeSelf != shouldShow)
                scrollSnap.PreviousButton.gameObject.SetActive(shouldShow);
        }

        if (scrollSnap.NextButton != null)
        {
            bool shouldShow = currentPanelIndex < scrollSnap.NumberOfPanels - 1;
            if (scrollSnap.NextButton.gameObject.activeSelf != shouldShow)
                scrollSnap.NextButton.gameObject.SetActive(shouldShow);
        }
    }

    private IEnumerator PlayPendingAnimationsSequence()
    {
        yield return new WaitForSeconds(0.5f);

        List<ArtefactProgressData> pendingCompletions = activeArtefactData.GetPendingCompletionAnimations();

        for (int i = 0; i < pendingCompletions.Count; i++)
        {
            ArtefactProgressData data = pendingCompletions[i];
            // Optional: scroll the snap to the panel where this artefact is located
            ScrollToArtefactGroup(data.artefactId);
            yield return new WaitForSeconds(0.5f);

            AppLogger.Log($"[Animation] Playing REVEAL animation for {data.artefactId}");
            
            // TODO: Trigger particle/visual animation on the UI item here
            
            yield return new WaitForSeconds(2.0f); 
            
            activeArtefactData.MarkCompletionAnimSeen(data.artefactId);
        }

        List<ArtefactProgressData> pendingUnlocks = activeArtefactData.GetPendingUnlockAnimations();

        for (int i = 0; i < pendingUnlocks.Count; i++)
        {
            ArtefactProgressData data = pendingUnlocks[i];
            // Optional: scroll the snap to the panel of the newly unlocked artefact
            ScrollToArtefactGroup(data.artefactId);
            yield return new WaitForSeconds(0.5f);

            AppLogger.Log($"[Animation] Playing UNLOCK animation for {data.artefactId}");
            
            // TODO: Trigger lock-break/shake animation here
            
            yield return new WaitForSeconds(1.5f); 
            
            activeArtefactData.MarkUnlockAnimSeen(data.artefactId);
        }
        
        AppLogger.Log("[Animation] All animation sequences complete!");
    }

    private void ScrollToArtefactGroup(string artefactId)
    {
        if (activeArtefactData == null || scrollSnap == null) return;

        ArtefactDatabase database = activeArtefactData.GetArtefactDatabase();
        if (database == null) return;

        ArtefactGroupData[] allGroups = database.GetAllItems();
        
        for (int i = 0; i < allGroups.Length; i++)
        {
            ArtefactGroupData group = allGroups[i];
            if (group == null || group.ArtefactDatas == null) continue;

            foreach (ArtefactData artefact in group.ArtefactDatas)
            {
                if (artefact.BaseData.Id == artefactId)
                {
                    if (currentPanelIndex != i)
                    {
                        AppLogger.Log($"[LevelSelection] Automatically scrolling to page {i} for artefact {artefactId}");
                        scrollSnap.GoToPanel(i);
                    }
                    return;
                }
            }
        }
    }

    private void OnPanelCentered(int newIndex, int oldIndex)
    {
        currentPanelIndex = newIndex;

        UpdateNavigationButtons();
    }

    private void OnRequestArtefactPlay(ArtefactData data)
    {
        playerProgressionData.SetCurrentActiveArtefact(data.BaseData.Id);

        _ = sceneLoader.LoadSceneAsync(targetScene, LoadingType.Music); // TODO: REVISIT
    }

    private void OnRequestArtefactDetail(ArtefactItemUI itemUI, ArtefactGroupData groupData)
    {        
        bool isCompleted = activeArtefactData.IsArtefactCompleted(itemUI.ArtefactData.BaseData.Id);
        Sprite bgSprite = groupData.BaseData.ItemIcon;
        RectTransform sourceRect = itemUI.TargetIconRect;

        artefactDetailController.OpenDetail(itemUI.ArtefactData, bgSprite, isCompleted); 
        artefactDetailController.HideContentInstant();

        RectTransform targetRef = null;
        if (isCompleted)
        {
            targetRef = artefactDetailController.TargetIconRectRestore;
        }
        else
        {
            targetRef = artefactDetailController.TargetIconRectUnrestore;
        }

        scrollZoomController.ZoomToMatchTarget(sourceRect, targetRef, () => 
        {
            artefactDetailController.ShowContentFadeIn(); 
        });
    }

    private void OnCloseArtefactDetail()
    {
        artefactDetailController.CloseDetail(); 
        
        scrollZoomController.ZoomOut();
    }

    private void OnBackButtonClick()
    {
        CloseLevelSelection();
    }
}
