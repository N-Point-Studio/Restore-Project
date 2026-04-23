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

    private readonly HashSet<ICleanSurface> cleanSurfaces = new();
    private readonly HashSet<ICleanChunk> cleanChunks = new();

    public bool isCleaning;

    private int totalHardObjects;
    private int destroyedHardObjects;

    public event Action<float> OnSurfaceCleaningUpdate;
    public event Action<float> OnHardCleaningUpdate;

    public void Initialize()
    {
        //new 
        CleaningChunk.OnCreated += RegisterChunk;
        CleaningChunk.OnDestroyed += CalculateChunks;
        CleaningSurface.OnCreated += RegisterSurface;

        //old
        // CleaningHardObject.OnCreated += RegisterChunk;
        // CleaningHardObject.OnDestroy += CalculateChunks;
        // CleaningSurfaceObject.OnCreated += RegisterSurface;
    }

    public void Dispose()
    {
        //new
        CleaningChunk.OnCreated -= RegisterChunk;
        CleaningChunk.OnDestroyed -= CalculateChunks;
        CleaningSurface.OnCreated -= RegisterSurface;

        //old
        // CleaningHardObject.OnCreated -= RegisterChunk;
        // CleaningHardObject.OnDestroy -= CalculateChunks;
        // CleaningSurfaceObject.OnCreated -= RegisterSurface;
    }

    private void RegisterChunk(CleaningChunk chunk)
    {
        cleanChunks.Add(chunk);
        totalHardObjects++;
    }

    private void CalculateChunks(CleaningChunk chunk)
    {
        if (cleanChunks.Remove(chunk))
        {
            destroyedHardObjects++;
        }
    }

    private void RegisterSurface(ICleanSurface surface)
    {
        Debug.Log("Registering clean surface: " + surface);
        cleanSurfaces.Add(surface);
    }

    // public void RegisterSurface(CleaningSurfaceObject cleanObj)
    // {
    //     cleaningSurfaces.Add(cleanObj);
    // }

    // public void RegisterChunk(CleaningHardObject cleanObj)
    // {
    //     if (cleaningChunks.Add(cleanObj))
    //     {
    //         totalHardObjects++;
    //     }
    // }

    // public void CalculateChunks(CleaningHardObject cleanObj)
    // {
    //     if (cleaningChunks.Remove(cleanObj))
    //     {
    //         destroyedHardObjects++;
    //     }
    // }

    public void CleanSurface(ICleanSurface surface, Texture2D brush, Vector3 hitPoint, Vector3 hitNormal, Vector3 direction, float scale, float strength, Color color)
    {
        // Debug.Log("Cleaning surface with brush tool: " + surface);
        if (!cleanSurfaces.Contains(surface)) return;
        surface?.CleanSurface(hitPoint, brush, hitNormal, direction, scale, strength);
        OnSurfaceCleaningUpdate?.Invoke(CalculateSurfaceProgress());
    }

    public void CleanChunk(ICleanChunk chunk)
    {
        if (!cleanChunks.Contains(chunk)) return;
        chunk?.Hit();
        OnHardCleaningUpdate?.Invoke(GetHardProgress());
    }

    // public void CleanChunk(ICleanHardObject chunk)
    // {
    //     isCleaning = true;
    //     chunk.Hit();
    //     OnHardCleaningUpdate?.Invoke(GetHardProgress());
    // }

    // public void TryCleaning(ICleanSurfaceObject clean, Vector2 textureSurface, Texture2D brush)
    // {
    //     if (clean == null || brush == null)
    //         return;

    //     isCleaning = true;

    //     clean.TryClean(textureSurface, brush);
    //     OnSurfaceCleaningUpdate?.Invoke(CalculateSurfaceProgress());
    // }

    // public void TryCleaningHardSurface(ICleanHardObject clean)
    // {
    //     isCleaning = true;

    //     clean.Hit();
    //     OnHardCleaningUpdate?.Invoke(GetHardProgress());
    // }

    // public void EndClean()
    // {
    //     isCleaning = false;
    // }

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
        if (cleanSurfaces.Count == 0)
            return 1f;

        float total = 0;

        foreach (var surface in cleanSurfaces)
        {
            total += surface.GetCleaningProgress();
        }
        float averagePercentage = total / cleanSurfaces.Count;
        return averagePercentage / 100;
    }

    public float GetHardProgress()
    {
        if (totalHardObjects == 0)
            return 1f;

        return (float)destroyedHardObjects / totalHardObjects;
    }

}
