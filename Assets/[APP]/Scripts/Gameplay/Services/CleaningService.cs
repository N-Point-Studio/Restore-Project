using System;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using System.Collections.Generic;
using Modules;

public class CleaningService : IInitializable, IDisposable
{
    private readonly HashSet<CleaningHardObject> cleaningChunks = new();
    private readonly HashSet<CleaningSurfaceObject> cleaningSurfaces = new();
    public bool isCleaning;

    private int totalHardObjects;
    private int destroyedHardObjects;

    public event Action<float> OnSurfaceCleaningUpdate;
    public event Action<float> OnHardCleaningUpdate;

    public void Initialize()
    {
        CleaningHardObject.OnCreated += RegisterChunk;
        CleaningHardObject.OnDestroy += CalculateChunks;
        CleaningSurfaceObject.OnCreated += RegisterSurface;
    }

    public void Dispose()
    {
        CleaningHardObject.OnCreated -= RegisterChunk;
        CleaningHardObject.OnDestroy -= CalculateChunks;
        CleaningSurfaceObject.OnCreated -= RegisterSurface;
    }

    public void RegisterSurface(CleaningSurfaceObject cleanObj)
    {
        cleaningSurfaces.Add(cleanObj);
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
        OnSurfaceCleaningUpdate?.Invoke(CalculateSurfaceProgress());
    }

    public void TryCleaningHardSurface(ICleanHardObject clean)
    {
        isCleaning = true;

        clean.Hit();
        OnHardCleaningUpdate?.Invoke(GetHardProgress());
    }

    public void EndClean()
    {
        isCleaning = false;
    }

    public void ForceCleanAll()
    {
        foreach (CleaningSurfaceObject surface in cleaningSurfaces)
        {
            if (surface != null) surface.ForceClean();
        }

        List<CleaningHardObject> chunksToDestroy = new List<CleaningHardObject>(cleaningChunks);
        
        for (int i = 0; i < chunksToDestroy.Count; i++)
        {
            CleaningHardObject chunk = chunksToDestroy[i];
            if (chunk != null) chunk.ForceClean();
        }

        OnSurfaceCleaningUpdate?.Invoke(1f);
        OnHardCleaningUpdate?.Invoke(1f);
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
