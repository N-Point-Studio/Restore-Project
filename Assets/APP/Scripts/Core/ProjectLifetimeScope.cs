using VContainer;
using VContainer.Unity;
using UnityEngine;
using Modules.SoundSystems;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SoundSystem soundSystem;
    [SerializeField] protected GameObject loadingPrefab;
    [SerializeField] private ProjectSettingsData defaultSettingsData;
    [SerializeField] private ArtefactDatabase artefactDatabase;
    [SerializeField] private GameConfigData gameConfigData;

    protected override void Configure(IContainerBuilder builder)
    {
        // Core systems
        SoundSystem soundSystemInstance = Instantiate(soundSystem, transform);
        builder.RegisterComponent(soundSystemInstance).AsSelf();
        
        builder.RegisterInstance(gameConfigData);

        // Active Container (runtime state - all Singleton)
        builder.Register<PlayerProgressionData>(Lifetime.Singleton).AsSelf();
        builder.Register<ActiveArtefactData>(Lifetime.Singleton).AsSelf().WithParameter(artefactDatabase);
        builder.Register<ActiveSettingsData>(Lifetime.Singleton).WithParameter(defaultSettingsData);

        // Loading Service
        builder.RegisterEntryPoint<ProjectLoadingService>(Lifetime.Singleton).AsSelf().WithParameter(loadingPrefab);
        builder.Register<SceneLoader>(Lifetime.Singleton);

        // Settings Service
        builder.RegisterEntryPoint<ProjectSettingsService>(Lifetime.Singleton).AsSelf();

        // Audio Service
        builder.RegisterEntryPoint<ProjectAudioService>(Lifetime.Singleton).AsSelf();

        // Input System
        builder.RegisterEntryPoint<PlayerInputSystem>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<InputSystemService>(Lifetime.Singleton).AsSelf();

        // Saving System
        builder.RegisterEntryPoint<ProjectSavingSystem>(Lifetime.Singleton).AsSelf();

    }
}
