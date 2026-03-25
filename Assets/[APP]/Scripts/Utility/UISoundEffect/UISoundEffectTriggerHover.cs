using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundEffectTriggerHover : UISoundEffectTrigger, ISelectHandler, IPointerEnterHandler
{
    protected override void PlaySound()
    {
        base.PlaySound();
        AudioEvents.TriggerPlayUIComponentSelectedSFX();
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
