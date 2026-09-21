using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private Inspection inspect;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private PlaneReference planeReference;
    [SerializeField] private HoldProgressUI holdProgressUI;
    [SerializeField] private string targetScene;
    [SerializeField] private List<Light> mainLight;

    [Header("Tutorial Section")]
    [SerializeField] private TutorialOverlayUI tutorialOverlay;
    [SerializeField] private TutorialDragAnimator tutorialDragAnimator;
    [SerializeField] private TutorialZoomAnimator tutorialZoomAnimator;


    protected override void Configure(IContainerBuilder builder)
    {
        // Plane dragPlane = new(planeReference.up, planeReference.position);
        // builder.RegisterInstance(dragPlane);
        builder.RegisterInstance(planeReference);

        builder.Register<TutorialService>(Lifetime.Scoped);

        builder.RegisterInstance(Camera.main);
        builder.RegisterComponent(inspect);
        builder.RegisterInstance(pivotPoint);
        builder.RegisterInstance(holdProgressUI);
        builder.RegisterInstance(mainLight);

        builder.RegisterComponent(tutorialOverlay);
        builder.RegisterComponent(tutorialDragAnimator);
        builder.RegisterComponent(tutorialZoomAnimator);

        builder.RegisterEntryPoint<FragmentService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<AssemblyService>(Lifetime.Scoped).AsSelf();

        builder.Register<SurfaceDetectionService>(Lifetime.Scoped);

        builder.RegisterEntryPoint<CleaningService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ArtefactManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectDetectionService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectPressService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectDragService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectRotateService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectHoldService>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<ObjectZoomService>(Lifetime.Scoped).AsSelf();

        // builder.RegisterEntryPoint<MobileInputSystemService>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ObjectInteractionManager>(Lifetime.Scoped).AsSelf();

        builder.RegisterEntryPoint<ToolService>(Lifetime.Scoped).AsSelf();

        builder.RegisterComponentInHierarchy<GameplayUIManager>().AsSelf();
        builder.RegisterComponentInHierarchy<GameplayToolManager>().AsSelf();

        builder.RegisterEntryPoint<GameplayManager>().AsSelf().WithParameter(targetScene);
    }
}