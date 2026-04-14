using System;
using Modules.SoundSystems;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public enum ProgressType
{
    None,
    Dust,
    Mud,
    Assemble
}

public enum SoundType
{
    Continuous,
    Once
}

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Image checkList;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI progressTitle;
    [SerializeField] private int maximum = 100;
    [SerializeField] private AudioKey audioKey;
    [SerializeField] private SoundType soundType;

    private int currentValue = 0;
    private int lastValue = -1;

    [SerializeField] private ProgressType progressType = ProgressType.None;
    public ProgressType ProgressType => progressType;

    public static event Action OnProgressBarCompleted;

    private void Awake()
    {
        checkList.enabled = false;
        progressText.text = currentValue.ToString();
    }

    private void Start()
    {
        UpdateFill();
    }

    public void SetValue(float value01)
    {
        if (value01 >= 0.99f)
            value01 = 1f;
        else
            value01 = Mathf.Clamp01(value01);

        currentValue = Mathf.RoundToInt(value01 * maximum);

        if (currentValue == 100 && lastValue != 100)
        {
            AudioEvents.TriggerPlayCustomSFX(audioKey);
            OnProgressBarCompleted?.Invoke();
        }

        lastValue = currentValue;

        progressText.text = currentValue.ToString();
        checkList.enabled = currentValue == 100;
        progressText.enabled = currentValue != 100;

        UpdateFill();
    }

    public void SetValue1(float value01)
    {
        float previousValue01 = (float)currentValue / maximum;

        if (value01 >= 0.99f) value01 = 1f;
        else value01 = Mathf.Clamp01(value01);

        currentValue = Mathf.RoundToInt(value01 * maximum);
        // HandleAudioLogic(value01, previousValue01);

        if (currentValue == 100 && lastValue != 100)
        {
            if (soundType == SoundType.Once)
            {
                AudioEvents.TriggerPlayCustomSFX(audioKey);
            }
            OnProgressBarCompleted?.Invoke();
        }

        lastValue = currentValue;
        progressText.text = currentValue.ToString();
        checkList.enabled = currentValue == 100;
        progressText.enabled = currentValue != 100;

        UpdateFill();
    }

    // private void HandleAudioLogic(float newValue, float oldValue)
    // {
    //     if (soundType != SoundType.Continuous) return;
    //     if (newValue > oldValue && newValue < 1f)
    //     {
    //         AudioEvents.TriggerPlayContinuousSFX(audioKey, true);
    //     }
    //     else
    //     {
    //         AudioEvents.TriggerPlayContinuousSFX(audioKey, false);
    //     }
    // }

    public void UpdateFill()
    {
        var progress = (float)currentValue / maximum;

        if (progress >= 0.99f)
            fill.fillAmount = 1f;
        else
            fill.fillAmount = progress;
    }

    public int GetValue()
    {
        return currentValue;
    }
}
