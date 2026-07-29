using Modules.SoundSystems;
using UnityEngine;

public class UISoundEffectTriggerSelectable : UISoundEffectTrigger
{
    [SerializeField] private bool isConfirm = true;

    public void PlayTriggerSound()
    {
        PlaySound();
    }

    protected override void PlaySound()
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