using UnityEngine;
using VContainer;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MenuController : BaseMenuController
{
    [Header("Menu")]
    [SerializeField] private ButtonItemUI buttonNewGame;
    [SerializeField] private ButtonItemUI buttonContinue;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonCredits;
    [SerializeField] private ButtonItemUI buttonQuit;

    [Header("Layout Settings")]
    [SerializeField] private float mobileSpacing = 36f;
    [SerializeField] private float desktopSpacing = 12f;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
 
    [Header("Localization")]
    [SerializeField] private LocalizedString newGameLabel;
    [SerializeField] private LocalizedString startLabel;

    private PlayerProgressionData playerProgressionData;

    [Inject]
    public void Construct(PlayerProgressionData playerProgressionData)
    {
        this.playerProgressionData = playerProgressionData;
    }

    protected override void Awake()
    {
        base.Awake();
        if (buttonNewGame != null) buttonNewGame.OnClick += OnButtonNewGameClick;
        if (buttonContinue != null) buttonContinue.OnClick += OnButtonContinueClick;
        if (buttonSettings != null) buttonSettings.OnClick += OnButtonSettingsClick;
        if (buttonQuit != null) buttonQuit.OnClick += OnButtonQuitClick;
        if (buttonCredits != null) buttonCredits.OnClick += OnButtonCreditsClick;

        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif
        verticalLayoutGroup.spacing = isMobile ? mobileSpacing : desktopSpacing;
    }

    protected override void Start()
    {
        base.Start();
        RefreshButtonVisibility();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (buttonNewGame != null) buttonNewGame.OnClick -= OnButtonNewGameClick;
        if (buttonContinue != null) buttonContinue.OnClick -= OnButtonContinueClick;
        if (buttonSettings != null) buttonSettings.OnClick -= OnButtonSettingsClick;
        if (buttonQuit != null) buttonQuit.OnClick -= OnButtonQuitClick;
        if (buttonCredits != null) buttonCredits.OnClick -= OnButtonCreditsClick;
    }

    public void RefreshButtonVisibility()
    {
        if (playerProgressionData != null)
        {
            bool hasPlayed = playerProgressionData.HasPlayedBefore;
            if (buttonContinue != null) buttonContinue.gameObject.SetActive(hasPlayed);
            if (buttonNewGame != null) buttonNewGame.SetLocalizedLabel(hasPlayed ? newGameLabel : startLabel);
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
        MainMenuEvents.TriggerOpenSettingsGame();
    }

    private void OnButtonQuitClick()
    {
        MainMenuEvents.TriggerQuitGame();
    }

    private void OnButtonCreditsClick()
    {
        MainMenuEvents.TriggerOpenCredits();
    }
}
