using System;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using System.Collections.Generic;

public class CleaningService : IInitializable, IDisposable
{
    public event Action OnCleaningPerformed;
    public event Action OnCleaningEnded;

    private readonly HashSet<CleaningHardObject> cleaningChunks = new();
    private readonly HashSet<CleaningSurfaceObject> cleaningSurfaces = new();
    public bool isCleaning;

    private int totalHardObjects;
    private int destroyedHardObjects;

    public void Initialize()
    {
        CleaningHardObject.OnCreated += RegisterChunk;
        CleaningHardObject.OnDestroy += CalculateChunks;
        CleaningSurfaceObject.OnCreated += RegisterSurface;
    }

    public void Dispose()
    {
        CleaningHardObject.OnCreated -= RegisterChunk;
        CleaningHardObject.OnDestroy += CalculateChunks;
        CleaningSurfaceObject.OnCreated -= RegisterSurface;
    }

    public void RegisterSurface(CleaningSurfaceObject cleanObj)
    {
        if (cleaningSurfaces.Add(cleanObj)) Debug.Log($"Registered: {cleanObj.name}");
    }

    public void RegisterChunk(CleaningHardObject cleanObj)
    {
        if (cleaningChunks.Add(cleanObj))
        {
            totalHardObjects++;
        }
    }

    public void CalculateChunks(CleaningHardObject cleanObj)
    {
        if (cleaningChunks.Remove(cleanObj))
        {
            destroyedHardObjects++;
        }
    }

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

    private float CalculateSurfaceProgress()
    {
        if (cleaningSurfaces.Count == 0)
            return 1f;

        float total = 0;

        foreach (var surface in cleaningSurfaces)
        {
            total += surface.GetCleanProgress();
        }

        return total / cleaningSurfaces.Count;
    }

    public float GetHardProgress()
    {
        if (totalHardObjects == 0)
            return 1f;

        return (float)destroyedHardObjects / totalHardObjects;
    }

}
