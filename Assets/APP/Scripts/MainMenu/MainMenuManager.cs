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
    [SerializeField] private PopUpConfirmationController popUpConfirmationController;

    [Header("Scene Configuration")]
    [SerializeField] private string splashSceneName = "Splash";

    private PlayerProgressionData playerProgressionData;
    private ActiveArtefactData activeArtefactData;
    private SceneLoader sceneLoader;
    private ProjectSavingSystem projectSavingSystem;
    private InputSystemService input;

    private static bool isFirstSessionLoad = true;

    [Inject]
    public void Construct(
        PlayerProgressionData playerProgressionData, 
        ActiveArtefactData activeArtefactData, 
        SceneLoader sceneLoader,
        ProjectSavingSystem projectSavingSystem,
        IObjectResolver container,
        InputSystemService input)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.projectSavingSystem = projectSavingSystem;
        this.input = input;

        container.Inject(menuController);
        container.Inject(levelSelectionController);
        container.Inject(settingsController);
        container.Inject(quitController);

        input.ChangeInputState(InputStateType.UI);
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        
        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.ClearOverrideCursor();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);

        MainMenuEvents.OnNewGame += OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame += OnRequestContinueGame;
        MainMenuEvents.OnCloseLevelSelection += OnRequestCloseLevelSelection;
        MainMenuEvents.OnOpenArtefactDetail += OnRequestOpenArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail += OnRequestCloseArtefactDetail;
        MainMenuEvents.OnOpenSettings += OnRequestOpenSettings;
        MainMenuEvents.OnRequestQuit += OnRequestQuit;
        MainMenuEvents.OnShowBackground += OnRequestShowBackground;

        popUpConfirmationController.OnConfirm += OnConfirmNewGame;
        popUpConfirmationController.OnCancel += OnCancelNewGame;
        
        settingsController.OnSettingsClosed += OnSettingsClosed; 
        quitController.OnQuitCancelled += OnQuitCancelled;

        LoadingEvents.OnLoadingFinished += HandleLoadingFinished;
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
        MainMenuEvents.OnOpenArtefactDetail -= OnRequestOpenArtefactDetail;
        MainMenuEvents.OnCloseArtefactDetail -= OnRequestCloseArtefactDetail;
        MainMenuEvents.OnOpenSettings -= OnRequestOpenSettings;
        MainMenuEvents.OnRequestQuit -= OnRequestQuit;
        MainMenuEvents.OnShowBackground -= OnRequestShowBackground;

        popUpConfirmationController.OnConfirm -= OnConfirmNewGame;
        popUpConfirmationController.OnCancel -= OnCancelNewGame;
        
        settingsController.OnSettingsClosed -= OnSettingsClosed; 
        quitController.OnQuitCancelled -= OnQuitCancelled;

        LoadingEvents.OnLoadingFinished -= HandleLoadingFinished;
    }

    private void OnRequestNewGameGame()
    {
        bool isReturningPlayer = playerProgressionData.HasPlayedBefore;

        if (isReturningPlayer)
        {
            menuController.SetActive(false);
            popUpConfirmationController.SetActive(true);
        }
        else
        {
            AppLogger.Log("[MainMenuManager] New player starting -> Level Selection");
            playerProgressionData.MarkAsPlayed();
            activeArtefactData.ResetData(); 
            projectSavingSystem.SaveAll();

            menuController.SetActive(false);

            levelSelectionController.OpenLevelSelectionAndPlayAnimations();
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

    private void OnRequestOpenArtefactDetail(ArtefactData data)
    {
        backgroundController.SetActive(true);
    }

    private void OnRequestCloseArtefactDetail()
    {
        backgroundController.SetActive(false);
    }

    private void OnRequestOpenSettings()
    {
        menuController.SetActive(false);
        settingsController.SetActive(true);
    }

    private void OnRequestQuit()
    {
        menuController.SetActive(false);
        quitController.SetActive(true);
    }

    private void OnRequestShowBackground(bool show)
    {
        backgroundController.SetActive(show);
    }

    private void OnConfirmNewGame()
    {
        AppLogger.Log("[MainMenuManager] Veteran player resetting game -> Reload Splash");
        playerProgressionData.ClearData();
        activeArtefactData.ResetData(); 
        
        projectSavingSystem.SaveAll();

        isFirstSessionLoad = true;

        _ = sceneLoader.LoadSceneAsync(splashSceneName);
    }

    private void OnCancelNewGame()
    {
        popUpConfirmationController.SetActive(false);
        menuController.SetActive(true);
    }

    private void OnSettingsClosed()
    {
        menuController.SetActive(true);
    }

    private void OnQuitCancelled()
    {
        menuController.SetActive(true);
    }

    private void HandleLoadingFinished()
    {
        AudioEvents.TriggerPlayBGMMainMenu();
    }
}