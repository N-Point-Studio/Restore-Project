using UnityEngine;
using VContainer;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuController : BaseMenuController
{
    [Header("Menu")]
    [SerializeField] private ButtonItemUI buttonNewGame;
    [SerializeField] private ButtonItemUI buttonContinue;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonQuit;
    [Header("Others")]
    [SerializeField] private ButtonItemUI buttonWishlist;
    [SerializeField] private ButtonItemUI buttonTwitter;
    [SerializeField] private ButtonItemUI buttonInstagram;
    [SerializeField] private ButtonItemUI buttonWebsite;

    private PlayerProgressionData playerProgressionData;

    private Tween quitTween;

    [Inject]
    public void Construct(PlayerProgressionData playerProgressionData)
    {
        this.playerProgressionData = playerProgressionData;
    }

    protected override void Awake()
    {
        base.Awake();
        buttonNewGame.OnClick += OnButtonNewGameClick;
        buttonContinue.OnClick += OnButtonContinueClick;
        buttonSettings.OnClick += OnButtonSettingsClick;
        buttonQuit.OnClick += OnButtonQuitClick;
        buttonWishlist.OnClick += OnButtonWishlistClick;
        buttonTwitter.OnClick += OnButtonTwitterClick;
        buttonInstagram.OnClick += OnButtonInstagramClick;
        buttonWebsite.OnClick += OnButtonWebsiteClick;
    }

    protected override void Start()
    {
        base.Start();
        RefreshButtonVisibility();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonNewGame.OnClick -= OnButtonNewGameClick;
        buttonContinue.OnClick -= OnButtonContinueClick;
        buttonSettings.OnClick -= OnButtonSettingsClick;
        buttonQuit.OnClick -= OnButtonQuitClick;
        buttonWishlist.OnClick -= OnButtonWishlistClick;
        buttonTwitter.OnClick -= OnButtonTwitterClick;
        buttonInstagram.OnClick -= OnButtonInstagramClick;
        buttonWebsite.OnClick -= OnButtonWebsiteClick;

        if (quitTween != null && quitTween.IsActive())
        {
            quitTween.Kill();
        }
    }

    public void RefreshButtonVisibility()
    {
        if (playerProgressionData != null)
        {
            bool hasPlayed = playerProgressionData.HasPlayedBefore;
            buttonContinue.gameObject.SetActive(hasPlayed);
            buttonNewGame.SetLabel(hasPlayed ? "New Game" : "Start");
        }
    }

    private void OnButtonNewGameClick()
    {
        MainMenuEvents.TriggerNewGame();
    }

    private void OnButtonContinueClick()
    {
        MainMenuEvents.TriggerContinueGame();
    }

    private void OnButtonSettingsClick()
    {
        // TODO: Ignore for now
    }

    private void OnButtonQuitClick()
    {
        if (quitTween != null && quitTween.IsActive()) return;

        quitTween = DOVirtual.DelayedCall(0.05f, () =>
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }).SetUpdate(true);
    }

    private void OnButtonWishlistClick()
    {
        // TODO: Ignore for now
    }

    private void OnButtonTwitterClick()
    {
        // TODO: Ignore for now
    }

    private void OnButtonInstagramClick()
    {
        // TODO: Ignore for now
    }

    private void OnButtonWebsiteClick()
    {
        // TODO: Ignore for now
    }
}
