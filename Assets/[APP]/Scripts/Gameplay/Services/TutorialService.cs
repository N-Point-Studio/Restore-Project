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

    // Properti tambahan untuk dicek oleh class lain
    public bool IsProcessing => isProcessing;
    public int CurrentStage => currentStage;
    public int CurrentModule => currentModule;

    [Inject]
    public TutorialService() { }

    public void StartTutorial(int sIndex, int mIndex)
    {
        // Jangan start kalau masih proses transisi dari tutorial sebelumnya
        if (isProcessing) return;

        currentStage = sIndex;
        currentModule = mIndex;

        bool success = TutorialManager.Instance.StageStarted(sIndex, mIndex);
        if (success)
        {
            isTutorialActive = true;
            Debug.Log($"<color=cyan>[Tutorial] Started: {sIndex},{mIndex}</color>");
        }

        //wait sekian detik untuk bisa completeadvance
    }

    public void CompleteAndAdvance(float transitionDelay = 0.5f)
    {
        // Jika sedang sibuk atau tidak aktif, blokir total
        if (!isTutorialActive || isProcessing) return;

        TutorialManager.Instance.StartCoroutine(CompleteRoutine(transitionDelay));
    }

    private IEnumerator CompleteRoutine(float delay)
    {
        isProcessing = true; // KUNCI GERBANG
        isTutorialActive = false;

        Debug.Log($"<color=green>[Tutorial] Completing: {currentStage},{currentModule}. Delay: {delay}s</color>");

        // Perintah ke NINESOFT
        TutorialManager.Instance.StageCompleted(currentStage, currentModule);

        // Tunggu transisi selesai
        yield return new WaitForSeconds(delay);

        currentModule++;

        isProcessing = false; // BUKA GERBANG
        StartTutorial(currentStage, currentModule);
    }
}