using System;
using UnityEngine;

public static class MainMenuEvents
{
    public static Action<ArtefactData> OnOpenArtefactDetail;
    public static Action<ArtefactData> OnArtefactPlay;
    public static Action OnCloseArtefactDetail;
    public static Action OnCloseLevelSelection;
    public static Action OnNewGame;
    public static Action OnContinueGame;
    public static Action OnOpenSettings;
    public static Action OnRequestQuit;
    public static Action OnCameraToMainMenu;
    public static Action OnCameraToLevelSelection;
    public static Action<Transform> OnCameraFocusToArtefact;

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
    }
}
