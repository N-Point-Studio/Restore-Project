using UnityEngine;
using UnityEngine.UI;
using Modules.SoundSystems;

[RequireComponent(typeof(Toggle))]
public class UISoundEffectTriggerToggle : UISoundEffectTrigger
{
    private bool lastToggleValue;
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
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
        if (lastToggleValue && isOn)
        {
            return;
        }

        lastToggleValue = isOn;
        if (isOn)
        {
            PlaySound();
        }
    }
}
