using System;
using Modules.SoundSystems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectSettingsService : IInitializable, IStartable, IDisposable
{
    protected readonly ActiveSettingsData activeSettingsData;
    protected readonly SoundSystem soundSystem;

    [Inject]
    public ProjectSettingsService(ActiveSettingsData activeSettingsData, SoundSystem soundSystem)
    {
        this.activeSettingsData = activeSettingsData;
        this.soundSystem = soundSystem;
    }

    private ProjectSettingsData defaultSettingsData;
    public ProjectSettingsData DefaultSettingsData => defaultSettingsData;

    private ProjectSettingsData currentSettingsData;
    public ProjectSettingsData CurrentSettingsData => currentSettingsData;

    public float masterVolume
    {
        get { return soundSystem != null ? soundSystem.GlobalVolume : 1f; }
        set { if (soundSystem != null) soundSystem.GlobalVolume = value; }
    }
    public float bgmVolume
    {
        get { return soundSystem != null ? soundSystem.GlobalMusicVolume : 1f; }
        set { if (soundSystem != null) soundSystem.GlobalMusicVolume = value; }
    }
    public float sfxVolume
    {
        get { return soundSystem != null ? soundSystem.GlobalSoundsVolume : 1f; }
        set { if (soundSystem != null) soundSystem.GlobalSoundsVolume = soundSystem.GlobalUISoundsVolume = soundSystem.GlobalVoiceVolume = value; } 
    }

    void IInitializable.Initialize()
    {
        CoreEvents.OnSettingsChanged += HandleOnSettingsChanged;
        activeSettingsData.OnSettingsDataLoaded += HandleOnActiveSettingsChanged;
    }

    void IStartable.Start()
    {
        defaultSettingsData = UnityEngine.Object.Instantiate(activeSettingsData.DefaultSettingsData);
        currentSettingsData = UnityEngine.Object.Instantiate(activeSettingsData.CurrentSettingsData);

        HandleOnSettingsChanged(currentSettingsData);
    }

    void IDisposable.Dispose()
    {
        CoreEvents.OnSettingsChanged -= HandleOnSettingsChanged;
        activeSettingsData.OnSettingsDataLoaded -= HandleOnActiveSettingsChanged;
    }

    private void HandleOnSettingsChanged(ProjectSettingsData settingsData)
    {
        currentSettingsData = settingsData;
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            if ((int)settingsData.GraphicMode == i)
            {
                QualitySettings.SetQualityLevel(i, true);
            }
        }

        masterVolume = settingsData.MasterVolume;
        bgmVolume = settingsData.BGMVolume;
        sfxVolume = settingsData.SFXVolume;

        activeSettingsData.UpdateSettingsData(currentSettingsData);
    }

    private void HandleOnActiveSettingsChanged()
    {
        if (defaultSettingsData != null) UnityEngine.Object.Destroy(defaultSettingsData);
        if (currentSettingsData != null) UnityEngine.Object.Destroy(currentSettingsData);

        defaultSettingsData = UnityEngine.Object.Instantiate(activeSettingsData.DefaultSettingsData);
        currentSettingsData = UnityEngine.Object.Instantiate(activeSettingsData.CurrentSettingsData);

        HandleOnSettingsChanged(currentSettingsData);
    }
}