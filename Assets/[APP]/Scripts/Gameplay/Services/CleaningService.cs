using System;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using System.Collections.Generic;
using Modules;

public class CleaningService : IInitializable, IDisposable
{
    private readonly HashSet<ICleanSurface> cleanSurfaces = new();
    private readonly HashSet<ICleanChunk> cleanChunks = new();

    public bool isCleaning;

    private int totalHardObjects;
    private int destroyedHardObjects;

    public event Action<float> OnSurfaceCleaningUpdate;
    public event Action<float> OnHardCleaningUpdate;

    public void Initialize()
    {
        CleaningChunk.OnCreated += RegisterChunk;
        CleaningChunk.OnDestroyed += CalculateChunks;
        CleaningSurface.OnCreated += RegisterSurface;
    }

    public void Dispose()
    {
        CleaningChunk.OnCreated -= RegisterChunk;
        CleaningChunk.OnDestroyed -= CalculateChunks;
        CleaningSurface.OnCreated -= RegisterSurface;
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

    public void CleanSurface(ICleanSurface surface, Texture2D brush, Vector3 hitPoint, Vector3 hitNormal, Vector3 direction, float scale, float strength, Color color)
    {
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

    public void ForceCleanAll()
    {
        foreach (ICleanSurface surface in cleanSurfaces)
        {
            if (surface != null) surface.ForceClean();
        }

        List<ICleanChunk> chunksToDestroy = new List<ICleanChunk>(cleanChunks);

        for (int i = 0; i < chunksToDestroy.Count; i++)
        {
            ICleanChunk chunk = chunksToDestroy[i];
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