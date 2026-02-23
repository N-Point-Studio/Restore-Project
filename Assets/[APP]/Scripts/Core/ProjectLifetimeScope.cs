using VContainer;
using VContainer.Unity;
using UnityEngine;
using Modules.SoundSystems;

public class ProjectLifetimeScope : LifetimeScope
{
    // Core systems
    [SerializeField] private SoundSystem soundSystem;
    [SerializeField] protected HapticManager hapticManager;
    [SerializeField] protected GameObject loadingPrefab;

    // Databases
    [SerializeField] private ArtefactDatabase artefactDatabase;

    protected override void Configure(IContainerBuilder builder)
    {
        // Core systems
        Instantiate(soundSystem, transform);
        builder.RegisterComponentInHierarchy<SoundSystem>().AsSelf();

        // Haptic manager
#if UNITY_IOS
            HapticManager hapticInstance = Instantiate(hapticManager, transform);
            builder.RegisterComponentInHierarchy<HapticManager>().AsSelf();
            hapticInstance.SetActiveHaptic(true);
#endif

        // Active Container (runtime state - all Singleton)
        builder.Register<ActiveArtefactData>(Lifetime.Singleton).AsSelf().WithParameter(artefactDatabase);

        // Saving System
        builder.RegisterEntryPoint<ProjectSavingSystem>(Lifetime.Singleton).AsSelf();

        // Loading Service
        builder.RegisterEntryPoint<LoadingService>(Lifetime.Singleton).AsSelf().WithParameter(loadingPrefab);
        builder.Register<SceneLoader>(Lifetime.Singleton);

        // Input System
        builder.RegisterEntryPoint<PlayerInputSystem>(Lifetime.Singleton).AsSelf();
    }
}
