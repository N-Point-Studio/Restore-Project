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

    public static Action<string> OnTutorialHighlightOn;
    public static Action<string> OnTutorialHighlightOff;

    [Inject]
    public TutorialService() { }

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
        bool success = TutorialManager.Instance.ForceStageStarted(sIndex, mIndex);

        AppLogger.Log("berhasil: " + success);

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

        bool success = TutorialManager.Instance.StageStarted(sIndex, mIndex);

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
        TutorialManager.Instance.StageCompleted(currentStage, currentModule);

        yield return new WaitForSeconds(delay);

        currentModule++;

        isProcessing = false;

        if (isNext) StartTutorial(currentStage, currentModule);
    }

    public void CompleteStage()
    {
        TutorialManager.Instance.StageCompleted(currentStage, currentModule);
        currentStage++;
        currentModule = 0;
    }

    public void TriggerHighlight(bool isOn, string objId)
    {
        if (isOn)
        {
            OnTutorialHighlightOn.Invoke(objId);
        }
        else
        {
            OnTutorialHighlightOff.Invoke(objId);
        }
    }
}