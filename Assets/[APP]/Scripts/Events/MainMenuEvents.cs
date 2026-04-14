using System;
using UnityEngine;

public static class MainMenuEvents
{
    public static event Action<ArtefactData> OnOpenArtefactDetail;
    public static event Action<ArtefactData> OnArtefactPlay;
    public static event Action OnCloseArtefactDetail;
    public static event Action OnCloseLevelSelection;
    public static event Action OnNewGame;
    public static event Action OnContinueGame;
    public static event Action OnOpenSettings;
    public static event Action OnRequestQuit;

    // Camera
    public static event Action OnCameraToMainMenu;
    public static event Action OnCameraToLevelSelection;
    public static event Action<Transform> OnCameraFocusToArtefact;
    public static event Action OnCameraMoveFinished;
    public static event Action<bool> OnShowBackground;

    public static void TriggerOpenArtefactDetail(ArtefactData data) => OnOpenArtefactDetail?.Invoke(data);
    public static void TriggerArtefactPlay(ArtefactData artefactData) => OnArtefactPlay?.Invoke(artefactData);
    public static void TriggerCloseArtefactDetail() => OnCloseArtefactDetail?.Invoke();
    public static void TriggerCloseLevelSelection() => OnCloseLevelSelection?.Invoke();
    public static void TriggerNewGame() => OnNewGame?.Invoke();
    public static void TriggerContinueGame() => OnContinueGame?.Invoke();
    public static void TriggerOpenSettingsGame() => OnOpenSettings?.Invoke();
    public static void TriggerQuitGame() => OnRequestQuit?.Invoke();
    public static void TriggerCameraToMainMenu() => OnCameraToMainMenu?.Invoke();
    public static void TriggerCameraToLevelSelection() => OnCameraToLevelSelection?.Invoke();
    public static void TriggerCameraFocusToArtefact(Transform targetTransform) => OnCameraFocusToArtefact?.Invoke(targetTransform);
    public static void TriggerCameraMoveFinished() => OnCameraMoveFinished?.Invoke();
    public static void TriggerShowBackground(bool show) => OnShowBackground?.Invoke(show);
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnOpenArtefactDetail = null;
        OnArtefactPlay = null;
        OnCloseArtefactDetail = null;
        OnCloseLevelSelection = null;

        OnNewGame = null;
        OnContinueGame = null;
        OnOpenSettings = null;
        OnRequestQuit = null;
        OnCameraToMainMenu = null;
        OnCameraToLevelSelection = null;
        OnCameraFocusToArtefact = null;
        OnShowBackground = null;
    }
}
