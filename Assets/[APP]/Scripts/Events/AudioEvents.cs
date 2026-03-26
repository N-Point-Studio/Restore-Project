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
    public static Action OnPlayBrushSFX;
    public static Action OnPlayChiselSFX;
    public static Action OnPlayAssembleSFX;

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

    public static void TriggerPlayBrushSFX()
    {
        OnPlayBrushSFX?.Invoke();
    }

    public static void TriggerPlayChiselSFX()
    {
        OnPlayChiselSFX?.Invoke();
    }

    public static void TriggerPlayAssembleSFX()
    {
        OnPlayAssembleSFX?.Invoke();
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
        OnPlayBrushSFX = null;
        OnPlayChiselSFX = null;
        OnPlayAssembleSFX = null;
    }
}