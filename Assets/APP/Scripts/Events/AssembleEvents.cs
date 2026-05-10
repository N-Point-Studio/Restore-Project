using System;
using UnityEngine;

public static class AssembleEvents
{
    public static Action OnAssemblePerformed;
    public static Action OnAssembleFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnAssemblePerformed = null;
        OnAssembleFinished = null;
    }
}
