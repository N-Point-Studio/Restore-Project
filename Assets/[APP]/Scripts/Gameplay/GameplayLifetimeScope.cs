using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private Inspection inspect;
    [SerializeField] private RectTransform inspectZone;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private Transform planeReference;
    [SerializeField] private HoldProgressUI holdProgressUI;
    protected override void Configure(IContainerBuilder builder)
    {
        Plane dragPlane = new(planeReference.up, planeReference.position);
        builder.RegisterInstance(dragPlane);

        builder.Register<TutorialService>(Lifetime.Scoped);
        
        builder.RegisterInstance(Camera.main);
        builder.RegisterComponent(inspect);
        builder.RegisterInstance(inspectZone);
        builder.RegisterInstance(pivotPoint);
        builder.RegisterInstance(holdProgressUI);

        builder.RegisterEntryPoint<FragmentService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<AssemblyService>(Lifetime.Scoped).AsSelf();

        builder.Register<InspectService>(Lifetime.Scoped);

        builder.Register<SurfaceDetectionService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<CleaningService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ArtefactManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectDetectionService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectPressService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectDragService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectRotateService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectHoldService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectZoomService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectInteractionManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ToolService>(Lifetime.Scoped).AsSelf();

        builder.RegisterComponentInHierarchy<GameplayUIManager>().AsSelf();

        builder.RegisterEntryPoint<GameplayManager>().AsSelf();
    }
}