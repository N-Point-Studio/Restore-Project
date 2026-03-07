using UnityEngine;

public class UISoundEffectTriggerSelectable : UISoundEffectTrigger
{
    [SerializeField] private bool isConfirm;

    public void PlayTriggerSound()
    {
        base.PlaySound();
        SoundEvents.OnPlayButtonSFX?.Invoke(isConfirm);
    }
}
