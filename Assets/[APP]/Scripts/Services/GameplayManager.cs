using System;
using Modules;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public class GameplayManager : IInitializable, IDisposable
{
    private readonly PlayerProgressionData playerProgressionData;
    private readonly SceneLoader sceneLoader;
    private ActiveArtefactData activeArtefactData;
    private InputSystemService inputSystemService;
    private ArtefactData artefactData;

    private const string TARGET_SCENE = "MainMenu";

    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData, ActiveArtefactData activeArtefactData, SceneLoader sceneLoader, InputSystemService inputSystemService)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.inputSystemService = inputSystemService;

        InitializeSession();
    }

    public void Initialize()
    {
        GameplayUIManager.OnGameFinished += HandleGameFinished;
        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    public void Dispose()
    {
        GameplayUIManager.OnGameFinished -= HandleGameFinished;
        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
    }

    private void HandleGameWrapped()
    {
        SaveObjectCompletion();
    }

    private void HandleGameFinished()
    {
        StartSceneTransition();
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

    public void StartSceneTransition()
    {
        AppLogger.Log($"[Gameplay Manager] Back to menu using SceneLoader...");

        _ = sceneLoader.LoadSceneAsync(TARGET_SCENE, 2f, "Displaying Artefact...");
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
                AppLogger.Log($"[Gameplay Manager] Artefact:{artefactData.BaseData.Id} Loaded!");
            }
            else
            {
                AppLogger.LogWarning("[Gameplay Manager] Empty artefact ID!");
            }
        }
    }
}
