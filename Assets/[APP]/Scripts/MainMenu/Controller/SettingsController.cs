using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SettingsController : BaseMenuController
{
    [SerializeField] private ButtonInputInstructionUI backButton;
    [SerializeField] private Slider masterSoundSlider;
    [SerializeField] private Slider musicSoundSlider;
    [SerializeField] private Slider sfxSoundSlider;
    [SerializeField] private Toggle crossHairToggle;

    private ActiveSettingsData activeSettingsData;
    
    [Inject]
    public void Construct(
        ActiveSettingsData activeSettingsData)
    {
        this.activeSettingsData = activeSettingsData;
    }

    protected override void Awake()
    {
        base.Awake();
        backButton.OnClick += OnBackButtonClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        backButton.OnClick -= OnBackButtonClick;
    }

    private void OnBackButtonClick()
    {
        SetActive(false);
    }
}
