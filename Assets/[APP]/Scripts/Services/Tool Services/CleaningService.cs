using System;
using UnityEngine;

public class CleaningService
{
    public event Action OnCleaningPerformed;
    public event Action OnCleaningEnded;

    public bool isCleaning;

    public void TryCleaning(ICleanSurfaceObject clean, Vector2 textureSurface, Texture2D brush)
    {
        if (clean == null || brush == null)
            return;

        isCleaning = true;

        clean.TryClean(textureSurface, brush);
        OnCleaningPerformed?.Invoke();
    }

    public void TryCleaningHardSurface(ICleanHardObject clean)
    {
        clean.Hit();
    }

    public void EndClean()
    {
        isCleaning = false;
    }
}
