using Modules.SoundSystems;
using UnityEngine;
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
        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif
        if (isMobile) return;

        PlaySound();
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        if (eventData is PointerEventData) return;

        PlaySound();
    }
}