using System;
using UnityEngine;

public static class SoundEvents
{
    public static Action<bool> OnPlayButtonSFX;   
    public static Action OnPlayToggleSFX; 
    public static Action OnPlaySliderSFX;
    public static Action OnPlayUIComponentSelectedSFX;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnPlayButtonSFX = null;
        OnPlayToggleSFX = null;
        OnPlaySliderSFX = null;
        OnPlayUIComponentSelectedSFX = null;
    }
}