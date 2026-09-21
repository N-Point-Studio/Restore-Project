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
    public bool IsInputBlocked { get; private set; }

    public static Action<ToolType> OnTutorialHighlightOn;
    public static Action<ToolType> OnTutorialHighlightOff;
    public event Action<int, int> OnTutorialStateChanged;
    
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
        // if (IsMobilePlatform())
        // {
        //     AppLogger.Log($"[TutorialService] Mobile platform detected. Offsetting stage index by 4. Base: {baseStageIndex}, Actual: {baseStageIndex + 4}");
        // }
        // else
        // {
        //     AppLogger.Log($"[TutorialService] Non-mobile platform detected. Using base stage index: {baseStageIndex}");
        // }

        // If mobile, offset by 4 (Stage 0 becomes 4, Stage 1 becomes 5, etc.)
        return IsMobilePlatform() ? baseStageIndex + 4 : baseStageIndex;
    }

    private void NotifyStateChange()
    {
        OnTutorialStateChanged?.Invoke(currentStage, currentModule);
    }

    public void StartTutorial(int sIndex, int mIndex, float initialDelay = 0.5f)
    {
        if (isProcessing) return;
        // Debug.Log($"[TutorialService] Starting tutorial for Stage: {sIndex}, Module: {mIndex} with initial delay: {initialDelay}s");
        TutorialManager.Instance.StartCoroutine(StartTutorialRoutine(sIndex, mIndex, initialDelay));
        NotifyStateChange();
    }

    public void StartInstantTutorial(int sIndex, int mIndex)
    {
        if (isProcessing) return;

        currentStage = sIndex;
        currentModule = mIndex;

        int actualStage = GetActualStageIndex(sIndex);
        bool success = TutorialManager.Instance.ForceStageStarted(actualStage, mIndex);

        // AppLogger.Log($"[Tutorial] Force starting stage: {actualStage}, module: {mIndex} | Success: {success}");

        if (success)
        {
            isTutorialActive = true;
            isProcessing = false;
        }
        NotifyStateChange();
    }

    private IEnumerator StartTutorialRoutine(int sIndex, int mIndex, float delay)
    {
        isProcessing = true;
        isTutorialActive = false;

        currentStage = sIndex;
        currentModule = mIndex;

        int actualStage = GetActualStageIndex(sIndex);
        bool success = TutorialManager.Instance.ForceStageStarted(actualStage, mIndex);
        // Debug.Log($"[TutorialService] Instantly starting tutorial for Stage: {actualStage}, Module: {mIndex}, Success: {success}");


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
        NotifyStateChange();
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
        
        NotifyStateChange();
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

    public void SetInputBlock(bool isBlocked)
    {
        IsInputBlocked = isBlocked;
    }

    // --- GATEKEEPER METHODS ---
    // Stage 0 (Tutorial 1): 0=Drag, 1=Zoom, 2=Rotate, 3=Chisel, 4=Brush
    // Stage 3 (Tutorial 4): 0=Attach, 1=Detach
    
    public bool CanZoom() => currentStage > 0 || currentModule >= 1;
    public bool CanRotate() => currentStage > 0 || currentModule >= 2;
    
    public bool CanUseTool(ToolType toolType)
    {
        if (currentStage > 0) return true; // Unlocked after Tutorial 1
        
        if (toolType == ToolType.Chisel) return currentModule >= 3;
        if (toolType == ToolType.Brush) return currentModule >= 4;
        
        return false;
    }

    public bool CanDetach()
    {
        // Detach is taught in Tutorial 4 (Stage 3), Module 1
        if (currentStage < 3) return false; 
        if (currentStage == 3 && currentModule < 1) return false;
        return true; 
    }
}