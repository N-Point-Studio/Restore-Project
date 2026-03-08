using Modules.SavingSystems;
using SimpleJSON;
using System;

public class ActiveSettingsData : ISaveable
{
    private ProjectSettingsData defaultSettingsData;
    public ProjectSettingsData DefaultSettingsData => defaultSettingsData;

    private ProjectSettingsData currentSettingsData;
    public ProjectSettingsData CurrentSettingsData => currentSettingsData;

    public Action OnSettingsDataLoaded;

    public ActiveSettingsData(ProjectSettingsData defaultSettingsDataAsset)
    {
        this.defaultSettingsData = UnityEngine.Object.Instantiate(defaultSettingsDataAsset);
        this.currentSettingsData = UnityEngine.Object.Instantiate(defaultSettingsDataAsset);
    }

    public void UpdateSettingsData(ProjectSettingsData updatedSettingsData)
    {
        if (currentSettingsData != null) UnityEngine.Object.Destroy(currentSettingsData);
        currentSettingsData = UnityEngine.Object.Instantiate(updatedSettingsData);
    }

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();
        json[nameof(currentSettingsData)] = currentSettingsData.AsJSON();

        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        if (json == null || json.ToString() == "{}" || json.ToString() == "null")
        {
            return;
        }

        currentSettingsData.LoadFromJSON(json[nameof(currentSettingsData)]);
        OnSettingsDataLoaded?.Invoke();
    }
}