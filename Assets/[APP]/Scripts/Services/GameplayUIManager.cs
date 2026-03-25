using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;

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
    private InputSystemService inputSystemService;

    public static event Action OnGameWrapped;
    public static event Action OnGameFinished;

    [Inject]
    public void Construct(CleaningService cleaningService, FragmentService fragmentService, InputSystemService inputSystemService)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
        this.inputSystemService = inputSystemService;
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

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Dust, progress);
        //complete tutorial brush
        if (!TutorialManager.Instance.AllTutorialsCompleted)
        {
            Debug.Log("Harusnya stage brush selesai");
            TutorialManager.Instance.StageCompleted(0, 3);
        }
    }
    private void HandleHardCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Mud, progress);
        //complete tutorial chisel
        if (!TutorialManager.Instance.AllTutorialsCompleted)
        {
            Debug.Log("Harusnya stage chisel selesai");
            TutorialManager.Instance.StageCompleted(0, 4);
        }
    }
    private void HandleProgressUpdate(float progress)
    {
        Debug.Log("Assemble harusnya");
        UpdateProgress(ProgressType.Assemble, progress);
    }

    private void OnButtonWrapUpClick()
    {
        Debug.Log("Wrap up!");
        toolCamera.gameObject.SetActive(false);
        mainCanvas.SetActive(false);
        endgameCanvas.SetActive(true);
        
        if (inputSystemService != null)
        {
            inputSystemService.ChangeInputState(InputStateType.UI);
        }

        OnGameWrapped?.Invoke();
        if (!TutorialManager.Instance.AllTutorialsCompleted)
        {
            TutorialManager.Instance.StageCompleted(1, 0);
        }
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
        buttonWrapUp.gameObject.SetActive(overall >= 95f);

        if (!TutorialManager.Instance.AllTutorialsCompleted && overall >= 95f)
        {
            TutorialManager.Instance.StageStarted(1, 0);
        }
    }
}
