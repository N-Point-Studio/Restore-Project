using System.Collections;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using UnityEngine;
using Modules;
using System;

public class TutorialService
{
    private int currentStage = 0;
    private int currentModule = 0;
    private bool isTutorialActive = false;
    private bool isProcessing = false;

    public bool IsProcessing => isProcessing;
    public int CurrentStage => currentStage;
    public int CurrentModule => currentModule;

    public static Action<ToolType> OnTutorialHighlightOn;
    public static Action<ToolType> OnTutorialHighlightOff;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnTutorialHighlightOn = null;
        OnTutorialHighlightOff = null;
    }

    [Inject]
    public TutorialService() { }

    private bool IsMobilePlatform()
    {
        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif
        return isMobile;
    }

    private int GetActualStageIndex(int baseStageIndex)
    {
        // If mobile, offset by 4 (Stage 0 becomes 4, Stage 1 becomes 5, etc.)
        return IsMobilePlatform() ? baseStageIndex + 4 : baseStageIndex;
    }

    public void StartTutorial(int sIndex, int mIndex, float initialDelay = 0.5f)
    {
        if (isProcessing) return;

        TutorialManager.Instance.StartCoroutine(StartTutorialRoutine(sIndex, mIndex, initialDelay));
    }

    public void StartInstantTutorial(int sIndex, int mIndex)
    {
        if (isProcessing) return;

        currentStage = sIndex;
        currentModule = mIndex;
        
        int actualStage = GetActualStageIndex(sIndex);
        bool success = TutorialManager.Instance.ForceStageStarted(actualStage, mIndex);

        AppLogger.Log($"[Tutorial] Force starting stage: {actualStage}, module: {mIndex} | Success: {success}");

        if (success)
        {
            isTutorialActive = true;
            isProcessing = false;
        }
    }

    private IEnumerator StartTutorialRoutine(int sIndex, int mIndex, float delay)
    {
        isProcessing = true;
        isTutorialActive = false;

        currentStage = sIndex;
        currentModule = mIndex;

        int actualStage = GetActualStageIndex(sIndex);
        bool success = TutorialManager.Instance.StageStarted(actualStage, mIndex);

        if (success)
        {
            yield return new WaitForSeconds(delay);

            isTutorialActive = true;
            isProcessing = false;
        }
        else
        {
            isProcessing = false;
        }
    }

    public void CompleteAndAdvance(bool isNext, float transitionDelay = 0.5f)
    {
        if (!isTutorialActive || isProcessing) return;

        TutorialManager.Instance.StartCoroutine(CompleteRoutine(transitionDelay, isNext));
    }

    private IEnumerator CompleteRoutine(float delay, bool isNext)
    {
        isProcessing = true;
        isTutorialActive = false;
        
        int actualStage = GetActualStageIndex(currentStage);
        TutorialManager.Instance.StageCompleted(actualStage, currentModule);

        yield return new WaitForSeconds(delay);

        currentModule++;

        isProcessing = false;

        if (isNext) StartTutorial(currentStage, currentModule);
    }

    public void CompleteStage()
    {
        int actualStage = GetActualStageIndex(currentStage);
        TutorialManager.Instance.StageCompleted(actualStage, currentModule);
        currentStage++;
        currentModule = 0;
    }

    public void LockTutorialState()
    {
        currentStage = 2;
        isTutorialActive = false;
        isProcessing = false;
    }

    public void TriggerHighlight(bool isOn, ToolType toolType)
    {
        if (isOn)
        {
            OnTutorialHighlightOn?.Invoke(toolType);
        }
        else
        {
            OnTutorialHighlightOff?.Invoke(toolType);
        }
    }
}