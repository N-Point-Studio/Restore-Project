using Modules.SavingSystems;
using VContainer;
using VContainer.Unity;
using Modules;
using System;

public class ProjectSavingSystem : SavingSystem, IInitializable, IStartable, IDisposable, ITickable
{
    [Inject] protected readonly ActiveArtefactData activeArtefactData;
    [Inject] protected readonly ActiveSettingsData activeSettingsData;
    [Inject] protected readonly PlayerProgressionData playerProgressionData;

    public int MaxSlot => 6;

    private int progress;
    private bool isSaving;
    private bool isLoading;
    private Action onFinished;

    void IInitializable.Initialize()
    {
        playerProgressionData.OnUpdated += HandleDataChanged;
        activeArtefactData.OnUpdated += HandleDataChanged;
    }

    void IDisposable.Dispose()
    {
        playerProgressionData.OnUpdated -= HandleDataChanged;
        activeArtefactData.OnUpdated -= HandleDataChanged;
    }

    private void HandleDataChanged()
    {
        SaveAll();
    }

    void IStartable.Start()
    {
        LoadAll(0, () => 
        {
            AppLogger.Log("[ProjectSavingSystem] Semua data save berhasil dimuat pada saat startup!");
        });
    }

    public void SaveAll(int slot = 0, Action onFinished = null)
    {
        isSaving = true;
        this.onFinished = onFinished;
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
        }
    }

    public void LoadAll(int slot = 0, Action onFinished = null)
    {
        isLoading = true;
        this.onFinished = onFinished;
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
        }
    }

    void ITickable.Tick()
    {
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if ((isSaving || isLoading) && progress <= 0)
        {
            onFinished?.Invoke();
            isSaving = false;
            isLoading = false;
            onFinished = null;
        }
    }
}