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
    private ArtefactData artefactData;

    private const string TARGET_SCENE = "MainMenu";

    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData, ActiveArtefactData activeArtefactData, SceneLoader sceneLoader)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
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

        _ = sceneLoader.LoadSceneAsync(TARGET_SCENE, LoadingType.Camera, 1.0f);
    }

    // TODO: Call this at the first time playing of the timing not right (e.g: UI or sumthin not loaded properly)
    // do edit GameLoader and initialize from there and make Maybe GameplayEvents.OnArtefactInitialize maybe? then make UI and other service listen from it
    private void InitializeSession()
    {
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
        // LoadArtefact();
    }
}
