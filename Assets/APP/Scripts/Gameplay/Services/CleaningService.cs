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
    private InputSystemService inputSystemService;
    private GameplayManager gameplayManager;

    public bool isCleaning;

    private int totalHardObjects;
    private int destroyedHardObjects;
    private float overallProgress;

    public event Action<float> OnSurfaceCleaningUpdate;
    public event Action<float> OnHardCleaningUpdate;

    [Inject]
    public void Construct(InputSystemService input, GameplayManager gameplayManager)
    {
        inputSystemService = input;
        this.gameplayManager = gameplayManager;
    }

    public void Initialize()
    {
        CleaningChunk.OnCreated += RegisterChunk;
        CleaningChunk.OnDestroyed += CalculateChunks;
        CleaningSurface.OnCreated += RegisterSurface;
        CleaningSurface.OnForceClean += HandleOnForceClean;

        InteractionEvents.OnTabPerformed += HandleKeycodeTabPerformed;
        InteractionEvents.OnTabCanceled += HandleKeycodeTabCanceled;

        GameplayUIManager.OnOverallProgressUpdated += HandleOveralProgressUpdated;

    }

    public void Dispose()
    {
        CleaningChunk.OnCreated -= RegisterChunk;
        CleaningChunk.OnDestroyed -= CalculateChunks;
        CleaningSurface.OnCreated -= RegisterSurface;
        CleaningSurface.OnForceClean -= HandleOnForceClean;

        InteractionEvents.OnTabPerformed -= HandleKeycodeTabPerformed;
        InteractionEvents.OnTabCanceled -= HandleKeycodeTabCanceled;

        GameplayUIManager.OnOverallProgressUpdated -= HandleOveralProgressUpdated;
    }

    private void HandleOnForceClean(ICleanSurface surface)
    {
        OnSurfaceCleaningUpdate?.Invoke(CalculateSurfaceProgress());
    }

    private void HandleOveralProgressUpdated(float obj)
    {
        overallProgress = obj;
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
        // Debug.Log("Registering clean surface: " + surface);
        cleanSurfaces.Add(surface);
    }

    private void HandleKeycodeTabPerformed()
    {
        // AppLogger.Log("Tab kepencet di celaning service " + overallProgress + " < " + GameplayUIManager.clueEnableTreshold);
        if (overallProgress < gameplayManager.clueTreshold) return;
        // AppLogger.Log("[HARUSNYA] Tab nyala cleaning");
        foreach (ICleanSurface surface in cleanSurfaces)
        {
            surface.ShowClue(true);
        }
    }

    private void HandleKeycodeTabCanceled()
    {
        if (overallProgress < gameplayManager.clueTreshold) return;
        foreach (ICleanSurface surface in cleanSurfaces)
        {
            surface.ShowClue(false);
        }
    }

    public void CleanSurface(ICleanSurface surface, Texture2D brush, Vector3 hitPoint, Vector3 hitNormal, Vector3 direction, float scale, float strength, Color color, float brushDepth)
    {
        if (!cleanSurfaces.Contains(surface)) return;
        // AppLogger.Log("[CLEANING] triggered");
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

    public bool IsDustRemoved(ICleanSurface surface)
    {
        return surface.IsDustRemoved();
    }
}