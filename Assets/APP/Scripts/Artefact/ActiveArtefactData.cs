using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using Modules.SavingSystems;
using Modules;

[Serializable]
public class ActiveArtefactData : ISaveable
{
    [SerializeField] private List<ArtefactProgressData> artefactProgress = new List<ArtefactProgressData>();

    private HashSet<string> unlockedArtefactIds = new HashSet<string>();
    private HashSet<string> completedArtefactIds = new HashSet<string>();
    private Dictionary<string, ArtefactData> artefactDataCache = new Dictionary<string, ArtefactData>();

    private readonly ArtefactDatabase artefactDatabase;

    public Action OnUpdated;

    public ActiveArtefactData(ArtefactDatabase artefactDatabase)
    {
        this.artefactDatabase = artefactDatabase;
    }

    public ArtefactDatabase GetArtefactDatabase() => artefactDatabase;

    public void Initialize()
    {
        BuildDataCache();
        RefreshStateCache();

        var allArtefacts = artefactDatabase.GetAllArtefactDatas();
        for (int i = 0; i < allArtefacts.Count; i++)
        {
            if (allArtefacts[i] != null && allArtefacts[i].UnlockedByDefault)
            {
                UnlockArtefact(allArtefacts[i].BaseData.Id);
            }
        }

        EvaluateUnlockRequirements();

        // for (int i = 0; i < artefactProgress.Count; i++)
        // {
        //     artefactProgress[i].hasSeenUnlockAnim = true;
        //     artefactProgress[i].hasSeenCompletionAnim = true;
        // }
    }

    public void ResetData()
    {
        artefactProgress.Clear();

        Initialize();
    }

    private void BuildDataCache()
    {
        artefactDataCache.Clear();
        var allData = artefactDatabase.GetAllArtefactDatas();

        for (int i = 0; i < allData.Count; i++)
        {
            var item = allData[i];
            if (item != null && !artefactDataCache.ContainsKey(item.BaseData.Id))
            {
                artefactDataCache.Add(item.BaseData.Id, item);
            }
        }
    }

    private void RefreshStateCache()
    {
        unlockedArtefactIds.Clear();
        completedArtefactIds.Clear();

        for (int i = 0; i < artefactProgress.Count; i++)
        {
            if (artefactProgress[i].isUnlocked)
            {
                unlockedArtefactIds.Add(artefactProgress[i].artefactId);
            }

            if (artefactProgress[i].isCompleted)
            {
                completedArtefactIds.Add(artefactProgress[i].artefactId);
            }
        }
    }

    public void UnlockArtefact(string artefactId)
    {
        if (unlockedArtefactIds.Contains(artefactId))
            return;

        artefactProgress.Add(new ArtefactProgressData
        {
            artefactId = artefactId,
            isUnlocked = true,
            isCompleted = false,
            hasSeenUnlockAnim = false,
            hasSeenCompletionAnim = false
        });

        unlockedArtefactIds.Add(artefactId);

        OnUpdated?.Invoke();
    }

    public void CompleteArtefact(string artefactId)
    {
        if (completedArtefactIds.Contains(artefactId))
            return;

        completedArtefactIds.Add(artefactId);

        var data = artefactProgress.Find(x => x.artefactId == artefactId);

        if (data != null)
        {
            data.isCompleted = true;
            if (!data.isUnlocked)
            {
                data.isUnlocked = true;
                unlockedArtefactIds.Add(artefactId);
            }
            data.hasSeenCompletionAnim = false;
        }
        else
        {
            artefactProgress.Add(new ArtefactProgressData
            {
                artefactId = artefactId,
                isUnlocked = true,
                isCompleted = true
            });
            unlockedArtefactIds.Add(artefactId);
        }

        OnUpdated?.Invoke();

        EvaluateUnlockRequirements();
    }

    private void EvaluateUnlockRequirements()
    {
        bool hasUnlockedNewArtefact = false;

        foreach (var kvp in artefactDataCache)
        {
            string targetId = kvp.Key;
            ArtefactData targetData = kvp.Value;

            if (IsArtefactUnlocked(targetId))
                continue;

            if (targetData.UnlockRequirements == null || targetData.UnlockRequirements.Length == 0)
                continue;

            bool allRequirementsMet = true;

            foreach (var req in targetData.UnlockRequirements)
            {
                if (req == null) continue;

                if (!IsArtefactCompleted(req.BaseData.Id))
                {
                    allRequirementsMet = false;
                    break;
                }
            }

            if (allRequirementsMet)
            {
                UnlockArtefact(targetId);
                hasUnlockedNewArtefact = true;
                AppLogger.Log($"[Artefact System] Unlocked new artefact: {targetData.BaseData.ItemName} because all requirements are met!");
            }
        }

        if (hasUnlockedNewArtefact)
        {
            EvaluateUnlockRequirements();
        }
    }

    public bool IsArtefactUnlocked(string artefactId)
    {
        return unlockedArtefactIds.Contains(artefactId);
    }

    public bool IsArtefactCompleted(string artefactId)
    {
        return completedArtefactIds.Contains(artefactId);
    }

    public List<ArtefactRuntimeData> GetUnlockedArtefacts()
    {
        var result = new List<ArtefactRuntimeData>();

        foreach (string id in unlockedArtefactIds)
        {
            if (artefactDataCache.TryGetValue(id, out ArtefactData data))
            {
                result.Add(new ArtefactRuntimeData
                {
                    ArtefactData = data,
                });
            }
        }
        return result;
    }

    public List<ArtefactProgressData> GetPendingCompletionAnimations()
    {
        return artefactProgress.FindAll(x => x.isCompleted && !x.hasSeenCompletionAnim);
    }

    public List<ArtefactProgressData> GetPendingUnlockAnimations()
    {
        return artefactProgress.FindAll(x => x.isUnlocked && !x.hasSeenUnlockAnim);
    }

    public void MarkCompletionAnimSeen(string artefactId)
    {
        ArtefactProgressData data = artefactProgress.Find(x => x.artefactId == artefactId);
        if (data != null && !data.hasSeenCompletionAnim)
        {
            data.hasSeenCompletionAnim = true;
            OnUpdated?.Invoke();
        }
    }

    public void MarkUnlockAnimSeen(string artefactId)
    {
        ArtefactProgressData data = artefactProgress.Find(x => x.artefactId == artefactId);
        if (data != null && !data.hasSeenUnlockAnim)
        {
            data.hasSeenUnlockAnim = true;
            OnUpdated?.Invoke();
        }
    }

    public bool IsStoryRead(string artefactId)
    {
        var data = artefactProgress.Find(x => x.artefactId == artefactId);
        return data != null && data.hasReadStory;
    }

    public void MarkStoryRead(string artefactId)
    {
        var data = artefactProgress.Find(x => x.artefactId == artefactId);
        if (data != null && !data.hasReadStory)
        {
            data.hasReadStory = true;
            OnUpdated?.Invoke();
        }
    }

    public bool AreAllArtefactsCompleted()
    {
        var allData = artefactDatabase.GetAllArtefactDatas();
        for (int i = 0; i < allData.Count; i++)
        {
            if (allData[i] != null && !IsArtefactCompleted(allData[i].BaseData.Id))
            {
                return false;
            }
        }
        return true;
    }

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();
        JSONArray artefactsArray = new JSONArray();

        for (int i = 0; i < artefactProgress.Count; i++)
        {
            JSONObject artefactJson = new JSONObject();
            artefactJson[nameof(ArtefactProgressData.artefactId)] = artefactProgress[i].artefactId;
            artefactJson[nameof(ArtefactProgressData.isUnlocked)].AsBool = artefactProgress[i].isUnlocked;
            artefactJson[nameof(ArtefactProgressData.isCompleted)].AsBool = artefactProgress[i].isCompleted;
            artefactJson[nameof(ArtefactProgressData.hasSeenUnlockAnim)].AsBool = artefactProgress[i].hasSeenUnlockAnim;
            artefactJson[nameof(ArtefactProgressData.hasSeenCompletionAnim)].AsBool = artefactProgress[i].hasSeenCompletionAnim;
            artefactJson[nameof(ArtefactProgressData.hasReadStory)].AsBool = artefactProgress[i].hasReadStory;
            artefactsArray.Add(artefactJson);
        }
        json["artefacts"] = artefactsArray;
        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        BuildDataCache();

        artefactProgress.Clear();

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
                    isCompleted = artefactJson[nameof(ArtefactProgressData.isCompleted)].AsBool,
                    hasSeenUnlockAnim = artefactJson[nameof(ArtefactProgressData.hasSeenUnlockAnim)].AsBool,
                    hasSeenCompletionAnim = artefactJson[nameof(ArtefactProgressData.hasSeenCompletionAnim)].AsBool,
                    hasReadStory = artefactJson[nameof(ArtefactProgressData.hasReadStory)].AsBool
                });
            }
        }

        RefreshStateCache();

        EvaluateUnlockRequirements();
    }
}

[Serializable]
public class ArtefactProgressData
{
    public string artefactId;
    public bool isUnlocked;
    public bool isCompleted;
    public bool hasSeenUnlockAnim;
    public bool hasSeenCompletionAnim;
    public bool hasReadStory;
}

public class ArtefactRuntimeData
{
    public ArtefactData ArtefactData { get; set; }
}