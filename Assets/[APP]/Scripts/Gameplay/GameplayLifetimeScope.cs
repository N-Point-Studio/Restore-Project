using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private Transform inspect;
    [SerializeField] private RectTransform inspectZone;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(Camera.main);
        builder.RegisterInstance(inspect);
        builder.RegisterInstance(inspectZone);

        builder.RegisterEntryPoint<AssemblyService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<FragmentService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<InspectService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<PointService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<InputService>(Lifetime.Scoped).AsSelf();
        // builder.RegisterEntryPoint<InteractionService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ClickService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<SwipeService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<HoldService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ZoomService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<GestureManager>(Lifetime.Scoped).AsSelf();

        // builder.RegisterComponentInHierarchy<ArtefactPieceStateMachine>().AsSelf();
    }
}