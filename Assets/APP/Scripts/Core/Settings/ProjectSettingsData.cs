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

    public JSONNode AsJSON()
    {
        JSONObject json = new JSONObject();

        json["graphic_mode"].AsInt = (int)graphicMode;
        json["master_volume"].AsFloat = masterVolume;
        json["sfx_volume"].AsFloat = sfxVolume;
        json["bgm_volume"].AsFloat = bgmVolume;
        json["crosshair"].AsBool = crosshair;

        return json;
    }

    public void LoadFromJSON(JSONNode json)
    {
        graphicMode = (ProjectSettingsGraphicMode)json["graphic_mode"].AsInt;
        masterVolume = json["master_volume"].AsFloat;
        sfxVolume = json["sfx_volume"].AsFloat;
        bgmVolume = json["bgm_volume"].AsFloat;
        crosshair = json["crosshair"].AsBool;
    }
}

public enum ProjectSettingsType { Graphic, Sound, Crosshair }

public enum ProjectSettingsGraphicMode { Performance, Balance, Quality }

public enum ProjectSettingsAudioType { Master, SFX, BGM }