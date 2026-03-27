using Modules.SavingSystems;
using VContainer;
using VContainer.Unity;
using Modules;
using System;
using UnityEngine;

public class ProjectSavingSystem : SavingSystem, IInitializable, IStartable, IDisposable, ITickable
{
    [Inject] protected readonly ActiveArtefactData activeArtefactData;
    [Inject] protected readonly ActiveSettingsData activeSettingsData;
    [Inject] protected readonly PlayerProgressionData playerProgressionData;
    [Inject] protected readonly GameConfigData config;

    public int MaxSlot => 6;

    private int progress;
    private bool isSaving;
    private bool isLoading;
    private Action onFinished;
    
    private bool isDataDirty = false;
    private float autoSaveTimer = 0f;

    void IInitializable.Initialize()
    {
        LoadAll(0, () => 
        {
            AppLogger.Log("[ProjectSavingSystem] All save data loaded successfully at startup!");
        });
        
        playerProgressionData.OnUpdated += RequestAutoSave;
        activeArtefactData.OnUpdated += RequestAutoSave;
    }

    void IDisposable.Dispose()
    {
        playerProgressionData.OnUpdated -= RequestAutoSave;
        activeArtefactData.OnUpdated -= RequestAutoSave;
    }

    private void RequestAutoSave()
    {
        isDataDirty = true;
        autoSaveTimer = config.autoSaveCooldown;
    }

    void IStartable.Start()
    {
    }

    public void SaveAll(int slot = 0, Action onFinishedCallback = null)
    {
        if (isSaving) 
        {
            AppLogger.LogWarning("[ProjectSavingSystem] Sedang proses save, request ditunda/diabaikan.");
            if (onFinishedCallback != null) this.onFinished += onFinishedCallback;
            return;
        }

        isSaving = true;
        isDataDirty = false;
        
        if (onFinishedCallback != null)
        {
            this.onFinished += onFinishedCallback;
        }
        
        progress = 3; 

        try
        {
            SaveToFile($"{slot}/{nameof(activeArtefactData)}", activeArtefactData.AsJSON(), () => progress--);
            SaveToFile($"{slot}/{nameof(activeSettingsData)}", activeSettingsData.AsJSON(), () => progress--);
            SaveToFile($"{slot}/{nameof(playerProgressionData)}", playerProgressionData.AsJSON(), () => progress--);
        }
        catch (Exception e)
        {
            AppLogger.LogError($"Error while saving data: {e}");
            isSaving = false;
            this.onFinished?.Invoke();
            this.onFinished = null;
        }
    }

    public void LoadAll(int slot = 0, Action onFinishedCallback = null)
    {
        if (isLoading) return;

        isLoading = true;
        this.onFinished = onFinishedCallback;
        progress = 3; 

        try
        {
            LoadFromFile($"{slot}/{nameof(activeArtefactData)}", result => 
            { 
                if (result != null && !result.IsNull) 
                {
                    activeArtefactData.LoadFromJSON(result); 
                }
                else 
                {
                    activeArtefactData.Initialize(); 
                }
                progress--; 
            });

            LoadFromFile($"{slot}/{nameof(activeSettingsData)}", result => 
            { 
                if (result != null && !result.IsNull) activeSettingsData.LoadFromJSON(result); 
                progress--; 
            });
            LoadFromFile($"{slot}/{nameof(playerProgressionData)}", result => 
            { 
                if (result != null && !result.IsNull) playerProgressionData.LoadFromJSON(result); 
                progress--; 
            });
        }
        catch (Exception e)
        {
            AppLogger.LogError($"Error while loading data: {e}");
            isLoading = false;
            this.onFinished?.Invoke();
            this.onFinished = null;
        }
    }

    void ITickable.Tick()
    {
        if (isDataDirty && !isSaving && !isLoading)
        {
            autoSaveTimer -= Time.deltaTime;
            if (autoSaveTimer <= 0)
            {
                SaveAll();
            }
        }

        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if ((isSaving || isLoading) && progress <= 0)
        {
            isSaving = false;
            isLoading = false;
            
            onFinished?.Invoke();
            onFinished = null;
        }
    }
}