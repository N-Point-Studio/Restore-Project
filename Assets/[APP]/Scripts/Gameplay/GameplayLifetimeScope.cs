using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(Camera.main);
        builder.Register<PointerService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<DragService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<ClickService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<HoldService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<RotateService>(Lifetime.Scoped);
        builder.RegisterEntryPoint<ZoomService>(Lifetime.Scoped);
    }
}
