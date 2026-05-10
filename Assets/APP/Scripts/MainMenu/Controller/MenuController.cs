using UnityEngine;
using VContainer;
using UnityEngine.Localization;

public class MenuController : BaseMenuController
{
    [Header("Menu")]
    [SerializeField] private ButtonItemUI buttonNewGame;
    [SerializeField] private ButtonItemUI buttonContinue;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonQuit;

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
        buttonNewGame.OnClick += OnButtonNewGameClick;
        buttonContinue.OnClick += OnButtonContinueClick;
        buttonSettings.OnClick += OnButtonSettingsClick;
        buttonQuit.OnClick += OnButtonQuitClick;
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
    }

    public void RefreshButtonVisibility()
    {
        if (playerProgressionData != null)
        {
            bool hasPlayed = playerProgressionData.HasPlayedBefore;
            buttonContinue.gameObject.SetActive(hasPlayed);
            buttonNewGame.SetLocalizedLabel(hasPlayed ? newGameLabel : startLabel);
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
}
