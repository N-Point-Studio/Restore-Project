using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[Serializable]
public class ProgressPair
{
    public ProgressBarUI progressBar;
    public ProgressType progressType;
}

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] public List<ProgressPair> progressBars;
    private CleaningService cleaningService;
    private FragmentService fragmentService;

    [Inject]
    public void Construct(CleaningService cleaningService, FragmentService fragmentService)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
    }

    private void OnEnable()
    {
        fragmentService.OnProgressUpdate += HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate += HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate += HandleSurfaceCleaningUpdate;
    }

    private void OnDisable()
    {
        fragmentService.OnProgressUpdate -= HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate -= HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate -= HandleSurfaceCleaningUpdate;
    }

    private void HandleSurfaceCleaningUpdate(float progress) => UpdateProgress(ProgressType.Dust, progress);
    private void HandleHardCleaningUpdate(float progress) => UpdateProgress(ProgressType.Mud, progress);
    private void HandleProgressUpdate(float progress) => UpdateProgress(ProgressType.Assemble, progress);

    private void UpdateProgress(ProgressType type, float value)
    {
        foreach (var pair in progressBars)
        {
            if (pair.progressType == type)
            {
                pair.progressBar.SetValue(value);
                return;
            }
        }
    }
}
