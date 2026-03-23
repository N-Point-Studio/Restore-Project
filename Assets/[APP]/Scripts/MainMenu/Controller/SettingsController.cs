using UnityEngine;
using VContainer;

public class SettingsController : BaseMenuController
{
    private ActiveSettingsData activeSettingsData;
    
    [Inject]
    public void Construct(
        ActiveSettingsData activeSettingsData)
    {
        this.activeSettingsData = activeSettingsData;
    }
}
