using VContainer;
using VContainer.Unity;
using UnityEngine;
using Modules.SoundSystems;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SoundSystem soundSystem;
    [SerializeField] protected HapticManager hapticManager;
    [SerializeField] protected GameObject loadingPrefab;

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
    }
}
