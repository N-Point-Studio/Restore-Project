using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private readonly LoadingService _loadingService;

    public SceneLoader(LoadingService loadingService) => _loadingService = loadingService;

    /// <summary>
    /// <param name="minDuration">Minimum duration in seconds (e.g: 2.0f)</param>
    /// <param name="customMessage">Custom message to display (if empty, will use default message based on type)</param>
    /// Loads a scene asynchronously while showing a loading screen with progress and optional custom message. Ensures the loading screen is visible for at least minDuration seconds.
    /// </summary>
    public async Task LoadSceneAsync(string sceneName, LoadingType type, float minDuration = 0f, string customMessage = "")
    {
        string message = customMessage;
        if (string.IsNullOrEmpty(message))
        {
            message = type switch
            {
                LoadingType.Music => "Use headphone for the best experience...",
                LoadingType.Camera => "Photo is being printed...",
                LoadingType.Home => "Back to home...",
                _ => "Loading..."
            };
        }

        float startTime = Time.time;

        _loadingService.ShowLoading(message, type);
        _loadingService.SetProgress(0f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        
        while (!op.isDone)
        {
            _loadingService.SetProgress(Mathf.Clamp01(op.progress / 0.9f));
            await Task.Yield();
        }

        float elapsed = Time.time - startTime;
        if (elapsed < minDuration)
        {
            int delayTime = (int)((minDuration - elapsed) * 1000);
            await Task.Delay(delayTime);
        }

        _loadingService.HideLoading();
    }
}