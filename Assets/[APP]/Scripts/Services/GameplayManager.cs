using System;
using System.Collections.Generic;
using DG.Tweening;
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

    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData, ActiveArtefactData activeArtefactData, SceneLoader sceneLoader)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
    }

    public void Initialize()
    {
        GameplayUIManager.OnGameFinished += HandleGameFinished;
    }

    public void Dispose()
    {
        GameplayUIManager.OnGameFinished -= HandleGameFinished;
    }

    private void HandleGameFinished()
    {
        throw new NotImplementedException();
    }

    // TODO: Mungkin ini di GameplayManager?
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
            AppLogger.Log($"[Manager] Artefact{currentArtefactId} has been completed");
        }
    }

    public void StartSceneTransition()
    {
        string targetScene = "MainMenu"; // Change to targetScene

        Debug.Log($"[Manager] Back to menu using SceneLoader...");

        _ = sceneLoader.LoadSceneAsync(targetScene, LoadingType.Home, 1.0f);
    }

    private void InitializeSession()
    {
        if (playerProgressionData != null && activeArtefactData != null)
        {
            string targetId = playerProgressionData.CurrentActiveArtefactId;
            if (!string.IsNullOrEmpty(targetId))
            {
                // artefactData = activeArtefactData.GetArtefactDatabase().GetArtefactData(targetId);
            }
            else
            {
                AppLogger.LogWarning("[GameLoader] Empty artefact ID!");
            }
        }
        // LoadArtefact();
    }
}
