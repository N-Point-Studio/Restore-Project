using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private readonly ProjectLoadingService _loadingService;

    public SceneLoader(ProjectLoadingService loadingService) => _loadingService = loadingService;

    /// <summary>
    /// <param name="minDuration">Minimum duration in seconds (e.g: 2.0f)</param>
    /// <param name="customMessage">Custom message to display (if empty, will use default message based on type)</param>
    /// Loads a scene asynchronously while showing a loading screen with progress and optional custom message. Ensures the loading screen is visible for at least minDuration seconds.
    /// </summary>
    public async Task LoadSceneAsync(string sceneName, float minDuration = 0f, string customMessage = "")
    {
        string message = customMessage;

        float startTime = Time.time;

        _loadingService.ShowLoading(message);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        
        while (!op.isDone)
        {
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