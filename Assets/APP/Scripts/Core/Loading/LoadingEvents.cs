using System;
using UnityEngine;

public static class LoadingEvents
{
    public static Action OnLoadingStarted;
    public static Action OnLoadingFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnLoadingStarted = null;
        OnLoadingFinished = null;
    }
}
