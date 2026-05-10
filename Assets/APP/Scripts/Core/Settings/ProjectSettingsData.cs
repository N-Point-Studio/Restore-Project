using Modules.SavingSystems;
using SimpleJSON;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ProjectSettingsData), menuName = "App/Data/Project Settings Data")]
[Serializable]
public class ProjectSettingsData : ScriptableObject, ISaveable
{
    [SerializeField] private ProjectSettingsGraphicMode graphicMode = ProjectSettingsGraphicMode.Quality;
    public ProjectSettingsGraphicMode GraphicMode => graphicMode;

    [SerializeField] private float masterVolume = 1f;
    public float MasterVolume => masterVolume;

    [SerializeField] private float sfxVolume = 1f;
    public float SFXVolume => sfxVolume;

    [SerializeField] private float bgmVolume = 1f;
    public float BGMVolume => bgmVolume;

    [SerializeField] private bool crosshair = false;
    public bool Crosshair => crosshair;

    [SerializeField] private string languageCode = "en";
    public string LanguageCode => languageCode;

    public void Apply(ProjectSettingsGraphicMode graphicMode)
    {
        this.graphicMode = graphicMode;
    }

    public void Apply(ProjectSettingsAudioType audioType, float volume)
    {
        if (audioType == ProjectSettingsAudioType.Master)
        {
            this.masterVolume = volume;
        }
        else if (audioType == ProjectSettingsAudioType.SFX)
        {
            this.sfxVolume = volume;
        }
        else if (audioType == ProjectSettingsAudioType.BGM)
        {
            this.bgmVolume = volume;
        }
    }

    public void Apply(bool enabled)
    {
        this.crosshair = enabled;
    }

    public void Apply(string code)
    {
        this.languageCode = code;
    }

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();

        json["graphic_mode"].AsInt = (int)graphicMode;
        json["master_volume"].AsFloat = masterVolume;
        json["sfx_volume"].AsFloat = sfxVolume;
        json["bgm_volume"].AsFloat = bgmVolume;
        json["crosshair"].AsBool = crosshair;        
        json["language_code"] = languageCode;

        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        if (json == null || json.ToString() == "{}" || json.ToString() == "null") return;

        if (json.HasKey("graphic_mode"))
        {
            graphicMode = (ProjectSettingsGraphicMode)json["graphic_mode"].AsInt;
        }

        if (json.HasKey("master_volume"))
        {
            masterVolume = json["master_volume"].AsFloat;
        }

        if (json.HasKey("sfx_volume"))
        {
            sfxVolume = json["sfx_volume"].AsFloat;
        }

        if (json.HasKey("bgm_volume"))
        {
            bgmVolume = json["bgm_volume"].AsFloat;
        }

        if (json.HasKey("crosshair"))
        {
            crosshair = json["crosshair"].AsBool;
        }

        if (json.HasKey("language_code"))
        {
            languageCode = json["language_code"].Value;
        }
    }
}

public enum ProjectSettingsType { Graphic, Sound, Crosshair, Language }
public enum ProjectSettingsGraphicMode { Performance, Balance, Quality }
public enum ProjectSettingsAudioType { Master, SFX, BGM }