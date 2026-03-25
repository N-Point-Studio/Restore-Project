using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using VContainer;
using Modules;

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

    [Header("Settings")]
    [SerializeField] private string wishlistLink;
    [SerializeField] private string discordLink;
    [SerializeField] private string instagramLink;

    private Tween quitTween;
    private ProjectSavingSystem projectSavingSystem;

    [Inject]
    public void Construct(ProjectSavingSystem projectSavingSystem)
    {
        this.projectSavingSystem = projectSavingSystem;
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
        if (!string.IsNullOrEmpty(wishlistLink)) Application.OpenURL(wishlistLink);
    }

    private void OnButtonDiscordClick()
    {
        if (!string.IsNullOrEmpty(discordLink)) Application.OpenURL(discordLink);
    }

    private void OnButtonInstagramClick()
    {
        if (!string.IsNullOrEmpty(instagramLink)) Application.OpenURL(instagramLink);
    }

    private void OnButtonCancelClick()
    {
        SetActive(false);
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