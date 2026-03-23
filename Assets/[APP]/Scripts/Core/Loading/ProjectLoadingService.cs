using System;
using UnityEngine;
using VContainer.Unity;

public class ProjectLoadingService : IInitializable, IDisposable
{
    private ProjectLoadingView loadingView;

    public ProjectLoadingService(GameObject viewSample)
    {
        loadingView = UnityEngine.Object.Instantiate(viewSample).GetComponent<ProjectLoadingView>();
    }

    void IInitializable.Initialize()
    {

    }

    void IDisposable.Dispose()
    {

    }

    public void ShowLoading(string text) 
    {
        loadingView.ShowLoading(text);
    }

    public void HideLoading()
    {
        loadingView.HideLoading();
    }
}