using VContainer;
using VContainer.Unity;
using UnityEngine;

public class MainMenuLifetimeScope : LifetimeScope
{
    [SerializeField] protected MainMenuManager mainMenuManager;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(mainMenuManager);
    }
}
