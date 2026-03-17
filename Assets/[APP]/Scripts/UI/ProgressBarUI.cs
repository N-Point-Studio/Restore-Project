using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ProgressType
{
    None,
    Dust,
    Mud,
    Assemble
}

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Image checkList;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI progressTitle;
    [SerializeField] private int minimum = 0;
    [SerializeField] private int maximum = 100;
    private int currentValue = 0;
    private int lastValue = -1;

    [SerializeField] private AudioSource AudioSourceProgress;
    [SerializeField] public AudioClip ProfressSound;

    public ProgressType progressType = ProgressType.None;

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
            if (AudioSourceProgress != null && ProfressSound != null)
            {
                AudioSourceProgress.PlayOneShot(ProfressSound);
                OnProgressBarCompleted?.Invoke();
            }
        }

        lastValue = currentValue;

        progressText.text = currentValue.ToString();
        checkList.enabled = currentValue == 100;
        progressText.enabled = currentValue != 100;

        UpdateFill();
    }

    public void UpdateFill()
    {
        var progress = (float)currentValue / maximum;

        if (progress >= 0.99f)
            fill.fillAmount = 1f;
        else
            fill.fillAmount = progress;

        Debug.Log("Progress " + progress);
    }

    public int GetValue()
    {
        return currentValue;
    }
}
