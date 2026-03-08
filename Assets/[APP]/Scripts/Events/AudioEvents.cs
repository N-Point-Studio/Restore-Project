using System;
using UnityEngine;

public static class AudioEvents
{
    public static Action<bool> OnPlayButtonSFX;   
    public static Action OnPlayToggleSFX; 
    public static Action OnPlaySliderSFX;
    public static Action OnPlayUIComponentSelectedSFX;
    public static Action OnPlayBGMMainMenu;
    public static Action OnPlayBGMGameplay;

    public static void TriggerPlayButtonSFX(bool isConfirm)
    {
        OnPlayButtonSFX?.Invoke(isConfirm);
    }

    public static void TriggerPlayToggleSFX()
    {
        OnPlayToggleSFX?.Invoke();
    }

    public static void TriggerPlaySliderSFX()
    {
        OnPlaySliderSFX?.Invoke();
    }

    public static void TriggerPlayUIComponentSelectedSFX()
    {
        OnPlayUIComponentSelectedSFX?.Invoke();
    }

    public static void TriggerPlayBGMMainMenu()
    {
        OnPlayBGMMainMenu?.Invoke();
    }

    public static void TriggerPlayBGMGameplay()
    {
        OnPlayBGMGameplay?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnPlayButtonSFX = null;
        OnPlayToggleSFX = null;
        OnPlaySliderSFX = null;
        OnPlayUIComponentSelectedSFX = null;
        OnPlayBGMMainMenu = null;
        OnPlayBGMGameplay = null;
    }
}