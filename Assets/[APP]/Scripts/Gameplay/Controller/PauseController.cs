using System;
using UnityEngine;

public class PauseController : BaseMenuController
{
    [SerializeField] private ButtonItemUI buttonResume;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonBackToMenu;

    public event Action OnResume;
    public event Action OnOpenSettings;
    public event Action OnBackToMenu;

    protected override void Awake()
    {
        base.Awake();
        buttonResume.OnClick += OnResumeClick;
        buttonSettings.OnClick += OnSettingsClick;
        buttonBackToMenu.OnClick += OnBackToMenuClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonResume.OnClick -= OnResumeClick;
        buttonSettings.OnClick -= OnSettingsClick;
        buttonBackToMenu.OnClick -= OnBackToMenuClick;
    }

    private void OnResumeClick()
    {
        OnResume?.Invoke();
    }

    private void OnSettingsClick()
    {
        OnOpenSettings?.Invoke();
    }

    private void OnBackToMenuClick()
    {
        OnBackToMenu?.Invoke();
    }
}
