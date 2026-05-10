using Modules.SavingSystems;
using SimpleJSON;
using System;

public class ActiveSettingsData : ISaveable
{
    public ProjectSettingsData DefaultSettingsData { get; private set; }
    public ProjectSettingsData CurrentSettingsData { get; private set; }

    public Action OnSettingsDataLoaded;

    public ActiveSettingsData(ProjectSettingsData defaultSettingsDataAsset)
    {
        this.DefaultSettingsData = defaultSettingsDataAsset;
        this.CurrentSettingsData = UnityEngine.Object.Instantiate(defaultSettingsDataAsset);
    }

    public void CopyFrom(ProjectSettingsData source)
    {
        CurrentSettingsData.Apply(source.GraphicMode);
        CurrentSettingsData.Apply(ProjectSettingsAudioType.Master, source.MasterVolume);
        CurrentSettingsData.Apply(ProjectSettingsAudioType.BGM, source.BGMVolume);
        CurrentSettingsData.Apply(ProjectSettingsAudioType.SFX, source.SFXVolume);
        CurrentSettingsData.Apply(source.Crosshair);
        CurrentSettingsData.Apply(source.LanguageCode);
    }

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();
        json[nameof(CurrentSettingsData)] = CurrentSettingsData.AsJSON();
        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        if (json == null || json.ToString() == "{}" || json.ToString() == "null") return;

        CurrentSettingsData.LoadFromJSON(json[nameof(CurrentSettingsData)]);
        OnSettingsDataLoaded?.Invoke();
    }
}