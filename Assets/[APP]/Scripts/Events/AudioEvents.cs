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
    public static event Action OnPlayBGMGameplay;
    public static event Action<AudioKey> OnPlayCustomSFX;

    public static void TriggerPlayButtonSFX(bool isConfirm) => OnPlayButtonSFX?.Invoke(isConfirm);
    public static void TriggerPlayToggleSFX() => OnPlayToggleSFX?.Invoke();
    public static void TriggerPlaySliderSFX() => OnPlaySliderSFX?.Invoke();
    public static void TriggerPlayUIComponentSelectedSFX() => OnPlayUIComponentSelectedSFX?.Invoke();
    public static void TriggerPlayBGMMainMenu() => OnPlayBGMMainMenu?.Invoke();
    public static void TriggerPlayBGMGameplay() => OnPlayBGMGameplay?.Invoke();
    public static void TriggerPlayCustomSFX(AudioKey audioKey) => OnPlayCustomSFX?.Invoke(audioKey);

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
    }
}