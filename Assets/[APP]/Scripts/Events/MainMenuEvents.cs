using System;
using UnityEngine;

public static class MainMenuEvents
{
    public static Action<ArtefactItemUI, ArtefactGroupData> OnOpenArtefactDetail;
    public static Action<ArtefactData> OnArtefactPlay;
    public static Action OnCloseArtefactDetail;
    public static Action OnOpenLevelSelection;
    public static Action OnCloseLevelSelection;
    public static Action OnNewGame;
    public static Action OnContinueGame;
    public static Action OnOpenSettingsGame;
    public static Action OnExitGame;

    //NORMAL Flow: Menu Selection (Continue Button) => Level Selection => Artefact Detail
    //FTUE Flow: Menu Selection (New Game) => 
    // Animate Level Selection Click FTUE Artefact => Open Artefact Detail that set for ftue
    // FTUE DISABLE BACK BUTTON

    public static void TriggerOpenArtefactDetail(ArtefactItemUI artefactItemUI, ArtefactGroupData artefactGroupData)
    {
        OnOpenArtefactDetail?.Invoke(artefactItemUI, artefactGroupData);
    }

    public static void TriggerArtefactPlay(ArtefactData artefactData)
    {
        OnArtefactPlay?.Invoke(artefactData);
    }

    public static void TriggerCloseArtefactDetail()
    {
        OnCloseArtefactDetail?.Invoke();
    }

    public static void TriggerOpenLevelSelection()
    {
        OnOpenLevelSelection?.Invoke();
    }

    public static void TriggerCloseLevelSelection()
    {
        OnCloseLevelSelection?.Invoke();
    }

    public static void TriggerNewGame()
    {
        OnNewGame?.Invoke();
    }

    public static void TriggerContinueGame()
    {
        OnContinueGame?.Invoke();
    }

    public static void TriggerOpenSettingsGame()
    {
        OnOpenSettingsGame?.Invoke();
    }

    public static void TriggerExitGame()
    {
        OnExitGame?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnOpenArtefactDetail = null;
        OnArtefactPlay = null;
        OnCloseArtefactDetail = null;

        OnOpenLevelSelection = null;
        OnCloseLevelSelection = null;

        OnNewGame = null;
        OnContinueGame = null;
        OnOpenSettingsGame = null;
        OnExitGame = null;
    }
}

public enum MainMenuState
{
    Default,
    MenuSelection,
    LevelSelection,
    ArtefactDetail
}
