using System;
using UnityEngine;

public class MenuController : BaseMenuController
{
    [Header("Menu")]
    [SerializeField] private ButtonItemUI buttonNewGame;
    [SerializeField] private ButtonItemUI buttonContinue;
    [SerializeField] private ButtonItemUI buttonSettings;
    [SerializeField] private ButtonItemUI buttonQuit;
    [SerializeField] private ButtonItemUI buttonWishlist;
    [SerializeField] private ButtonItemUI buttonTwitter;
    [SerializeField] private ButtonItemUI buttonInstagram;
    [SerializeField] private ButtonItemUI buttonWebsite;

    protected override void Awake()
    {
        buttonNewGame.OnClick += OnButtonNewGameClick;
        buttonContinue.OnClick += OnButtonContinueClick;
        buttonSettings.OnClick += OnButtonSettingsClick;
        buttonQuit.OnClick += OnButtonQuitClick;
        buttonWishlist.OnClick += OnButtonWishlistClick;
        buttonTwitter.OnClick += OnButtonTwitterClick;
        buttonInstagram.OnClick += OnButtonInstagramClick;
        buttonWebsite.OnClick += OnButtonWebsiteClick;
    }

    protected override void OnDestroy()
    {
        buttonNewGame.OnClick += OnButtonNewGameClick;
        buttonContinue.OnClick += OnButtonContinueClick;
        buttonSettings.OnClick += OnButtonSettingsClick;
        buttonQuit.OnClick += OnButtonQuitClick;
        buttonWishlist.OnClick += OnButtonWishlistClick;
        buttonTwitter.OnClick += OnButtonTwitterClick;
        buttonInstagram.OnClick += OnButtonInstagramClick;
        buttonWebsite.OnClick += OnButtonWebsiteClick;
    }

    private void OnButtonNewGameClick()
    {
        // TODO: Create new ActivePlayerData
        // Open 
        MainMenuEvents.TriggerNewGame();
    }

    private void OnButtonContinueClick()
    {
        // TODO: Load Save system
        MainMenuEvents.TriggerContinueGame();
    }

    private void OnButtonSettingsClick()
    {
        // TODO: Ignore for now
    }

    private void OnButtonQuitClick()
    {
        // TODO: Change to DOTTween and properly cleanup the tween
//         LeanTween.delayedCall(0.05f, () =>
//         {
// #if UNITY_EDITOR
//             EditorApplication.ExitPlaymode();
// #else
//             Application.Quit();
// #endif
//         });
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
