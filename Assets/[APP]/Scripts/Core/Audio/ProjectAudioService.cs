using System;
using Modules;
using Modules.SoundSystems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectAudioService : IInitializable, IStartable, IDisposable
{
    private readonly SoundSystem soundSystem;

    [Inject]
    public ProjectAudioService(SoundSystem soundSystem)
    {
        this.soundSystem = soundSystem;
    }

    private AudioKey? lastPlayedSoundType;
    private float lastPlayedSoundTime;
    private const float SOUND_COOLDOWN_DURATION = 0.1f; // 100ms cooldown

    void IInitializable.Initialize()
    {
        AudioEvents.OnPlayUIComponentSelectedSFX += HandleOnUIComponentSelected;
        AudioEvents.OnPlayButtonSFX += HandleOnOnPlayButtonSFX;
        AudioEvents.OnPlayToggleSFX += HandleOnOnPlayToggleSFX;
        AudioEvents.OnPlaySliderSFX += HandleOnOnPlaySliderSFX;
        AudioEvents.OnPlayBGMGameplay += HandleOnPlayBGMGameplay;
        AudioEvents.OnPlayBGMMainMenu += HandleOnPlayBGMMainMenu;
        AudioEvents.OnPlayBrushSFX += HandleOnPlayBrushSFX;
        AudioEvents.OnPlayChiselSFX += HandleOnPlayChiselSFX;
        AudioEvents.OnPlayAssembleSFX += HandleOnPlayAssembleSFX;
    }

    void IDisposable.Dispose()
    {
        AudioEvents.OnPlayUIComponentSelectedSFX -= HandleOnUIComponentSelected;
        AudioEvents.OnPlayButtonSFX -= HandleOnOnPlayButtonSFX;
        AudioEvents.OnPlayToggleSFX -= HandleOnOnPlayToggleSFX;
        AudioEvents.OnPlaySliderSFX -= HandleOnOnPlaySliderSFX;
        AudioEvents.OnPlayBGMGameplay -= HandleOnPlayBGMGameplay;
        AudioEvents.OnPlayBGMMainMenu -= HandleOnPlayBGMMainMenu;
        AudioEvents.OnPlayBrushSFX -= HandleOnPlayBrushSFX;
        AudioEvents.OnPlayChiselSFX -= HandleOnPlayChiselSFX;
        AudioEvents.OnPlayAssembleSFX -= HandleOnPlayAssembleSFX;
    }

    void IStartable.Start()
    {
        HandleOnPlayBGMMainMenu();
    }

    public void PlaySFX(AudioKey audioType)
    {
        if (audioType == AudioKey.UI_Button_Click)// || audioType == AudioKey.UI_Cancel || audioType == AudioKey.UI_Hover)
        {
            float currentTime = Time.unscaledTime;

            // Check if the same sound type was played recently
            if (lastPlayedSoundType == audioType &&
                currentTime - lastPlayedSoundTime < SOUND_COOLDOWN_DURATION)
            {
                return; // Skip playing the sound to prevent stacking
            }

            // Update tracking variables
            lastPlayedSoundType = audioType;
            lastPlayedSoundTime = currentTime;
        }

        try
        {
            soundSystem.PlayAudio(audioType);
        }
        catch (Exception e)
        {
            AppLogger.LogWarning($"[ProjectAudioService] Failed to play SFX {audioType}: {e.Message}");
        }
    }

    private void HandleOnUIComponentSelected()
    {
        PlaySFX(AudioKey.UI_Button_Click);
        // PlaySFX(AudioKey.UI_Hover);
    }

    private void HandleOnOnPlayButtonSFX(bool isConfirm)
    {
        PlaySFX(isConfirm ? AudioKey.UI_Button_Click : AudioKey.UI_Button_Click);
        // PlaySFX(isConfirm ? AudioKey.UI_Button_Click : AudioKey.UI_Button_Cancel);
    }

    private void HandleOnOnPlayToggleSFX()
    {
        PlaySFX(AudioKey.UI_Button_Click);
    }

    private void HandleOnOnPlaySliderSFX()
    {
        PlaySFX(AudioKey.UI_Button_Click);
    }

    private void HandleOnPlayBGMGameplay()
    {
        try
        {
            if (soundSystem != null)
                soundSystem.PlayAudio(AudioKey.BGM_Game, volume: 1f, loop: true, fadeInSeconds: 1f, fadeOutSeconds: 1f);
        }
        catch (Exception e)
        {
            AppLogger.LogWarning($"[ProjectAudioService] Failed to play BGM Game: {e.Message}");
        }
    }

    private void HandleOnPlayBGMMainMenu()
    {
        try
        {
            if (soundSystem != null)
                soundSystem.PlayAudio(AudioKey.BGM_Home, volume: 1f, loop: true, fadeInSeconds: 1f, fadeOutSeconds: 1f);
        }
        catch (Exception e)
        {
            AppLogger.LogWarning($"[ProjectAudioService] Failed to play BGM Main Menu: {e.Message}");
        }
    }

    private void HandleOnPlayBrushSFX()
    {
        PlaySFX(AudioKey.SFX_Brush);
    }

    private void HandleOnPlayChiselSFX()
    {
        PlaySFX(AudioKey.SFX_Chisel);
    }

    private void HandleOnPlayAssembleSFX()
    {
        PlaySFX(AudioKey.SFX_Assemble);
    }
}