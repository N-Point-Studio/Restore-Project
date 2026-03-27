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

                splashImage.sprite = splashSprites[i];
                splashImage.transform.localScale = Vector3.one;

                // ---> TAMBAH .SetLink(splashImage.gameObject) <---
                splashImage.transform.DOScale(1.1f, totalAnimationDuration).SetEase(Ease.Linear).SetLink(splashImage.gameObject);

                // ---> TAMBAH .SetLink(canvasGroup.gameObject) <---
                canvasGroup.DOFade(1f, fadeDuration).SetLink(canvasGroup.gameObject);
                await Task.Delay((int)(fadeDuration * 1000));

                await Task.Delay(holdDurationMs);

                // ---> TAMBAH .SetLink(canvasGroup.gameObject) <---
                canvasGroup.DOFade(0f, fadeDuration).SetLink(canvasGroup.gameObject);
                await Task.Delay((int)(fadeDuration * 1000));

                splashImage.transform.DOKill();
                await Task.Delay(250); 
            }

            await LoadNextScene();
    }

    private async Task LoadNextScene()
    {
        string sceneToLoad = string.IsNullOrEmpty(targetScene) ? "MainMenu" : targetScene;        
        await sceneLoader.LoadSceneAsync(sceneToLoad);
    }
}