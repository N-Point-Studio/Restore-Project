using NINESOFT.TUTORIAL_SYSTEM;
using UnityEngine;

public class TutorialEvents : MonoBehaviour
{
    public static System.Action OnTutorialManagerReady;
    public static System.Action<Tutorial> OnTutorialStarted;
    public static System.Action<Tutorial> OnTutorialFinished;
    public static System.Action<TutorialStage> OnStageStarted;
    public static System.Action<TutorialStage> OnStageFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        OnTutorialManagerReady = null;
        OnTutorialStarted = null;
        OnTutorialFinished = null;
        OnStageStarted = null;
        OnStageFinished = null;
    }

    public void TutorialStart(Tutorial tutorial)
    {
        OnTutorialStarted?.Invoke(tutorial);
    }

    public void TutorialFinish(Tutorial tutorial)
    {
        OnTutorialFinished?.Invoke(tutorial);
    }

    public void StageStart(TutorialStage tutorialStage)
    {
        OnStageStarted?.Invoke(tutorialStage);
    }

    public void StageFinish(TutorialStage tutorialStage)
    {
        OnStageFinished?.Invoke(tutorialStage);
    }
}