using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using DG.Tweening;
using Modules;

public class SplashService : IStartable
{
    private readonly SceneLoader sceneLoader;
    private readonly string targetScene;
    private readonly CanvasGroup canvasGroup;
    private readonly Sprite[] splashSprites;
    
    private readonly Image splashImage;

    [Inject]
    public SplashService(
        SceneLoader sceneLoader, 
        string targetScene, 
        CanvasGroup canvasGroup, 
        Sprite[] splashSprites,
        Image splashImage)
    {
        this.sceneLoader = sceneLoader;
        this.targetScene = targetScene;
        this.canvasGroup = canvasGroup;
        this.splashSprites = splashSprites;
        this.splashImage = splashImage;
    }

    void IStartable.Start()
    {
        _ = PlaySplashSequenceAsync();
    }

    private async Task PlaySplashSequenceAsync()
    {
        // Fallback: If any references are empty, directly load main menu to prevent the game from getting stuck
        if (canvasGroup == null || splashImage == null || splashSprites == null || splashSprites.Length == 0)
        {
            AppLogger.LogWarning("[SplashService] UI references are incomplete. Skipping Splash animation.");
            await LoadNextScene();
            return;
        }

        // Initial setup: make the screen transparent
        canvasGroup.alpha = 0f;

        // Time settings
        float fadeDuration = 1f;
        int holdDurationMs = 1500; // 1.5 seconds
        
        // Total time one logo is displayed on screen (Fade In + Hold + Fade Out)
        float totalAnimationDuration = (fadeDuration * 2) + (holdDurationMs / 1000f);

        // Loop to display all splash sprites alternately
        for (int i = 0; i < splashSprites.Length; i++)
        {
            if (splashSprites[i] == null) continue;

            // 1. Change logo image and reset size (Scale) to original size (1x)
            splashImage.sprite = splashSprites[i];
            splashImage.transform.localScale = Vector3.one;

            // Start scaling effect (slow zoom-in) to 1.1x size while logo is displayed
            // Using Ease.Linear for constant/smooth movement
            splashImage.transform.DOScale(1.1f, totalAnimationDuration).SetEase(Ease.Linear);

            // 2. Fade In
            canvasGroup.DOFade(1f, fadeDuration);
            await Task.Delay((int)(fadeDuration * 1000));

            // 3. Hold (Hold logo on screen)
            await Task.Delay(holdDurationMs);

            // 4. Fade Out
            canvasGroup.DOFade(0f, fadeDuration);
            await Task.Delay((int)(fadeDuration * 1000));

            // Clean up scaling tween animation attached to splashImage (just in case)
            splashImage.transform.DOKill();

            // Small black screen pause before next logo appears
            await Task.Delay(250); 
        }

        // 5. Move to the next Scene after the entire splash is finished
        await LoadNextScene();
    }

    private async Task LoadNextScene()
    {
        string sceneToLoad = string.IsNullOrEmpty(targetScene) ? "MainMenu" : targetScene;        
        await sceneLoader.LoadSceneAsync(sceneToLoad, LoadingType.Music);
    }
}