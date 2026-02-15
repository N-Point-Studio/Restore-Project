using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using Modules.SavingSystems;

[Serializable]
public class ActiveArtefactData : ISaveable
{
    [SerializeField] private List<ArtefactProgressData> artefactProgress = new List<ArtefactProgressData>();

    private HashSet<string> unlockedArtefactIds = new HashSet<string>();
    private HashSet<string> completedArtefactIds = new HashSet<string>();
    private Dictionary<string, ArtefactData> artefactDataCache = new Dictionary<string, ArtefactData>();

    private readonly ArtefactDatabase artefactDatabase;

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
            if (allArtefacts[i].UnlockedByDefault)
            {
                UnlockArtefact(allArtefacts[i].BaseData.Id);
            }
        }
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
            isCompleted = false
        });

        unlockedArtefactIds.Add(artefactId);
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
            artefactsArray.Add(artefactJson);
        }
        json["artefacts"] = artefactsArray;
        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
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
                    isCompleted = artefactJson[nameof(ArtefactProgressData.isCompleted)].AsBool
                });
            }
        }

        RefreshStateCache();
    }
}

[Serializable]
public class ArtefactProgressData
{
    public string artefactId;
    public bool isUnlocked;
    public bool isCompleted;
}

public class ArtefactRuntimeData
{
    public ArtefactData ArtefactData { get; set; }
}