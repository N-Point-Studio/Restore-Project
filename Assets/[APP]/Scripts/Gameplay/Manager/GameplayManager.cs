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

    private ArtefactData artefactData;
    public Action OnGameStarted;
    public bool isTutorialAvailable;
    public Vector3 finalRotation;
    private string targetScene = "MainMenu";

    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData,
    ActiveArtefactData activeArtefactData,
    SceneLoader sceneLoader,
    InputSystemService inputSystemService,
    string targetScene,
    TutorialService tutorialService,
    GameConfigData config)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.inputSystemService = inputSystemService;
        this.targetScene = targetScene;
        this.config = config;
        this.tutorialService = tutorialService;
        AppLogger.Log("tutorial service ada? " + tutorialService);

        InitializeSession();
    }

    public void Initialize()
    {
        GameplayUIManager.OnGameFinished += HandleGameFinished;
        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
        LoadingEvents.OnLoadingFinished += HandleLoadingFinished;
    }

    public void Dispose()
    {
        GameplayUIManager.OnGameFinished -= HandleGameFinished;
        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
        LoadingEvents.OnLoadingFinished -= HandleLoadingFinished;
    }


    private void HandleGameFinished(bool isCompleted)
    {
        string loadingText = isCompleted ? "Displaying Artefact..." : "Returning to Menu...";

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
        if (playerProgressionData == null || activeArtefactData == null)
        {
            AppLogger.LogWarning("Service Data not available - Cannot save data!");
            return;
        }

        string currentArtefactId = playerProgressionData.CurrentActiveArtefactId;

        if (!string.IsNullOrEmpty(currentArtefactId))
        {
            activeArtefactData.CompleteArtefact(currentArtefactId);
            AppLogger.Log($"[Gameplay Manager] Artefact{currentArtefactId} has been completed");
        }
    }

    public void StartSceneTransition(string loadingMessage)
    {
        Time.timeScale = 1f;

        AppLogger.Log($"[Gameplay Manager] Back to menu using SceneLoader. Message: {loadingMessage}");

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
                Debug.Log("final rotation: " + finalRotation);
                AppLogger.Log($"[Gameplay Manager] Artefact:{artefactData.BaseData.Id} Loaded!");


                List<ArtefactFragmentData> artefactFragments = artefactData.ArtefactFragmentDatas;
                for (int i = 0; i < artefactFragments.Count; i++)
                {
                    ArtefactFragmentData artefact = artefactFragments[i];
                    Spawn(artefact.Prefab, artefact.SpawnTransform.Position, artefact.SpawnTransform.Rotation);
                }

                isTutorialAvailable = artefactData.BaseData.Id == "Artefact_Coin";
            }
            else
            {
                AppLogger.LogWarning("[Gameplay Manager] Empty artefact ID!");
            }
        }
    }

    private void Spawn(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        if (prefab == null)
        {
            AppLogger.LogError("[Gameplay Manager] Prefab is NULL!");
            return;
        }

        Quaternion rot = Quaternion.Euler(rotation);

        GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rot);

        AppLogger.Log($"[Gameplay Manager] Spawned: {obj.name}");
    }
}
