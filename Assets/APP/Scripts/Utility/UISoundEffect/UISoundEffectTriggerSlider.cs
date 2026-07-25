using UnityEngine;
using UnityEngine.UI;
using Modules.SoundSystems;

[RequireComponent(typeof(Slider))]
public class UISoundEffectTriggerSlider : UISoundEffectTrigger
{
    private float lastSliderValue;
    private Slider slider;
    
    private bool isInitialized = false; 

    private void Awake()
    {
        slider = GetComponent<Slider>();
        lastSliderValue = slider.value;
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void Start()
    {
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
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
            AudioEvents.TriggerPlaySliderSFX();
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isInitialized) 
        {
            lastSliderValue = value;
            return;
        }

        if (!Mathf.Approximately(lastSliderValue, value))
        {
            lastSliderValue = value;
            PlaySound();
        }
    }
}