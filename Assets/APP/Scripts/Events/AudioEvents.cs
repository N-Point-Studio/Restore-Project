using System;
using Modules.SoundSystems;
using UnityEngine;

public static class AudioEvents
{
    public static event Action<bool> OnPlayButtonSFX;
    public static event Action OnPlayToggleSFX;
    public static event Action OnPlaySliderSFX;
    public static event Action OnPlayUIComponentSelectedSFX;
    public static event Action OnPlayBGMMainMenu;
    public static event Action<AudioKey> OnPlayBGMGameplay;
    public static event Action<AudioKey> OnPlayCustomSFX;
    public static event Action OnPlayAssembleSFX;
    public static event Action OnStopBGM;

    public static void TriggerPlayButtonSFX(bool isConfirm) => OnPlayButtonSFX?.Invoke(isConfirm);
    public static void TriggerPlayToggleSFX() => OnPlayToggleSFX?.Invoke();
    public static void TriggerPlaySliderSFX() => OnPlaySliderSFX?.Invoke();
    public static void TriggerPlayUIComponentSelectedSFX() => OnPlayUIComponentSelectedSFX?.Invoke();
    public static void TriggerPlayBGMMainMenu() => OnPlayBGMMainMenu?.Invoke();
    public static void TriggerPlayBGMGameplay(AudioKey audioKey) => OnPlayBGMGameplay?.Invoke(audioKey);
    public static void TriggerPlayCustomSFX(AudioKey audioKey) => OnPlayCustomSFX?.Invoke(audioKey);
    public static void TriggerPlayAssembleSFX() => OnPlayAssembleSFX?.Invoke();
    public static void TriggerStopBGM() => OnStopBGM?.Invoke();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnPlayButtonSFX = null;
        OnPlayToggleSFX = null;
        OnPlaySliderSFX = null;
        OnPlayUIComponentSelectedSFX = null;
        OnPlayBGMMainMenu = null;
        OnPlayBGMGameplay = null;
        OnPlayCustomSFX = null;
        OnPlayAssembleSFX = null;
        OnStopBGM = null;
    }
}