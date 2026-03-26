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
    public Action OnGameStarted;

    private string targetScene = "MainMenu";

    [Inject]
    public GameplayManager(PlayerProgressionData playerProgressionData,
    ActiveArtefactData activeArtefactData,
    SceneLoader sceneLoader,
    InputSystemService inputSystemService,
    string targetScene)
    {
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.sceneLoader = sceneLoader;
        this.inputSystemService = inputSystemService;
        this.targetScene = targetScene;

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

    private void HandleGameFinished(bool isCompleted)
    {
        string loadingText = isCompleted ? "Saving Artefact..." : "Returning to Menu...";

        StartSceneTransition(loadingText);
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
        AppLogger.Log($"[Gameplay Manager] Back to menu using SceneLoader. Message: {loadingMessage}");

        _ = sceneLoader.LoadSceneAsync(targetScene, 2f, loadingMessage);
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
                var artefactFragments = artefactData.ArtefactFragmentDatas;
                foreach (var artefact in artefactFragments)
                {
                    Spawn(artefact.Prefab, artefact.SpawnTransform.Position, artefact.SpawnTransform.Rotation);
                }
            }
            else
            {
                AppLogger.LogWarning("[Gameplay Manager] Empty artefact ID!");
            }
        }
    }

    void Spawn(GameObject prefab, Vector3 position, Vector3 rotation)
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
