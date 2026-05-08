using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SettingsController : BaseMenuController
{
    [SerializeField] private ButtonItemUI backButton;
    [SerializeField] private Slider masterSoundSlider;
    [SerializeField] private Slider musicSoundSlider;
    [SerializeField] private Slider sfxSoundSlider;
    [SerializeField] private ButtonItemUI buttonResetSettings;

    private ActiveSettingsData activeSettingsData;
    private ProjectSavingSystem projectSavingSystem;
    public event Action OnSettingsClosed;    
    
    [Inject]
    public void Construct(
        ActiveSettingsData activeSettingsData,
        ProjectSavingSystem projectSavingSystem)
    {
        this.activeSettingsData = activeSettingsData;
        this.projectSavingSystem = projectSavingSystem;
    }

    protected override void Awake()
    {
        base.Awake();
        backButton.OnClick += OnBackButtonClick;
        masterSoundSlider.onValueChanged.AddListener(OnMasterSliderValue);
        musicSoundSlider.onValueChanged.AddListener(OnMusicSliderValue);
        sfxSoundSlider.onValueChanged.AddListener(OnSfxSliderValue);
        buttonResetSettings.OnClick += OnResetSettingsClicked;
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        backButton.OnClick -= OnBackButtonClick;
        masterSoundSlider.onValueChanged.RemoveListener(OnMasterSliderValue);
        musicSoundSlider.onValueChanged.RemoveListener(OnMusicSliderValue);
        sfxSoundSlider.onValueChanged.RemoveListener(OnSfxSliderValue);
        buttonResetSettings.OnClick -= OnResetSettingsClicked;
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);
        if (isActive)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (activeSettingsData?.CurrentSettingsData != null)
        {
            masterSoundSlider.SetValueWithoutNotify(activeSettingsData.CurrentSettingsData.MasterVolume);
            musicSoundSlider.SetValueWithoutNotify(activeSettingsData.CurrentSettingsData.BGMVolume);
            sfxSoundSlider.SetValueWithoutNotify(activeSettingsData.CurrentSettingsData.SFXVolume);
        }
    }

    private void OnBackButtonClick()
    {
        if (projectSavingSystem != null)
        {
            projectSavingSystem.SaveAll();
        }
        SetActive(false);

        OnSettingsClosed?.Invoke();
    }

    private void OnMasterSliderValue(float value)
    {
        if (activeSettingsData?.CurrentSettingsData != null)
        {
            activeSettingsData.CurrentSettingsData.Apply(ProjectSettingsAudioType.Master, value);
            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData);
        }
    }

    private void OnMusicSliderValue(float value)
    {
        if (activeSettingsData?.CurrentSettingsData != null)
        {
            activeSettingsData.CurrentSettingsData.Apply(ProjectSettingsAudioType.BGM, value);
            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData);
        }
    }

    private void OnSfxSliderValue(float value)
    {
        if (activeSettingsData?.CurrentSettingsData != null)
        {
            activeSettingsData.CurrentSettingsData.Apply(ProjectSettingsAudioType.SFX, value);
            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData);
        }
    }

    private void OnResetSettingsClicked()
    {
        if (activeSettingsData != null)
        {
            activeSettingsData.CopyFrom(activeSettingsData.DefaultSettingsData);
            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData);
            
            RefreshUI();
        }
    }
}