using System;
using System.Collections.Generic;
using Modules;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public class GameplayManager : IInitializable, IDisposable
{
    private readonly PlayerProgressionData playerProgressionData;
    private readonly SceneLoader sceneLoader;
    private readonly ActiveArtefactData activeArtefactData;
    private readonly InputSystemService inputSystemService;
    private readonly GameConfigData config;
    private readonly TutorialService tutorialService;
    private readonly Light directionalLight;
    private readonly GameplayToolManager gameplayToolManager;

    private ArtefactData artefactData;
    public bool isTutorialAvailable;
    public Vector3 finalRotation;
    public float clueTreshold;
    private string targetScene = "MainMenu";
    public float overallProgress;


    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData,
    ActiveArtefactData activeArtefactData,
    SceneLoader sceneLoader,
    InputSystemService inputSystemService,
    string targetScene,
    TutorialService tutorialService,
    Light directionalLight,
    GameConfigData config,
    GameplayToolManager gameplayToolManager)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.inputSystemService = inputSystemService;
        this.targetScene = targetScene;
        this.config = config;
        this.tutorialService = tutorialService;
        this.directionalLight = directionalLight;
        this.gameplayToolManager = gameplayToolManager;

        InitializeSession();
    }

    public void Initialize()
    {
        GameplayUIManager.OnGameFinished += HandleGameFinished;
        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
        LoadingEvents.OnLoadingFinished += HandleLoadingFinished;
        InteractionEvents.OnTabPerformed += HandleTabPerformed;
        InteractionEvents.OnTabCanceled += HandleTabCanceled;

        GameplayUIManager.OnOverallProgressUpdated += HandleOveralProgressUpdated;
    }

    public void Dispose()
    {
        GameplayUIManager.OnGameFinished -= HandleGameFinished;
        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
        LoadingEvents.OnLoadingFinished -= HandleLoadingFinished;
        InteractionEvents.OnTabPerformed -= HandleTabPerformed;
        InteractionEvents.OnTabCanceled -= HandleTabCanceled;

        GameplayUIManager.OnOverallProgressUpdated -= HandleOveralProgressUpdated;
    }

    private void HandleOveralProgressUpdated(float overall)
    {
        overallProgress = overall;
    }

    private void HandleTabPerformed()
    {
        // AppLogger.Log("Tab nyala" + overallProgress + " < " + GameplayUIManager.clueEnableTreshold);
        if (overallProgress >= clueTreshold)
        {
            // AppLogger.Log("[HARUSNYA] Tab nyala");

            directionalLight.enabled = false;
            if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 0)
            {
                tutorialService.CompleteAndAdvance(false);
            }
        }
    }

    private void HandleTabCanceled()
    {
        if (overallProgress >= clueTreshold) directionalLight.enabled = true;
    }


    private void HandleGameFinished(bool isCompleted)
    {
        string loadingText = isCompleted
            ? config.displayArtefactString.GetLocalizedString()
            : config.returnToMenuString.GetLocalizedString();

        StartSceneTransition(loadingText);
    }

    private void HandleGameWrapped()
    {
        SaveObjectCompletion();
    }

    private void HandleLoadingFinished()
    {
        if (artefactData != null)
        {
            AudioEvents.TriggerPlayBGMGameplay(artefactData.CustomGameplayBGM);
        }
    }

    private void SaveObjectCompletion()
    {
        if (playerProgressionData == null || activeArtefactData == null) return;

        string currentArtefactId = playerProgressionData.CurrentActiveArtefactId;

        if (!string.IsNullOrEmpty(currentArtefactId))
        {
            activeArtefactData.CompleteArtefact(currentArtefactId);
        }
    }

    public void StartSceneTransition(string loadingMessage)
    {
        Time.timeScale = 1f;

        CursorController.instance?.UnlockCursorState();
        CursorController.instance?.ClearOverrideCursor();
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);

        _ = sceneLoader.LoadSceneAsync(targetScene, config.minLoadingScreenDuration, loadingMessage);
    }

    private void InitializeSession()
    {
        if (inputSystemService != null)
        {
            inputSystemService.ChangeInputState(InputStateType.Player);
        }

        if (playerProgressionData != null && activeArtefactData != null)
        {
            string targetId = playerProgressionData.CurrentActiveArtefactId;
            if (!string.IsNullOrEmpty(targetId))
            {
                artefactData = activeArtefactData.GetArtefactDatabase().GetItem(targetId);
                finalRotation = artefactData.FinalRotation;
                clueTreshold = artefactData.ClueTreshold;
                var ToolType = artefactData.AllowedToolTypes;
                // clueTreshold 

                List<ArtefactFragmentData> artefactFragments = artefactData.ArtefactFragmentDatas;
                for (int i = 0; i < artefactFragments.Count; i++)
                {
                    ArtefactFragmentData artefact = artefactFragments[i];
                    Spawn(artefact.Prefab, artefact.SpawnTransform.Position, artefact.SpawnTransform.Rotation);
                }

                gameplayToolManager.SetToolVisibility(ToolType, true);

                isTutorialAvailable = CheckTutorialAvailability(1);
            }
        }
    }

    private void Spawn(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        if (prefab == null) return;
        Quaternion rot = Quaternion.Euler(rotation);
        GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rot);
    }

    public bool CheckTutorialAvailability(int whichTutorial)
    {
        string targetId = playerProgressionData.CurrentActiveArtefactId;
        artefactData = activeArtefactData.GetArtefactDatabase().GetItem(targetId);
        bool tutorialAvailable = false;

        switch (whichTutorial)
        {
            case 1:
                tutorialAvailable = artefactData.BaseData.Id == config.tutorialItemId;
                break;
            case 2:
                tutorialAvailable = artefactData.BaseData.Id == config.tutorialSecondItemId;
                break;
        }

        return tutorialAvailable;
    }
}