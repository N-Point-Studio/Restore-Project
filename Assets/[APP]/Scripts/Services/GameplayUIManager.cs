using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using Modules;

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private List<ProgressBarUI> progressBars;
    [SerializeField] private Camera toolCamera;
    
    [Header("Controllers")]
    [SerializeField] private MainUIController mainUIController;
    [SerializeField] private EndgameUIController endgameController;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private PopUpConfirmationController quitConfirmationController;

    [Header("Settings")]
    [SerializeField] private float wrapUpThreshold;
    
    private CleaningService cleaningService;
    private FragmentService fragmentService;
    private InputSystemService input;

    private bool canWrapUp;

    public static event Action OnGameWrapped;
    public static event Action OnGameFinished;

    [Inject]
    public void Construct(CleaningService cleaningService, FragmentService fragmentService, InputSystemService input, IObjectResolver container)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
        this.input = input;
        
        fragmentService.OnProgressUpdate += HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate += HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate += HandleSurfaceCleaningUpdate;

        this.input.OnPlayerKeycodeEscapePerformed += OnPlayerKeycodeEscapePerformed;
        
        container.Inject(settingsController);
        container.Inject(endgameController);
        container.Inject(mainUIController);
    }

    private void Awake()
    {
        mainUIController.OnWrapUp += OnWrapUp;
        endgameController.OnFinishedGame += OnFinishedGame;

        pauseController.OnResume += OnResume;
        pauseController.OnOpenSettings += OnOpenSettings;
        pauseController.OnBackToMenu += OnBackToMenu;

        quitConfirmationController.OnConfirm += OnConfirmQuit;
        quitConfirmationController.OnCancel += OnCancelQuit;

        mainUIController.SetActive(true);
    }

    private void Start()
    {
        HandleAssembleAvailability();
        mainUIController.ShowButtonWrap(false);
    }

    private void OnDestroy()
    {
        fragmentService.OnProgressUpdate -= HandleProgressUpdate;
        cleaningService.OnHardCleaningUpdate -= HandleHardCleaningUpdate;
        cleaningService.OnSurfaceCleaningUpdate -= HandleSurfaceCleaningUpdate;

        input.OnPlayerKeycodeEscapePerformed -= OnPlayerKeycodeEscapePerformed;

        mainUIController.OnWrapUp -= OnWrapUp;
        endgameController.OnFinishedGame -= OnFinishedGame;
        
        pauseController.OnResume -= OnResume;
        pauseController.OnOpenSettings -= OnOpenSettings;
        pauseController.OnBackToMenu -= OnBackToMenu;
        
        quitConfirmationController.OnConfirm -= OnConfirmQuit;
        quitConfirmationController.OnCancel -= OnCancelQuit;
    }

    private void UpdateProgress(ProgressType type, float value)
    {
        for (int i = 0; i < progressBars.Count; i++)
        {
            if (progressBars[i].ProgressType == type)
            {
                progressBars[i].SetValue(value);
                CheckOverallProgress();
                break;
            }
        }
    }

    private void HandleAssembleAvailability()
    {
        if (fragmentService.GetPieceCount() <= 1)
        {
            for (int i = 0; i < progressBars.Count; i++)
            {
                if (progressBars[i].ProgressType == ProgressType.Assemble)
                {
                    progressBars[i].gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void CheckOverallProgress()
    {
        float total = 0f;
        int count = 0;

        for (int i = 0; i < progressBars.Count; i++)
        {
            if (!progressBars[i].gameObject.activeSelf) continue;
            total += progressBars[i].GetValue();
            count++;
        }
        if (count == 0) return;
        float overall = total / count;
        canWrapUp = overall >= wrapUpThreshold;
        mainUIController.EnableWrapUp(canWrapUp);
        mainUIController.ShowButtonWrap(canWrapUp);

        if (!TutorialManager.Instance.AllTutorialsCompleted && canWrapUp)
        {
            TutorialManager.Instance.StageStarted(1, 0);
        }
    }

    private void HandleProgressUpdate(float progress)
    {
        AppLogger.Log("Assemble harusnya");
        UpdateProgress(ProgressType.Assemble, progress);
    }

    private void HandleHardCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Mud, progress);
        //complete tutorial chisel
        if (!TutorialManager.Instance.AllTutorialsCompleted)
        {
            AppLogger.Log("Harusnya stage chisel selesai");
            TutorialManager.Instance.StageCompleted(0, 4);
        }
    }

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Dust, progress);
        //complete tutorial brush
        if (!TutorialManager.Instance.AllTutorialsCompleted)
        {
            AppLogger.Log("Harusnya stage brush selesai");
            TutorialManager.Instance.StageCompleted(0, 3);
        }
    }

    private void OnPlayerKeycodeEscapePerformed()
    {
        if (!pauseController.IsActive)
        {
            Time.timeScale = 0f;
            pauseController.SetActive(true);

            if (input != null)
            {
                input.ChangeInputState(InputStateType.UI);
            }
        }
    }

    private void OnWrapUp()
    {
        AppLogger.Log("Wrap up!");
        toolCamera.gameObject.SetActive(false);
        mainUIController.SetActive(false);
        endgameController.SetActive(true);
        
        if (input != null)
        {
            input.ChangeInputState(InputStateType.UI);
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

    private void OnResume()
    {
        Time.timeScale = 1f;
        pauseController.SetActive(false);

        if (endgameController.IsActive)
        {
            if (input != null)
            {
                input.ChangeInputState(InputStateType.UI);
            }
        }
        else
        {
            if (input != null)
            {
                input.ChangeInputState(InputStateType.Player);
            }            
        }
    }

    private void OnOpenSettings()
    {
        if (settingsController.IsActive)
            return;

        settingsController.SetActive(true);
    }

    private void OnBackToMenu()
    {
        if (quitConfirmationController.IsActive)
            return;
        
        quitConfirmationController.SetActive(true);
    }

    private void OnConfirmQuit()
    {
        Time.timeScale = 1f;
        // TODO:
        // This is quit from settings.
        // if canvas is end udah ada panggil buttonFinish.OnClick?.Invoke(); biar dia dari satu pintu aja
        // kalo belum end dan dia quit lgsg ke main menu (ini kan pasti belum ngesave apa2 asumsi ku sih)
    }

    private void OnCancelQuit()
    {
        quitConfirmationController.SetActive(false);
    }
}
