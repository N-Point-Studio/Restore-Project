using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using DG.Tweening;
using Modules;

[System.Serializable]
public class SplashSettings
{
    public float fadeDuration = 1f;
    public int holdDurationMs = 1500;
    public float targetScale = 1.1f;
    public int endDelayMs = 250;
}

public class SplashService : IStartable
{
    private readonly SceneLoader sceneLoader;
    private readonly string targetScene;
    private readonly CanvasGroup canvasGroup;
    private readonly Sprite[] splashSprites;
    private readonly Image splashImage;
    private readonly SplashSettings settings;

    [Inject]
    public SplashService(
        SceneLoader sceneLoader, 
        string targetScene, 
        CanvasGroup canvasGroup, 
        Sprite[] splashSprites,
        Image splashImage,
        SplashSettings settings)
    {
        this.sceneLoader = sceneLoader;
        this.targetScene = targetScene;
        this.canvasGroup = canvasGroup;
        this.splashSprites = splashSprites;
        this.splashImage = splashImage;
        this.settings = settings;
    }

    void IStartable.Start()
    {
        _ = PlaySplashSequenceAsync();
    }

    private async Task PlaySplashSequenceAsync()
    {
        if (canvasGroup == null || splashImage == null || splashSprites == null || splashSprites.Length == 0)
        {
            AppLogger.LogWarning("[SplashService] UI references are incomplete. Skipping Splash animation.");
            await LoadNextScene();
            return;
        }

        canvasGroup.alpha = 0f;

        float totalAnimationDuration = (settings.fadeDuration * 2) + (settings.holdDurationMs / 1000f);

        for (int i = 0; i < splashSprites.Length; i++)
            {
                if (splashSprites[i] == null) continue;

                splashImage.sprite = splashSprites[i];
                splashImage.transform.localScale = Vector3.one;

                splashImage.transform.DOScale(1.1f, totalAnimationDuration).SetEase(Ease.Linear).SetLink(splashImage.gameObject);

                canvasGroup.DOFade(1f, settings.fadeDuration).SetLink(canvasGroup.gameObject);
                await Task.Delay((int)(settings.fadeDuration * 1000));

                await Task.Delay(settings.holdDurationMs);

                canvasGroup.DOFade(0f, settings.fadeDuration).SetLink(canvasGroup.gameObject);
                await Task.Delay((int)(settings.fadeDuration * 1000));

                splashImage.transform.DOKill();
                await Task.Delay(settings.endDelayMs); 
            }

            await LoadNextScene();
    }

    private async Task LoadNextScene()
    {
        string sceneToLoad = string.IsNullOrEmpty(targetScene) ? "MainMenu" : targetScene;        
        await sceneLoader.LoadSceneAsync(sceneToLoad);
    }
}