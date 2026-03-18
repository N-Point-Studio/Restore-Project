using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

[Serializable]
public class ProgressPair
{
    public ProgressBarUI progressBar;
    public ProgressType progressType;
}

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private List<ProgressPair> progressBars;
    [SerializeField] private ButtonItemUI buttonWrapUp;
    [SerializeField] private ButtonItemUI buttonFinish;
    [SerializeField] private Camera toolCamera;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject endgameCanvas;
    private Dictionary<ProgressType, ProgressBarUI> progressMap;
    private CleaningService cleaningService;
    private FragmentService fragmentService;

    public event Action OnGameFinished;

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
        buttonWrapUp.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        fragmentService.OnProgressUpdate += HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate += HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate += HandleSurfaceCleaningUpdate;

        buttonWrapUp.OnClick += OnButtonWrapUpClick;
        buttonFinish.OnClick += OnFinishedGame;
    }

    private void OnDisable()
    {
        fragmentService.OnProgressUpdate -= HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate -= HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate -= HandleSurfaceCleaningUpdate;

        buttonWrapUp.OnClick -= OnButtonWrapUpClick;
        buttonFinish.OnClick -= OnFinishedGame;
    }

    private void HandleSurfaceCleaningUpdate(float progress) => UpdateProgress(ProgressType.Dust, progress);
    private void HandleHardCleaningUpdate(float progress) => UpdateProgress(ProgressType.Mud, progress);
    private void HandleProgressUpdate(float progress) => UpdateProgress(ProgressType.Assemble, progress);

    private void OnButtonWrapUpClick()
    {
        Debug.Log("Wrap up!");
        toolCamera.gameObject.SetActive(false);
        mainCanvas.SetActive(false);
        endgameCanvas.SetActive(true);
    }

    private void OnFinishedGame()
    {
        OnGameFinished?.Invoke();
    }

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
        buttonWrapUp.gameObject.SetActive(overall >= 100f);
    }
}
