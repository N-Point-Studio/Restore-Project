
using System;
using Modules;
using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    [Header("Sub-Controller")]
    [SerializeField] private MenuController menuController;
    [SerializeField] private LevelSelectionController levelSelectionController;

    private PlayerProgressionData playerProgressionData;
    private ActiveArtefactData activeArtefactData;

    [Inject]
    public void Construct(
        PlayerProgressionData playerProgressionData, 
        ActiveArtefactData activeArtefactData, 
        IObjectResolver container)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;

        container.Inject(menuController);
        container.Inject(levelSelectionController);
    }

    private void Awake()
    {
        MainMenuEvents.OnNewGame += OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame += OnRequestContinueGame;
    }

    private void Start()
    {
        bool hasPendingAnimations = 
            activeArtefactData.GetPendingCompletionAnimations().Count > 0 || 
            activeArtefactData.GetPendingUnlockAnimations().Count > 0;

        if (hasPendingAnimations)
        {
            AppLogger.Log("[MainMenuManager] New player just finished gameplay! Opening Level Selection immediately.");
            
            menuController.SetActive(false);
            levelSelectionController.OpenLevelSelectionAndPlayAnimations();
        }
        else
        {
            AppLogger.Log("[MainMenuManager] Entering normal Main Menu.");
            
            menuController.SetActive(true);
            levelSelectionController.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        MainMenuEvents.OnNewGame -= OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame -= OnRequestContinueGame;        
    }

    private void OnRequestNewGameGame()
    {
        playerProgressionData.ClearData();
        activeArtefactData.ResetData(); 
        playerProgressionData.MarkAsPlayed();

        menuController.SetActive(false);
        levelSelectionController.OpenLevelSelection();
    }

    private void OnRequestContinueGame()
    {
        menuController.SetActive(false);
        levelSelectionController.OpenLevelSelection();
    }
}