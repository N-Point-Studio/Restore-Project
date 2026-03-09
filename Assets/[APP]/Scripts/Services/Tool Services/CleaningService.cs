using System;
using UnityEngine;

public class CleaningService
{
    public event Action OnCleaningPerformed;
    public event Action OnCleaningEnded;

    public bool isCleaning;

    public void TryCleaning()
    {
        isCleaning = true;
        Debug.Log("Cleaning cuy!");
        // OnCleaningPerformed.Invoke();
    }

    public void EndClean()
    {
        isCleaning = false;
        Debug.Log("Cleaning cuy! false");
    }
}
