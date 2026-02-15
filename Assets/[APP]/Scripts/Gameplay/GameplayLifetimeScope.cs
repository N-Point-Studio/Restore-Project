using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private Transform inspect;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(Camera.main);
        builder.RegisterInstance(inspect);
        builder.Register<InteractionEventBus>(Lifetime.Scoped).AsImplementedInterfaces();

        builder.Register<PointerService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<InspectionService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<InteractionManager>(Lifetime.Scoped);
    }
}