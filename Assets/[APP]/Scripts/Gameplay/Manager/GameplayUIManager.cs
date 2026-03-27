using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using Modules;
using System.Collections;

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
    [SerializeField] private float wrapUpThreshold = 95f;
    [SerializeField] private float delayUntilAutoWrappedUp = 1.5f;

    private CleaningService cleaningService;
    private FragmentService fragmentService;
    private InputSystemService input;
    private TutorialService tutorialService;

    private bool canWrapUp;
    private bool isAutoWrapUpTriggered = false;
    private bool isTutorialTriggered = false;

    public static event Action OnGameWrapped;
    public static event Action<bool> OnGameFinished;

    [Inject]
    public void Construct(
        CleaningService cleaningService,
        FragmentService fragmentService,
        InputSystemService input,
        TutorialService tutorialService,
        IObjectResolver container)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
        this.input = input;
        this.tutorialService = tutorialService;

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

    private IEnumerator Start()
    {
        mainUIController.ShowButtonWrap(false);

        yield return null;

        tutorialService.StartTutorial(TutorialIDs.DRAG_TO_INSPECT, 0, 0);
        HandleAssembleAvailability();
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
                if (!progressBars[i].gameObject.activeSelf) continue;
                
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
        bool isFullyCompleted = overall >= 99f;

        bool showButton = canWrapUp && !isFullyCompleted && !isAutoWrapUpTriggered;
        mainUIController.EnableWrapUp(showButton);
        mainUIController.ShowButtonWrap(showButton);

        // 2. TRIGGER TUTORIAL
        if (canWrapUp && !isTutorialTriggered)
        {
            isTutorialTriggered = true;
            tutorialService.StartTutorial(TutorialIDs.WRAP_UP_SHOW, 1, 0);
        }

        if (isFullyCompleted && !isAutoWrapUpTriggered)
        {
            isAutoWrapUpTriggered = true;
            mainUIController.ShowButtonWrap(false);
            
            DG.Tweening.DOVirtual.DelayedCall(delayUntilAutoWrappedUp, () => 
            {
                if (mainUIController != null && mainUIController.IsActive)
                {
                    OnWrapUp();
                }
            });
        }
    }

    private void HandleProgressUpdate(float progress)
    {
        AppLogger.Log("Assemble should be");
        UpdateProgress(ProgressType.Assemble, progress);
    }

    private void HandleHardCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Mud, progress);
        tutorialService.CompleteTutorial(TutorialIDs.CHISEL_MUD, 0, 4);
    }

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Dust, progress);
        tutorialService.CompleteTutorial(TutorialIDs.BRUSH_DUST, 0, 3);
        tutorialService.StartTutorial(TutorialIDs.CHISEL_MUD, 0, 4);
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
        if (endgameController.IsActive) return;
        
        AppLogger.Log("Wrap up!");
        
        if (cleaningService != null)
        {
            cleaningService.ForceCleanAll();
        }

        toolCamera.gameObject.SetActive(false);
        mainUIController.SetActive(false);
        endgameController.SetActive(true);

        if (input != null)
        {
            input.ChangeInputState(InputStateType.UI);
        }

        OnGameWrapped?.Invoke();

        tutorialService.CompleteTutorial(TutorialIDs.WRAP_UP_CLICK, 1, 0);
    }

    private void OnFinishedGame()
    {
        OnGameFinished?.Invoke(true);
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
        quitConfirmationController.SetActive(false);

        if (endgameController.IsActive)
        {
            OnFinishedGame();
        }
        else
        {
            OnGameFinished?.Invoke(false);
        }
    }

    private void OnCancelQuit()
    {
        quitConfirmationController.SetActive(false);
    }
}
