using System;
using VContainer;
using VContainer.Unity;

public class ObjectZoomService : IInitializable, IDisposable
{
    private readonly InputSystemService inputSystemService;
    private readonly GameConfigData config;
    private readonly TutorialService tutorialService;

    [Inject]
    public ObjectZoomService(InputSystemService inputSystemService, GameConfigData config, TutorialService tutorialService)
    {
        this.inputSystemService = inputSystemService;
        this.config = config;
        this.tutorialService = tutorialService;
    }

    public void Initialize()
    {
        inputSystemService.OnScrollPerformed += HandleScrollPerformed;
    }


    public void Dispose()
    {
        inputSystemService.OnScrollPerformed -= HandleScrollPerformed;
    }

    private void HandleScrollPerformed(float value)
    {
        if (tutorialService.IsInputBlocked) return;
        float zoomDelta = value * config.scrollSensitivity;
        InteractionEvents.OnZoomPerformed(zoomDelta);
    }
}