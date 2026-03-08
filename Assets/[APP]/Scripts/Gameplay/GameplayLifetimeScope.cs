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
        builder.Register<SurfaceDetectionService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<ArtefactManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<InputSystemService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectDetectionService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectPressService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectDragService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectRotateService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectHoldService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectZoomService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectInteractionManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ToolManager>(Lifetime.Scoped).AsSelf();
    }
}