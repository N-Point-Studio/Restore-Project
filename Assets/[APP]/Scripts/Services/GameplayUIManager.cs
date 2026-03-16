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
    private Dictionary<ProgressType, ProgressBarUI> progressMap;
    private CleaningService cleaningService;
    private FragmentService fragmentService;

    [Inject]
    public void Construct(CleaningService cleaningService, FragmentService fragmentService)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
    }

    private void Awake()
    {
        progressMap = new Dictionary<ProgressType, ProgressBarUI>();
        foreach (var pair in progressBars)
        {
            progressMap[pair.progressType] = pair.progressBar;
        }
    }

    private void Start()
    {
        HandleAssembleAvailability();
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
        if (progressMap.TryGetValue(type, out var bar))
        {
            bar.SetValue(value);
            CheckOverallProgress();
        }
    }

    private void HandleAssembleAvailability()
    {
        if (fragmentService.GetPieceCount() <= 1 && progressMap.TryGetValue(ProgressType.Assemble, out var bar))
            bar.gameObject.SetActive(false);
    }

    private void CheckOverallProgress()
    {
        float total = 0f;
        int count = 0;

        foreach (var bar in progressMap.Values)
        {
            if (!bar.gameObject.activeSelf) continue;
            total += bar.GetValue();
            count++;
        }
        if (count == 0) return;
        float overall = total / count;

        if (overall >= 1f) OnAllProgressCompleted();
    }

    private void OnAllProgressCompleted()
    {
        Debug.Log("All progress completed!");
    }
}
