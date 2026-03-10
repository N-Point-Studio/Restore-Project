using Modules.SavingSystems;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerProgressionData : ISaveable
{
    private bool hasPlayedBefore;
    public bool HasPlayedBefore => hasPlayedBefore;

    private HashSet<int> shownTutorials = new HashSet<int>();
    
    private string currentActiveArtefactId;
    public string CurrentActiveArtefactId => currentActiveArtefactId;

    public Action OnUpdated;

    public PlayerProgressionData()
    {
        hasPlayedBefore = false;
        shownTutorials.Clear();
        currentActiveArtefactId = string.Empty;
    }

    public void MarkAsPlayed()
    {
        if (!hasPlayedBefore)
        {
            hasPlayedBefore = true;
            OnUpdated?.Invoke();
        }
    }

    public bool HasTutorialShown(int tutorialId)
    {
        return shownTutorials.Contains(tutorialId);
    }

    public void MarkTutorialShown(int tutorialId)
    {
        if (!shownTutorials.Contains(tutorialId))
        {
            shownTutorials.Add(tutorialId);
            OnUpdated?.Invoke();
        }
    }

    public void SetCurrentActiveArtefact(string artefactId)
    {
        if (currentActiveArtefactId != artefactId)
        {
            currentActiveArtefactId = artefactId;
            OnUpdated?.Invoke();
        }
    }

    public void ClearData()
    {
        hasPlayedBefore = false;
        shownTutorials.Clear();
        currentActiveArtefactId = string.Empty;
        OnUpdated?.Invoke();
    }

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();
        json[nameof(hasPlayedBefore)] = hasPlayedBefore;
        json[nameof(currentActiveArtefactId)] = currentActiveArtefactId;

        JSONArray tutorialsArray = new JSONArray();
        foreach (int id in shownTutorials)
        {
            tutorialsArray.Add(id);
        }
        json[nameof(shownTutorials)] = tutorialsArray;

        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        if (json.HasKey(nameof(hasPlayedBefore)))
            hasPlayedBefore = json[nameof(hasPlayedBefore)].AsBool;

        if (json.HasKey(nameof(currentActiveArtefactId)))
            currentActiveArtefactId = json[nameof(currentActiveArtefactId)];

        if (json.HasKey(nameof(shownTutorials)))
        {
            shownTutorials.Clear();
            JSONArray tutorialsArray = json[nameof(shownTutorials)].AsArray;
            foreach (JSONNode node in tutorialsArray)
            {
                shownTutorials.Add(node.AsInt);
            }
        }
    }
}