using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class ProgressService : IInitializable, IDisposable
{
    private readonly Dictionary<ProgressType, ProgressBarUI> progressBars = new();
    private readonly CleaningService cleaningService;
    private readonly FragmentService fragmentService;

    [Inject]
    public ProgressService(CleaningService cleaningService, FragmentService fragmentService)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
    }

    public void Initialize()
    {
        ProgressBarUI.OnCreated += Register;
        fragmentService.OnProgressUpdate += HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate += HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate += HandleSurfaceCleaningUpdate;
    }

    public void Dispose()
    {
        ProgressBarUI.OnCreated -= Register;
        fragmentService.OnProgressUpdate -= HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate -= HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate -= HandleSurfaceCleaningUpdate;
    }

    private void Register(ProgressBarUI progressBar)
    {
        if (progressBars.ContainsKey(progressBar.progressType))
            return;

        progressBars.Add(progressBar.progressType, progressBar);
        Debug.Log("Adding " + progressBar);
    }

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        //untuk progress bar tipe dust
        UpdateProgress(ProgressType.Dust, progress);
    }

    private void HandleHardCleaningUpdate(float progress)
    {
        //untuk progress bar tipe mud
        UpdateProgress(ProgressType.Mud, progress);
    }

    private void HandleProgressUpdate(float progress)
    {
        //untuk progress bar tipe assemble
        UpdateProgress(ProgressType.Assemble, progress);
    }

    private void UpdateProgress(ProgressType type, float value)
    {
        if (progressBars.TryGetValue(type, out var bar))
        {
            bar.SetValue(value);
        }
    }
}
