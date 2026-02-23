using System;
using UnityEngine;
using VContainer.Unity;

/// <summary>
/// Thin facade over a LoadingView prefab providing progress & text updates.
/// </summary>
public class LoadingService : IInitializable, IDisposable
{
    private LoadingView loadingView;

    /// <summary>
    /// Instantiates a loading view prefab.
    /// </summary>
    public LoadingService(GameObject viewSample)
    {
        loadingView = UnityEngine.Object.Instantiate(viewSample).GetComponent<LoadingView>();
    }

    void IInitializable.Initialize()
    {

    }

    void IDisposable.Dispose()
    {

    }

    /// <summary>
    /// Displays the loading view with optional text.
    /// </summary>
    public void ShowLoading(string text, LoadingType type) 
    {
        loadingView.ShowLoading(text, type);
    }

    /// <summary>
    /// Hides the loading view.
    /// </summary>
    public void HideLoading()
    {
        loadingView.HideLoading();
    }

    /// <summary>
    /// Updates the progress bar (expects 0..1 range as provided by async operation progress).
    /// </summary>
    public void SetProgress(float progress)
    {
        loadingView.SetProgress(progress);
    }
}