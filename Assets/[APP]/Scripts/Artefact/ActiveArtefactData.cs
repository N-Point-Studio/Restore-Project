using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using Modules.SavingSystems;

/// <summary>
/// Active runtime container for artefact progression and unlock states.
/// Manages which artefact the player has unlocked.
/// This is persistent data - NOT reset between runs.
/// </summary>
[Serializable]
public class ActiveArtefactData : ISaveable
{
    [SerializeField] private List<ArtefactProgressData> artefactProgress = new List<ArtefactProgressData>();

    private readonly ArtefactDatabase artefactDatabase;

    public ActiveArtefactData(ArtefactDatabase artefactDatabase)
    {
        this.artefactDatabase = artefactDatabase;
    }

    /// <summary>
    /// Gets the ArtefactDatabase reference (for other services that need it).
    /// </summary>
    public ArtefactDatabase GetArtefactDatabase() => artefactDatabase;

    /// <summary>
    /// Initialize with default unlocked content on new game.
    /// </summary>
    public void Initialize()
    {
        artefactProgress.Clear();

        // Unlock default artefacts (their equipment is automatically available)
        var defaultArtefacts = artefactDatabase.GetDefaultUnlockedArtefacts();
        foreach (var artefact in defaultArtefacts)
        {
            UnlockArtefact(artefact.BaseData.Id);
        }
    }

    #region Artefact Methods

    /// <summary>
    /// Unlocks an artefact for selection.
    /// </summary>
    public void UnlockArtefact(string artefactId)
    {
        if (artefactProgress.Any(p => p.artefactId == artefactId))
            return;

        artefactProgress.Add(new ArtefactProgressData
        {
            artefactId = artefactId,
            isUnlocked = true,
        });
    }

    /// <summary>
    /// Check if an artefact is unlocked.
    /// </summary>
    public bool IsArtefactUnlocked(string artefactId)
    {
        return artefactProgress.Any(p => p.artefactId == artefactId && p.isUnlocked);
    }

    /// <summary>
    /// Gets all unlocked artefacts with their runtime data.
    /// </summary>
    public List<ArtefactRuntimeData> GetUnlockedArtefacts()
    {
        var result = new List<ArtefactRuntimeData>();
        foreach (ArtefactProgressData progress in artefactProgress.Where(p => p.isUnlocked))
        {
            var artefactData = artefactDatabase.GetArtefact(progress.artefactId);
            if (artefactData != null)
            {
                result.Add(new ArtefactRuntimeData
                {
                    ArtefactData = artefactData,
                });
            }
        }
        return result;
    }

    #endregion

    #region ISaveable Implementation

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();

        // Artefact progress
        JSONArray artefactsArray = new JSONArray();
        foreach (var progress in artefactProgress)
        {
            JSONObject artefactJson = new JSONObject();
            artefactJson[nameof(ArtefactProgressData.artefactId)] = progress.artefactId;
            artefactJson[nameof(ArtefactProgressData.isUnlocked)].AsBool = progress.isUnlocked;
            artefactsArray.Add(artefactJson);
        }
        json["artefacts"] = artefactsArray;

        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        artefactProgress.Clear();

        // Load artefact progress
        if (json.HasKey("artefacts"))
        {
            JSONArray artefactsArray = json["artefacts"].AsArray;
            for (int i = 0; i < artefactsArray.Count; i++)
            {
                JSONNode artefactJson = artefactsArray[i];
                artefactProgress.Add(new ArtefactProgressData
                {
                    artefactId = artefactJson[nameof(ArtefactProgressData.artefactId)],
                    isUnlocked = artefactJson[nameof(ArtefactProgressData.isUnlocked)].AsBool,
                });
            }
        }
    }

    #endregion
}

/// <summary>
/// Serializable data for individual artefact progression.
/// </summary>
[Serializable]
public class ArtefactProgressData
{
    public string artefactId;
    public bool isUnlocked;
}

/// <summary>
/// Runtime data combining artefact static data with progression.
/// </summary>
public class ArtefactRuntimeData
{
    public ArtefactData ArtefactData { get; set; }
}
