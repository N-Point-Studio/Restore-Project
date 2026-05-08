using Modules.SoundSystems;
using UnityEngine.EventSystems;

public class UISoundEffectTriggerHover : UISoundEffectTrigger, ISelectHandler, IPointerEnterHandler
{
    protected override void PlaySound()
    {
        base.PlaySound();
        if (customKey != AudioKey.None)
        {
            AudioEvents.TriggerPlayCustomSFX(customKey);
        }
        else
        {
            AudioEvents.TriggerPlayUIComponentSelectedSFX();
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PlaySound();
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        PlaySound();
    }
}
