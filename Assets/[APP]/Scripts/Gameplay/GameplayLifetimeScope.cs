using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private Transform inspect;
    [SerializeField] private RectTransform inspectZone;
    [SerializeField] private Transform planeReference;
    protected override void Configure(IContainerBuilder builder)
    {
        Plane dragPlane = new(planeReference.up, planeReference.position);
        builder.RegisterInstance(dragPlane);

        builder.RegisterInstance(Camera.main);
        builder.RegisterInstance(inspect);
        builder.RegisterInstance(inspectZone);

        builder.RegisterEntryPoint<FragmentService>(Lifetime.Scoped).AsSelf();

        builder.Register<AssemblyService>(Lifetime.Scoped).AsSelf();
        builder.Register<InspectService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<ArtefactManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<InputService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<PointService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<InputSystemService>(Lifetime.Scoped).AsSelf();

        // builder.RegisterEntryPoint<PressService>(Lifetime.Scoped).AsSelf();

        // builder.RegisterEntryPoint<ClickService>(Lifetime.Scoped).AsSelf();
        // builder.RegisterEntryPoint<SwipeService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<RotateService>(Lifetime.Scoped).AsSelf();
        // builder.RegisterEntryPoint<HoldService>(Lifetime.Scoped).AsSelf();
        // builder.RegisterEntryPoint<ZoomService>(Lifetime.Scoped).AsSelf();

        // builder.RegisterEntryPoint<GestureManager>(Lifetime.Scoped).AsSelf();
    }
}