using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using VContainer;
using Modules;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitController : BaseMenuController
{
    [Header("UI References")]
    [SerializeField] private Button buttonWishlist;
    [SerializeField] private Button buttonDiscord;
    [SerializeField] private Button buttonInstagram;
    [SerializeField] private ButtonItemUI buttonCancel;
    [SerializeField] private ButtonItemUI buttonConfirm;

    private Tween quitTween;
    private ProjectSavingSystem projectSavingSystem;
    private GameConfigData configData;
    public event Action OnQuitCancelled;

    [Inject]
    public void Construct(ProjectSavingSystem projectSavingSystem, GameConfigData configData)
    {
        this.projectSavingSystem = projectSavingSystem;
        this.configData = configData;
    }

    protected override void Awake()
    {
        base.Awake();
        buttonCancel.OnClick += OnButtonCancelClick;
        buttonConfirm.OnClick += OnButtonConfirmClick;
        buttonWishlist.onClick.AddListener(OnButtonWishlistClick);
        buttonDiscord.onClick.AddListener(OnButtonDiscordClick);
        buttonInstagram.onClick.AddListener(OnButtonInstagramClick);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonCancel.OnClick -= OnButtonCancelClick;
        buttonConfirm.OnClick -= OnButtonConfirmClick;
        buttonWishlist.onClick.RemoveListener(OnButtonWishlistClick);
        buttonDiscord.onClick.RemoveListener(OnButtonDiscordClick);
        buttonInstagram.onClick.RemoveListener(OnButtonInstagramClick);

        if (quitTween != null && quitTween.IsActive())
        {
            quitTween.Kill();
        }
    }

    private void OnButtonWishlistClick()
    {
        if (configData == null)
            return;

        if (!string.IsNullOrEmpty(configData.steamURL)) Application.OpenURL(configData.steamURL);
    }

    private void OnButtonDiscordClick()
    {
        if (configData == null)
            return;

        if (!string.IsNullOrEmpty(configData.discordURL)) Application.OpenURL(configData.discordURL);
    }

    private void OnButtonInstagramClick()
    {
        if (configData == null)
            return;

        if (!string.IsNullOrEmpty(configData.instagramURL)) Application.OpenURL(configData.instagramURL);
    }

    private void OnButtonCancelClick()
    {
        SetActive(false);
        OnQuitCancelled?.Invoke();
    }

    private void OnButtonConfirmClick()
    {
        if (quitTween != null && quitTween.IsActive()) return;

        canvasGroup.interactable = false;

        if (projectSavingSystem != null)
        {
            AppLogger.Log("[QuitController] Saving progress before exiting...");
            projectSavingSystem.SaveAll(0, ExecuteQuitApp);
        }
        else
        {
            ExecuteQuitApp();
        }
    }

    private void ExecuteQuitApp()
    {
        quitTween = DOVirtual.DelayedCall(0.1f, () =>
        {
            AppLogger.Log("[QuitController] Exiting the Game!");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }).SetUpdate(true);
    }
}