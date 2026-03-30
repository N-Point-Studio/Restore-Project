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
    private GameplayManager gameplayManager;

    private bool canWrapUp;
    private bool isAutoWrapUpTriggered = false;
    private bool isTutorialTriggered = false;
    private bool isGamePaused = false;

    public static event Action OnGameWrapped;
    public static event Action<bool> OnGameFinished;

    [Inject]
    public void Construct(
        CleaningService cleaningService,
        FragmentService fragmentService,
        InputSystemService input,
        TutorialService tutorialService,
        IObjectResolver container,
        GameplayManager gameplayManager)
    {
        this.cleaningService = cleaningService;
        this.fragmentService = fragmentService;
        this.input = input;
        this.tutorialService = tutorialService;
        this.gameplayManager = gameplayManager;

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

        settingsController.OnSettingsClosed += OnSettingsClosed;

        mainUIController.SetActive(true);
    }

    private IEnumerator Start()
    {
        mainUIController.ShowButtonWrap(false);

        yield return null;

        if (gameplayManager.isTutorialAvailable)
        {
            tutorialService.StartTutorial(0, 0);
        }
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

        settingsController.OnSettingsClosed -= OnSettingsClosed;
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

            if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 0)
            {
                tutorialService.StartTutorial(1, 0);
            }
        }

        if (isFullyCompleted && !isAutoWrapUpTriggered)
        {
            isAutoWrapUpTriggered = true;
            mainUIController.ShowButtonWrap(false);

            DG.Tweening.DOVirtual.DelayedCall(delayUntilAutoWrappedUp, () =>
            {
                if (mainUIController != null && mainUIController.IsActive)
                {
                    AppLogger.Log("Manggil auto onwrap");
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

        if (tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 4)
        {
            tutorialService.CompleteStage();
        }
    }

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Dust, progress);
        if (tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 3)
        {
            tutorialService.CompleteAndAdvance();
        }
    }

    private void OnPlayerKeycodeEscapePerformed()
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
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

        if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 0)
        {
            tutorialService.CompleteStage();
        }
    }

    private void OnFinishedGame()
    {
        OnGameFinished?.Invoke(true);
    }

    private void OnResume()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        pauseController.SetActive(false);

        if (endgameController.IsActive)
        {
            if (input != null) input.ChangeInputState(InputStateType.UI);
        }
        else
        {
            if (input != null) input.ChangeInputState(InputStateType.Player);
        }
    }

    private void OnOpenSettings()
    {
        if (settingsController.IsActive)
            return;

        pauseController.SetActive(false);
        settingsController.SetActive(true);
    }

    private void OnBackToMenu()
    {
        if (quitConfirmationController.IsActive)
            return;

        pauseController.SetActive(false);

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

        if (isGamePaused)
        {
            pauseController.SetActive(true);
        }
    }

    private void OnSettingsClosed()
    {
        if (isGamePaused)
        {
            pauseController.SetActive(true);
        }
    }
}
