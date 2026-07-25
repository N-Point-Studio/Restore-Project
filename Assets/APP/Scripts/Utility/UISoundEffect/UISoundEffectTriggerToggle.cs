using UnityEngine;
using UnityEngine.UI;
using Modules.SoundSystems;

[RequireComponent(typeof(Toggle))]
public class UISoundEffectTriggerToggle : UISoundEffectTrigger
{
    private bool lastToggleValue;
    private Toggle toggle;
    
    private bool isInitialized = false;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        lastToggleValue = toggle.isOn; // Cache immediately
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void Start()
    {
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
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
            AudioEvents.TriggerPlayToggleSFX();
        }
    }

    protected void OnToggleValueChanged(bool isOn)
    {
        if (!isInitialized)
        {
            lastToggleValue = isOn;
            return;
        }

        if (lastToggleValue != isOn)
        {
            lastToggleValue = isOn;
            PlaySound();
        }
    }
}