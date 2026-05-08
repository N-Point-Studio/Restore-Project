using UnityEngine;
using VContainer;

public class MenuController : BaseMenuController
{
    [Header("Menu")]
    [SerializeField] private ButtonItemUI buttonNewGame;
    [SerializeField] private ButtonItemUI buttonContinue;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonQuit;

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
        MainMenuEvents.TriggerOpenSettingsGame();
    }

    private void OnButtonQuitClick()
    {
        MainMenuEvents.TriggerQuitGame();
    }
}
