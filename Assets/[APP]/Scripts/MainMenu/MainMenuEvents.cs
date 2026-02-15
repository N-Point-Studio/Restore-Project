using System;
using UnityEngine;

public static class MainMenuEvents
{
    public static Action<ArtefactItemUI> OnRequestArtefactDetail;
    public static Action<ArtefactData> OnRequestArtefactPlay;
    public static Action OnCloseArtefactDetail;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnRequestArtefactDetail = null;
        OnRequestArtefactPlay = null;
        OnCloseArtefactDetail = null;
    }
}
