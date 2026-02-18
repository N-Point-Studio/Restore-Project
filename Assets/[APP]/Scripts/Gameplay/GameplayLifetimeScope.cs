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

        builder.Register<AssemblyService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<FragmentService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<InspectService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<PointService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<GestureService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<InteractionService>(Lifetime.Scoped).AsSelf();

        builder.RegisterComponentInHierarchy<ArtefactPieceStateMachine>().AsSelf();
    }
}