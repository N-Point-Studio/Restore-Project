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
    private float soundCooldownDuration = 0.1f;
    private float bgmFadeDuration = 1f;

    void IInitializable.Initialize()
    {
        AudioEvents.OnPlayUIComponentSelectedSFX += HandleOnUIComponentSelected;
        AudioEvents.OnPlayButtonSFX += HandleOnOnPlayButtonSFX;
        AudioEvents.OnPlayToggleSFX += HandleOnOnPlayToggleSFX;
        AudioEvents.OnPlaySliderSFX += HandleOnOnPlaySliderSFX;
        AudioEvents.OnPlayBGMGameplay += HandleOnPlayBGMGameplay;
        AudioEvents.OnPlayBGMMainMenu += HandleOnPlayBGMMainMenu;
        AudioEvents.OnPlayAssembleSFX += HandleOnPlayAssembleSFX;
        AudioEvents.OnPlayCustomSFX += HandleOnPlayCustomSFX;
    }

    void IDisposable.Dispose()
    {
        AudioEvents.OnPlayUIComponentSelectedSFX -= HandleOnUIComponentSelected;
        AudioEvents.OnPlayButtonSFX -= HandleOnOnPlayButtonSFX;
        AudioEvents.OnPlayToggleSFX -= HandleOnOnPlayToggleSFX;
        AudioEvents.OnPlaySliderSFX -= HandleOnOnPlaySliderSFX;
        AudioEvents.OnPlayBGMGameplay -= HandleOnPlayBGMGameplay;
        AudioEvents.OnPlayBGMMainMenu -= HandleOnPlayBGMMainMenu;
        AudioEvents.OnPlayCustomSFX -= HandleOnPlayCustomSFX;
        AudioEvents.OnPlayAssembleSFX -= HandleOnPlayAssembleSFX;
    }

    void IStartable.Start()
    {
    }

    public void PlaySFX(AudioKey audioType)
    {
        if (audioType == AudioKey.UI_Click || audioType == AudioKey.UI_Cancel || audioType == AudioKey.UI_Hover)
        {
            float currentTime = Time.unscaledTime;

            // Check if the same sound type was played recently
            if (lastPlayedSoundType == audioType &&
                currentTime - lastPlayedSoundTime < soundCooldownDuration)
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
        PlaySFX(AudioKey.UI_Hover);
    }

    private void HandleOnOnPlayButtonSFX(bool isConfirm)
    {
        PlaySFX(isConfirm ? AudioKey.UI_Click : AudioKey.UI_Cancel);
    }

    private void HandleOnOnPlayToggleSFX()
    {
        PlaySFX(AudioKey.UI_Click);
    }

    private void HandleOnOnPlaySliderSFX()
    {
        PlaySFX(AudioKey.UI_Click);
    }

    private void HandleOnPlayBGMGameplay(AudioKey bgmKey)
    {
        try
        {
            AudioKey keyToPlay = bgmKey != AudioKey.None ? bgmKey : AudioKey.BGM_Game;

            if (soundSystem != null)
                soundSystem.PlayAudio(keyToPlay, volume: 1f, loop: true, fadeInSeconds: bgmFadeDuration, fadeOutSeconds: bgmFadeDuration);
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
                soundSystem.PlayAudio(AudioKey.BGM_Home, volume: 1f, loop: true, fadeInSeconds: bgmFadeDuration, fadeOutSeconds: bgmFadeDuration);
        }
        catch (Exception e)
        {
            AppLogger.LogWarning($"[ProjectAudioService] Failed to play BGM Main Menu: {e.Message}");
        }
    }

    private void HandleOnPlayAssembleSFX()
    {
        PlaySFX(AudioKey.SFX_Assemble);
    }

    private void HandleOnPlayCustomSFX(AudioKey audioKey)
    {
        PlaySFX(audioKey);
    }
}