using System;
using UnityEngine;

public static class CoreEvents
{
    public static event Action<ProjectSettingsData> OnSettingsChanged;

    public static void TriggerSettingsChange(ProjectSettingsData projectSettingsData)
    {
        OnSettingsChanged?.Invoke(projectSettingsData);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnSettingsChanged = null;
    }
}