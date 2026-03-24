using System;
using Modules.SoundSystems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectSettingsService : IInitializable, IStartable, IDisposable
{
    private readonly ActiveSettingsData activeSettingsData;
    private readonly SoundSystem soundSystem;

    [Inject]
    public ProjectSettingsService(ActiveSettingsData activeSettingsData, SoundSystem soundSystem)
    {
        this.activeSettingsData = activeSettingsData;
        this.soundSystem = soundSystem;
    }

    void IInitializable.Initialize()
    {
        CoreEvents.OnSettingsChanged += ApplySettingsToEngine;
        activeSettingsData.OnSettingsDataLoaded += HandleOnSaveDataLoaded;
    }

    void IDisposable.Dispose()
    {
        CoreEvents.OnSettingsChanged -= ApplySettingsToEngine;
        activeSettingsData.OnSettingsDataLoaded -= HandleOnSaveDataLoaded;
    }

    void IStartable.Start()
    {
        ApplySettingsToEngine(activeSettingsData.CurrentSettingsData);
    }

    private void HandleOnSaveDataLoaded()
    {
        ApplySettingsToEngine(activeSettingsData.CurrentSettingsData);
    }

    private void ApplySettingsToEngine(ProjectSettingsData settingsData)
    {
        if (settingsData == null) return;

        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            if ((int)settingsData.GraphicMode == i)
            {
                QualitySettings.SetQualityLevel(i, true);
                break;
            }
        }

        if (soundSystem != null)
        {
            soundSystem.GlobalVolume = settingsData.MasterVolume;
            soundSystem.GlobalMusicVolume = settingsData.BGMVolume;
            
            soundSystem.GlobalSoundsVolume = settingsData.SFXVolume;
            soundSystem.GlobalUISoundsVolume = settingsData.SFXVolume;
            soundSystem.GlobalVoiceVolume = settingsData.SFXVolume;
        }
    }
}