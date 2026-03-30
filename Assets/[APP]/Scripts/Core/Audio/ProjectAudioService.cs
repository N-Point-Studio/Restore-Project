using System;
using Modules;
using Modules.SoundSystems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectAudioService : IInitializable, IStartable, IDisposable
{
    private readonly SoundSystem soundSystem;
    private readonly GameConfigData config;

    [Inject]
    public ProjectAudioService(SoundSystem soundSystem, GameConfigData config)
    {
        this.soundSystem = soundSystem;
        this.config = config;
    }

    private AudioKey? lastPlayedSoundType;
    private float lastPlayedSoundTime;
    private float soundCooldownDuration = 0.1f;
    private AudioKey currentBGM = AudioKey.None;

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
        AudioEvents.OnPlayContinuousSFX += HandleOnPlayContinuousSFX;
        AudioEvents.OnStopBGM += HandleOnStopBGM;
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
        AudioEvents.OnPlayContinuousSFX -= HandleOnPlayContinuousSFX;
        AudioEvents.OnStopBGM -= HandleOnStopBGM;
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

    private void PlayContinuousSFX(AudioKey key, bool shouldPlay)
    {
        if (shouldPlay)
        {
            soundSystem.PlayAudio(key, loop: true);
        }
        else
        {
            soundSystem.StopAudio(key);
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
            currentBGM = keyToPlay;

            if (soundSystem != null)
                soundSystem.PlayAudio(keyToPlay, volume: 1f, loop: true, fadeInSeconds: config.bgmFadeDuration, fadeOutSeconds: config.bgmFadeDuration);
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
            currentBGM = AudioKey.BGM_Home;

            if (soundSystem != null)
                soundSystem.PlayAudio(AudioKey.BGM_Home, volume: 1f, loop: true, fadeInSeconds: config.bgmFadeDuration, fadeOutSeconds: config.bgmFadeDuration);
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

    private void HandleOnPlayContinuousSFX(AudioKey key, bool shouldPlay)
    {
        PlayContinuousSFX(key, shouldPlay);
    }

    private void HandleOnStopBGM()
    {
        try
        {
            if (soundSystem != null && currentBGM != AudioKey.None)
            {
                soundSystem.StopAudio(currentBGM); 
                
                currentBGM = AudioKey.None;
            }
        }
        catch (Exception e)
        {
            AppLogger.LogWarning($"[ProjectAudioService] Failed to stop BGM: {e.Message}");
        }
    }
}