using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using Modules;
using System.Collections;
using UnityEngine.InputSystem;

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private List<ProgressBarUI> progressBars;

    [Header("Cameras")]
    [SerializeField] private Camera endCamera;

    [Header("Controllers")]
    [SerializeField] private MainUIController mainUIController;
    [SerializeField] private EndgameUIController endgameController;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private PopUpConfirmationController quitConfirmationController;

    [Header("Tutorial Canvas Configuration")]
    [SerializeField] private GameObject tutorialDesktopRoot;
    [SerializeField] private GameObject tutorialMobileRoot;

    [Header("Settings")]
    [SerializeField] private float delayUntilAutoWrappedUp = 1.5f;
    [SerializeField] private float wrapUpThreshold = 95f;

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
    public static event Action<float> OnOverallProgressUpdated;


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
        this.input.OnUIKeycodeEscapePerformed += OnUIKeycodeEscapePerformed;

        container.Inject(settingsController);
        container.Inject(endgameController);
        container.Inject(mainUIController);
    }

    private void Awake()
    {
        mainUIController.OnWrapUp += OnWrapUp;
        mainUIController.OnPauseRequest += PauseGame;
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
        endCamera.enabled = false;

        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif

        if (tutorialDesktopRoot != null) tutorialDesktopRoot.SetActive(!isMobile);
        if (tutorialMobileRoot != null) tutorialMobileRoot.SetActive(isMobile);

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
        input.OnUIKeycodeEscapePerformed -= OnUIKeycodeEscapePerformed;

        mainUIController.OnWrapUp -= OnWrapUp;
        mainUIController.OnPauseRequest -= PauseGame;
        endgameController.OnFinishedGame -= OnFinishedGame;

        pauseController.OnResume -= OnResume;
        pauseController.OnOpenSettings -= OnOpenSettings;
        pauseController.OnBackToMenu -= OnBackToMenu;

        quitConfirmationController.OnConfirm -= OnConfirmQuit;
        quitConfirmationController.OnCancel -= OnCancelQuit;

        settingsController.OnSettingsClosed -= OnSettingsClosed;
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // [DEBUG CHEAT] Press F1 to instantly show the Wrap Up button
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            // AppLogger.Log("[Cheat] F1 Pressed! Forcing Wrap Up button to appear.");

            // Bypass the normal progress checks
            canWrapUp = true;
            isAutoWrapUpTriggered = false; // Ensure auto-wrap doesn't conflict

            // Force the UI controller to show and enable the button
            if (mainUIController != null)
            {
                mainUIController.EnableWrapUp(true);
                mainUIController.ShowButtonWrap(true);
            }
        }
#endif
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
        float surfaceProgress = 0;

        foreach (var progress in progressBars)
        {
            if (!progress.gameObject.activeSelf) continue;
            if (progress.ProgressType == ProgressType.Dust) surfaceProgress = progress.GetValue();
            if (progress) total += progress.GetValue();
            count++;
        }

        if (count == 0) return;
        float overall = total / count;

        OnOverallProgressUpdated?.Invoke(surfaceProgress);
        canWrapUp = overall >= wrapUpThreshold;
        bool isFullyCompleted = overall >= 99f;

        bool shouldShowHintToggle = surfaceProgress >= gameplayManager.clueTreshold;

        if (shouldShowHintToggle)
        {
            if (gameplayManager.isTutorialAvailable)
            {
                if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 0)
                    tutorialService.StartTutorial(1, 0);
            }
            else
            {
                AppLogger.Log("Masuk ke treshold Non Tutorial");

                if (tutorialService.CurrentStage != 2)
                {
                    tutorialService.LockTutorialState();
                }
            }
        }

        mainUIController.ShowHintToggle(shouldShowHintToggle);

        bool showButton = canWrapUp && !isFullyCompleted && !isAutoWrapUpTriggered;
        mainUIController.EnableWrapUp(showButton);
        mainUIController.ShowButtonWrap(showButton);

        // 2. TRIGGER TUTORIAL
        if (canWrapUp)
        {
            isTutorialTriggered = true;
            if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 1)
            {
                tutorialService.StartTutorial(1, 1);
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
                    // AppLogger.Log("Manggil auto onwrap");
                    OnWrapUp();
                }
            });
        }
    }

    private void PauseGame()
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

    private void HandleProgressUpdate(float progress)
    {
        // AppLogger.Log("Assemble should be");
        UpdateProgress(ProgressType.Assemble, progress);
    }

    private void HandleHardCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Mud, progress);

        if (tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 3)
        {
            tutorialService.CompleteAndAdvance(true);
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
            tutorialService.TriggerHighlight(false, ToolType.Chisel);
#endif
            tutorialService.TriggerHighlight(true, ToolType.Brush);
        }
    }

    private void HandleSurfaceCleaningUpdate(float progress)
    {
        UpdateProgress(ProgressType.Dust, progress);
        if (tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 4)
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
            tutorialService.TriggerHighlight(false, ToolType.Brush);
#endif
            tutorialService.CompleteStage();
        }
    }

    private void OnPlayerKeycodeEscapePerformed()
    {
        PauseGame();
    }

    private void OnUIKeycodeEscapePerformed()
    {
        if (isGamePaused && pauseController.IsActive)
        {
            OnResume();
        }
    }

    private void OnWrapUp()
    {
        if (endgameController.IsActive) return;
        endCamera.enabled = true;

        AppLogger.Log("Wrap up!");
        if (tutorialService.CurrentStage == 1 && tutorialService.CurrentModule == 1)
        {
            tutorialService.CompleteStage();
        }

        if (cleaningService != null)
        {
            cleaningService.ForceCleanAll();
        }

        mainUIController.SetActive(false);
        endgameController.SetActive(true);

        if (input != null)
        {
            input.ChangeInputState(InputStateType.UI);
        }

        OnGameWrapped?.Invoke();
        AudioEvents.TriggerPlayCustomSFX(Modules.SoundSystems.AudioKey.SFX_Finish);
    }

    private void OnFinishedGame()
    {
        OnGameFinished?.Invoke(true);
        tutorialService.CompleteStage();
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
