using System;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using VContainer;

public class LevelSelectionController : BaseMenuController
{
    [SerializeField] private SimpleScrollSnap scrollSnap;
    [SerializeField] private PaginationUI paginationUI; 
    [SerializeField] private ArtefactDetailController artefactDetailController;
    [SerializeField] private ScrollTargetZoomController scrollZoomController;
    [SerializeField] private ButtonItemUI backButtonItemUI;

    private int currentPanelIndex = 0;

    private ActiveArtefactData activeArtefactData;
    private ProjectSavingSystem savingSystem;

    [Inject]
    public void Construct(ActiveArtefactData activeArtefactData, ProjectSavingSystem savingSystem, IObjectResolver container)
    {
        this.activeArtefactData = activeArtefactData;
        this.savingSystem = savingSystem;
        this.savingSystem.ClearPreviousData();
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

    public void StartFTUE()
    {
        
    }

    public void OpenLevelSelection()
    {
        SetActive(true);
    }

    public void CloseLevelSelection()
    {
        SetActive(false);
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
                Debug.LogWarning($"Group index {i} is null or missing PagePrefab!");
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

    public void RemoveArtifactGroupItem(int index)
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

    private void OnPanelCentered(int newIndex, int oldIndex)
    {
        currentPanelIndex = newIndex;

        UpdateNavigationButtons();
    }

    private void OnRequestArtefactPlay(ArtefactData data)
    {
        // TODO: show Loading Service later and pass the data to playerprogressiondata to initizlie the ebject and start playing on gameplay scene
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
