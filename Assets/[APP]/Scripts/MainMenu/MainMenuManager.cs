
using System;
using Modules;
using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    [Header("Sub-Controller")]
    [SerializeField] private MenuController menuController;
    [SerializeField] private LevelSelectionController levelSelectionController;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private QuitController quitController;
    [SerializeField] private BackgroundController backgroundController;

    [Header("Scene Configuration")]
    [SerializeField] private string splashSceneName = "Splash";

    private PlayerProgressionData playerProgressionData;
    private ActiveArtefactData activeArtefactData;
    private SceneLoader sceneLoader;
    private ProjectSavingSystem projectSavingSystem;

    private static bool isFirstSessionLoad = true;

    [Inject]
    public void Construct(
        PlayerProgressionData playerProgressionData, 
        ActiveArtefactData activeArtefactData, 
        SceneLoader sceneLoader,
        ProjectSavingSystem projectSavingSystem,
        IObjectResolver container)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.projectSavingSystem = projectSavingSystem;

        container.Inject(menuController);
        container.Inject(levelSelectionController);
        container.Inject(settingsController);
        container.Inject(quitController);
    }

    private void Awake()
    {
        MainMenuEvents.OnNewGame += OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame += OnRequestContinueGame;
        MainMenuEvents.OnCloseLevelSelection += OnRequestCloseLevelSelection;
        MainMenuEvents.OnCameraFocusToArtefact += OnRequestCameraFocusArtefact;
        MainMenuEvents.OnCloseArtefactDetail += OnRequestCloseArtefactDetail;
        MainMenuEvents.OnOpenSettings += OnRequestOpenSettings;
        MainMenuEvents.OnRequestQuit += OnRequestQuit;
    }

    private void Start()
    {
        bool hasPendingAnimations = 
            activeArtefactData.GetPendingCompletionAnimations().Count > 0 || 
            activeArtefactData.GetPendingUnlockAnimations().Count > 0;

        if (isFirstSessionLoad)
        {
            isFirstSessionLoad = false;
            AppLogger.Log("[MainMenuManager] App just started. Entering normal Main Menu.");
            
            menuController.SetActive(true);
        }
        else if (hasPendingAnimations)
        {
            AppLogger.Log("[MainMenuManager] Player returned from gameplay! Opening Level Selection immediately.");
            
            levelSelectionController.OpenLevelSelectionAndPlayAnimations();
        }
        else
        {
            AppLogger.Log("[MainMenuManager] Entering normal Main Menu.");
            
            menuController.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        MainMenuEvents.OnNewGame -= OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame -= OnRequestContinueGame;   
        MainMenuEvents.OnCloseLevelSelection -= OnRequestCloseLevelSelection;   
        MainMenuEvents.OnCameraFocusToArtefact -= OnRequestCameraFocusArtefact;  
        MainMenuEvents.OnCloseArtefactDetail -= OnRequestCloseArtefactDetail;
        MainMenuEvents.OnOpenSettings -= OnRequestOpenSettings;
        MainMenuEvents.OnRequestQuit -= OnRequestQuit;
    }

    private void OnRequestNewGameGame()
    {
        bool isReturningPlayer = playerProgressionData.HasPlayedBefore;

        if (isReturningPlayer)
        {
            AppLogger.Log("[MainMenuManager] Veteran player resetting game -> Reload Splash");

            playerProgressionData.ClearData();
            activeArtefactData.ResetData(); 
            
            projectSavingSystem.SaveAll();

            _ = sceneLoader.LoadSceneAsync(splashSceneName);
        }
        else
        {
            AppLogger.Log("[MainMenuManager] New player starting -> Level Selection");
            playerProgressionData.MarkAsPlayed();
            activeArtefactData.ResetData(); 
            projectSavingSystem.SaveAll();

            menuController.SetActive(false);
            levelSelectionController.OpenLevelSelection();
            MainMenuEvents.TriggerCameraToLevelSelection();
        }
    }

    private void OnRequestContinueGame()
    {
        menuController.SetActive(false);
        
        bool hasPendingAnimations = 
            activeArtefactData.GetPendingCompletionAnimations().Count > 0 || 
            activeArtefactData.GetPendingUnlockAnimations().Count > 0;

        if (hasPendingAnimations)
        {
            levelSelectionController.OpenLevelSelectionAndPlayAnimations();
        }
        else
        {
            levelSelectionController.OpenLevelSelection();
            MainMenuEvents.TriggerCameraToLevelSelection();
        }
    }

    private void OnRequestCloseLevelSelection()
    {
        menuController.SetActive(true);
        menuController.RefreshButtonVisibility();
    }

    private void OnRequestCameraFocusArtefact(Transform transform)
    {
        backgroundController.SetActive(true);
    }

    private void OnRequestCloseArtefactDetail()
    {
        backgroundController.SetActive(false);
    }

    private void OnRequestOpenSettings()
    {
        settingsController.SetActive(true);
    }

    private void OnRequestQuit()
    {
        quitController.SetActive(true);
    }
}