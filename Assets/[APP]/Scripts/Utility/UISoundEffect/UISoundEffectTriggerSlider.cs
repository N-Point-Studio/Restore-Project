using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISoundEffectTriggerSlider : UISoundEffectTrigger
{
    private float lastSliderValue;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    protected override void PlaySound()
    {
        base.PlaySound();
        AudioEvents.TriggerPlaySliderSFX();
    }

    private void OnSliderValueChanged(float value)
    {
        if (lastSliderValue != value)
        {
            PlaySound();
        }
    }
}
