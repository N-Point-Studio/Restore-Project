using System.Collections.Generic; // Tambahkan ini untuk HashSet
using VContainer;
using NINESOFT.TUTORIAL_SYSTEM;
using Modules;

public class TutorialService
{
    private readonly PlayerProgressionData progressionData;

    private readonly HashSet<int> startedTutorials = new HashSet<int>();

    [Inject]
    public TutorialService(PlayerProgressionData progressionData)
    {
        this.progressionData = progressionData;
    }

    public void CompleteTutorial(int tutorialId, int prevTutorial, int stageIndex, int moduleIndex)
    {
        if (!progressionData.HasTutorialShown(tutorialId) && CheckTutorialAvailability(prevTutorial))
        {
            AppLogger.Log($"[TutorialService] Completing Tutorial ID: {tutorialId}");
            TutorialManager.Instance.StageCompleted(stageIndex, moduleIndex);

            progressionData.MarkTutorialShown(tutorialId);

            startedTutorials.Remove(tutorialId);
        }
    }

    public void StartTutorial(int tutorialId, int prevTutorial, int stageIndex, int moduleIndex)
    {
        if (!progressionData.HasTutorialShown(tutorialId) && !startedTutorials.Contains(tutorialId) && CheckTutorialAvailability(prevTutorial))
        {
            AppLogger.Log($"[TutorialService] Starting Tutorial ID: {tutorialId}");
            TutorialManager.Instance.StageStarted(stageIndex, moduleIndex);

            startedTutorials.Add(tutorialId);
        }
    }

    public bool CheckTutorialAvailability(int tutorialId)
    {
        if (tutorialId == -1) return true;
        return progressionData.HasTutorialShown(tutorialId);
    }
}