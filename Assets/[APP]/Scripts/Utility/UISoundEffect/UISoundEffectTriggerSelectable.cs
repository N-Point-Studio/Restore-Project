using Modules.SoundSystems;
using UnityEngine;

public class UISoundEffectTriggerSelectable : UISoundEffectTrigger
{
    [SerializeField] private bool isConfirm;

    public void PlayTriggerSound()
    {
        base.PlaySound();
        if (customKey != AudioKey.None)
        {
            AudioEvents.TriggerPlayCustomSFX(customKey);
        }
        else
        {
            AudioEvents.TriggerPlayButtonSFX(isConfirm);
        }
    }
}
