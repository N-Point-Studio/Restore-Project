
using System;
using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{
    [Header("Sub-Controller")]
    [SerializeField] private MenuController menuController;
    [SerializeField] private LevelSelectionController levelSelectionController;

    [Inject]
    public void Construct(IObjectResolver container)
    {
        container.Inject(menuController);
        container.Inject(levelSelectionController);
        // TODO: nanti ini panggil nya pas user baru atau pas user manual reset (sementara di sini dulu buat test)
    }

    private void Awake()
    {
        MainMenuEvents.OnNewGame += OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame += OnRequestContinueGame;
    }

    private void OnDestroy()
    {
        MainMenuEvents.OnNewGame -= OnRequestNewGameGame;
        MainMenuEvents.OnContinueGame -= OnRequestContinueGame;        
    }

    private void OnRequestNewGameGame()
    {
        // Start FTUE Flow, reset save
    }

    private void OnRequestContinueGame()
    {
        levelSelectionController.OpenLevelSelection();
    }
}