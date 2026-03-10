using Modules.SavingSystems;
using SimpleJSON;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Collections.Generic;
using Modules;

public class ProjectSavingSystem : SavingSystem, IStartable, ITickable
{
    [Inject] protected readonly ActiveArtefactData activeArtefactData;
    [Inject] protected readonly ActiveSettingsData activeSettingsData;

    // Player progress tracking
    private bool hasPlayedBefore;
    private HashSet<int> shownTutorials = new HashSet<int>();
    
    /// <summary>
    /// Whether the player has ever launched gameplay before (used for first-time flows / tutorials).
    /// </summary>
    public bool HasPlayedBefore => hasPlayedBefore;

    public int MaxSlot => 6;

    private int progress;
    private bool isSaving;
    private bool isLoading;
    private System.Action onFinished;

    /// <summary>
    /// Marks that the player has completed their first play session. Idempotent.
    /// </summary>
    public void MarkAsPlayed()
    {
        hasPlayedBefore = true;
    }

    /// <summary>
    /// Checks if a specific tutorial has been shown before.
    /// </summary>
    public bool HasTutorialShown(int tutorialId)
    {
        return shownTutorials.Contains(tutorialId);
    }

    /// <summary>
    /// Marks a specific tutorial as shown. Idempotent.
    /// </summary>
    public void MarkTutorialShown(int tutorialId)
    {
        shownTutorials.Add(tutorialId);
    }

    /// <summary>
    /// Clears all runtime data and initializes to default state.
    /// Call this when starting a new game.
    /// </summary>
    public void ClearPreviousData()
    {
        activeArtefactData.Initialize();
        hasPlayedBefore = false;
        shownTutorials.Clear();
    }

    /// <summary>
    /// Saves all game state to the specified slot.
    /// </summary>
    public void SaveAll(int slot = 0, System.Action onFinished = null)
    {
        isSaving = true;
        this.onFinished = onFinished;
        progress = 2; // Number of things we're saving (activeArtefactData + player progress)

        try
        {
            SaveToFile($"{slot}/{nameof(activeArtefactData)}", activeArtefactData.AsJSON(), () => progress--);
            SaveToFile($"{slot}/{nameof(activeSettingsData)}", activeSettingsData.AsJSON(), () => progress--);
                
            // Save player progress (hasPlayedBefore, etc.)
            JSONNode playerProgressNode = new JSONObject();
            playerProgressNode["hasPlayedBefore"] = hasPlayedBefore;
            JSONArray tutorialsArray = new JSONArray();
            foreach (int id in shownTutorials)
            {
                tutorialsArray.Add(id);
            }
            playerProgressNode["shownTutorials"] = tutorialsArray;

            SaveToFile($"{slot}/PlayerProgress", playerProgressNode, () => progress--);
        }
        catch (System.Exception e)
        {
            AppLogger.LogError($"Error while saving data: {e}");
            isSaving = false;
        }
    }

    /// <summary>
    /// Loads all game state from the specified slot.
    /// </summary>
    public void LoadAll(int slot = 0, System.Action onFinished = null)
    {
        isLoading = true;
        this.onFinished = onFinished;
        progress = 2; // Number of things we're loading (activeArtefactData + player progress)

        try
        {
            LoadFromFile($"{slot}/{nameof(activeArtefactData)}", result => { 
                    if (result != null && !result.IsNull)
                    {
                        activeArtefactData.LoadFromJSON(result);
                    }
                    progress--; 
                });

                
            LoadFromFile($"{slot}/{nameof(activeSettingsData)}", result => { activeSettingsData.LoadFromJSON(result); progress--; });
                
            // Load player progress
            LoadFromFile($"{slot}/PlayerProgress", 
                result => { 
                    if (result != null && !result.IsNull)
                    {
                        hasPlayedBefore = result["hasPlayedBefore"].AsBool;
                        shownTutorials.Clear();
                        if (result["shownTutorials"] != null)
                        {
                            JSONArray tutorialsArray = result["shownTutorials"].AsArray;
                            foreach (JSONNode node in tutorialsArray)
                            {
                                shownTutorials.Add(node.AsInt);
                            }
                        }
                    }
                    progress--; 
                });
        }
        catch (System.Exception e)
        {
            AppLogger.LogError($"Error while loading data: {e}");
            isLoading = false;
        }
    }

    void IStartable.Start()
    {
        // Initialize on startup if needed
    }

    void ITickable.Tick()
    {
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if ((isSaving || isLoading) && progress == 0)
        {
            onFinished?.Invoke();
            isSaving = false;
            isLoading = false;
            onFinished = null;
        }
    }
}