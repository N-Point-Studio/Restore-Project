using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer;
using TMPro;
using System.Collections.Generic;

public class SettingsController : BaseMenuController
{
    [Header("Audio Settings")]
    [SerializeField] private ButtonItemUI backButton;
    [SerializeField] private Slider masterSoundSlider;
    [SerializeField] private Slider musicSoundSlider;
    [SerializeField] private Slider sfxSoundSlider;
    [SerializeField] private ButtonItemUI buttonResetSettings;

    [Header("Language Settings")]
    [SerializeField] private Button buttonNextLanguage;
    [SerializeField] private Button buttonPreviousLanguage;
    [SerializeField] private TMP_Text textCurrentLanguage;

    [Space(10)]
    [Tooltip("Map locale codes to custom display names here!")]
    [SerializeField] private List<LanguageDisplayMapping> customLanguageNames;

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

        if (buttonNextLanguage != null) buttonNextLanguage.onClick.AddListener(OnNextLanguageClick);
        if (buttonPreviousLanguage != null) buttonPreviousLanguage.onClick.AddListener(OnPreviousLanguageClick);
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();
        
        StartCoroutine(InitLocalizationRoutine());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        backButton.OnClick -= OnBackButtonClick;
        masterSoundSlider.onValueChanged.RemoveListener(OnMasterSliderValue);
        musicSoundSlider.onValueChanged.RemoveListener(OnMusicSliderValue);
        sfxSoundSlider.onValueChanged.RemoveListener(OnSfxSliderValue);
        buttonResetSettings.OnClick -= OnResetSettingsClicked;

        if (buttonNextLanguage != null) buttonNextLanguage.onClick.RemoveListener(OnNextLanguageClick);
        if (buttonPreviousLanguage != null) buttonPreviousLanguage.onClick.RemoveListener(OnPreviousLanguageClick);

        LocalizationSettings.SelectedLocaleChanged -= UpdateLanguageLabel;
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

    #region Language Logic
    
    private IEnumerator InitLocalizationRoutine()
    {
        yield return LocalizationSettings.InitializationOperation;
        string savedLangCode = activeSettingsData?.CurrentSettingsData?.LanguageCode;
        if (!string.IsNullOrEmpty(savedLangCode))
        {
            var localeToLoad = LocalizationSettings.AvailableLocales.GetLocale(new UnityEngine.Localization.LocaleIdentifier(savedLangCode));
            if (localeToLoad != null)
            {
                LocalizationSettings.SelectedLocale = localeToLoad;
            }
        }

        UpdateLanguageLabel(LocalizationSettings.SelectedLocale);
        LocalizationSettings.SelectedLocaleChanged += UpdateLanguageLabel;
    }

    private void OnNextLanguageClick()
    {
        ChangeLanguage(1);
    }

    private void OnPreviousLanguageClick()
    {
        ChangeLanguage(-1);
    }

    private void ChangeLanguage(int direction)
    {
        if (!LocalizationSettings.InitializationOperation.IsDone) return;

        var availableLocales = LocalizationSettings.AvailableLocales.Locales;
        var currentLocale = LocalizationSettings.SelectedLocale;
        int currentIndex = availableLocales.IndexOf(currentLocale);

        currentIndex += direction;
        if (currentIndex >= availableLocales.Count) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = availableLocales.Count - 1;

        Locale newLocale = availableLocales[currentIndex];
        LocalizationSettings.SelectedLocale = newLocale;
        
        if (activeSettingsData?.CurrentSettingsData != null)
        {
            activeSettingsData.CurrentSettingsData.Apply(newLocale.Identifier.Code); 
            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData); 
        }
    }

    private void UpdateLanguageLabel(Locale locale)
    {
        if (textCurrentLanguage != null && locale != null)
        {
            string code = locale.Identifier.Code;
            string finalDisplayName = "";

            if (customLanguageNames != null)
            {
                foreach (var mapping in customLanguageNames)
                {
                    if (mapping.localeCode == code)
                    {
                        finalDisplayName = mapping.customDisplayName;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(finalDisplayName))
            {
                finalDisplayName = locale.LocaleName;
                int parenthesisIndex = finalDisplayName.IndexOf('(');
                if (parenthesisIndex > 0)
                {
                    finalDisplayName = finalDisplayName.Substring(0, parenthesisIndex).Trim();
                }
            }

            textCurrentLanguage.text = finalDisplayName;
        }
    }
    
    #endregion

    #region Audio & Buttons Logic

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
            string defaultLangCode = activeSettingsData.CurrentSettingsData.LanguageCode;
            if (!string.IsNullOrEmpty(defaultLangCode) && LocalizationSettings.InitializationOperation.IsDone)
            {
                var localeToLoad = LocalizationSettings.AvailableLocales.GetLocale(new UnityEngine.Localization.LocaleIdentifier(defaultLangCode));
                if (localeToLoad != null)
                {
                    LocalizationSettings.SelectedLocale = localeToLoad; 
                }
            }

            CoreEvents.TriggerSettingsChange(activeSettingsData.CurrentSettingsData);
            
            RefreshUI();
        }
    }
    
    #endregion
}

[Serializable]
public struct LanguageDisplayMapping
{
    [Tooltip("The ISO code from Unity Localization, e.g., 'en' or 'id'")]
    public string localeCode;
    
    [Tooltip("What you want the text to actually say, e.g., 'English' or 'Bahasa Indonesia'")]
    public string customDisplayName;
}