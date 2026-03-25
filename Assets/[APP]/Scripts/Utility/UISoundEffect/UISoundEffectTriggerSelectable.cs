using UnityEngine;

public class UISoundEffectTriggerSelectable : UISoundEffectTrigger
{
    [SerializeField] private bool isConfirm;

    public void PlayTriggerSound()
    {
        base.PlaySound();
        AudioEvents.TriggerPlayButtonSFX(isConfirm);
    }
}
