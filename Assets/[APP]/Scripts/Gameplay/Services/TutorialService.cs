using System.Collections;
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using UnityEngine;

public class TutorialService
{
    private int currentStage = 0;
    private int currentModule = 0;
    private bool isTutorialActive = false;
    private bool isProcessing = false;

    public bool IsProcessing => isProcessing;
    public int CurrentStage => currentStage;
    public int CurrentModule => currentModule;

    [Inject]
    public TutorialService() { }

    public void StartTutorial(int sIndex, int mIndex, float initialDelay = 0.5f)
    {
        if (isProcessing) return;

        TutorialManager.Instance.StartCoroutine(StartTutorialRoutine(sIndex, mIndex, initialDelay));
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
}