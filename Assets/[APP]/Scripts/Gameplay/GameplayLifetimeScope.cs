using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] protected PlayerTouch playerTouch;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(playerTouch);
    }
}
