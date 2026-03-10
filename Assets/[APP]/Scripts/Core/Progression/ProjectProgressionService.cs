using Modules.SavingSystems;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectProgressionService : IInitializable, IPostInitializable, IDisposable
{
    [Inject] private readonly ProjectSavingSystem savingSystem;
    [Inject] private readonly PlayerProgressionData progressionData;
    [Inject] private readonly ActiveArtefactData activeArtefactData;
    [Inject] private readonly ActiveSettingsData activeSettingsData;

    private const int CURRENT_SAVE_SLOT = 0;

    void IInitializable.Initialize()
    {        
        progressionData.OnUpdated += HandleOnDataChanged;
        activeArtefactData.OnUpdated += HandleOnDataChanged;
        activeSettingsData.OnSettingsDataLoaded += HandleOnDataChanged;
    }

    void IPostInitializable.PostInitialize()
    {
        LoadProgression();
    }

    void IDisposable.Dispose()
    {
        progressionData.OnUpdated -= HandleOnDataChanged;
        activeArtefactData.OnUpdated += HandleOnDataChanged;
        activeSettingsData.OnSettingsDataLoaded -= HandleOnDataChanged;
    }

    private void HandleOnDataChanged()
    {
        SaveProgression();
    }

    private void SaveProgression()
    {
        savingSystem.SaveToFile($"{CURRENT_SAVE_SLOT}/{nameof(progressionData)}", progressionData.AsJSON());
        savingSystem.SaveToFile($"{CURRENT_SAVE_SLOT}/{nameof(activeArtefactData)}", activeArtefactData.AsJSON());
        savingSystem.SaveToFile($"{CURRENT_SAVE_SLOT}/{nameof(activeSettingsData)}", activeSettingsData.AsJSON());
    }

    private void LoadProgression()
    {
        savingSystem.LoadFromFile($"{CURRENT_SAVE_SLOT}/{nameof(progressionData)}", result => 
        {
            if (result != null && !result.IsNull) progressionData.LoadFromJSON(result);
        });

        savingSystem.LoadFromFile($"{CURRENT_SAVE_SLOT}/{nameof(activeArtefactData)}", result => 
        {
            if (result != null && !result.IsNull) activeArtefactData.LoadFromJSON(result);
        });

        savingSystem.LoadFromFile($"{CURRENT_SAVE_SLOT}/{nameof(activeSettingsData)}", result => 
        {
            if (result != null && !result.IsNull) activeSettingsData.LoadFromJSON(result);
        });
    }
}