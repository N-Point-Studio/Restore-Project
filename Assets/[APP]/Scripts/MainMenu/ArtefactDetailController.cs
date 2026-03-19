using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactDetailController : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroupRoot;
    [SerializeField] private Button buttonBack;
    
    [Header("Sub-Controllers")]
    [SerializeField] private JournalController journalController; 
    
    [Header("Text Wall (Post-Restoration)")]
    [SerializeField] private CanvasGroup wallTextGroup; 
    [SerializeField] private TMP_Text wallTitleText;
    [SerializeField] private TMP_Text wallDescText;
    [SerializeField] private Button buttonReplay;
    [SerializeField] private Button buttonContinueStory;

    [Header("Text Instructions")]
    [SerializeField] private TMP_Text instructionText; 

    private ArtefactData currentArtefactData;
    private Artefact3DItem current3DItem; 
    private ActiveArtefactData activeArtefactData;
    private bool isCompleted;
    
    private bool isBookOpen = false; 

    private void Awake()
    {
        buttonBack.onClick.AddListener(OnBackClicked);
        buttonReplay.onClick.AddListener(OnReplayClicked);
        buttonContinueStory.onClick.AddListener(OnContinueStoryClicked);
        root.SetActive(false);
        
        StickyNote3DItem.OnNotePeeled += HandleNotePeeled;
    }

    private void OnDestroy()
    {
        buttonBack.onClick.RemoveListener(OnBackClicked);
        buttonReplay.onClick.RemoveListener(OnReplayClicked);
        buttonContinueStory.onClick.RemoveListener(OnContinueStoryClicked);
        StickyNote3DItem.OnNotePeeled -= HandleNotePeeled;
    }

    public void OpenDetail(ArtefactData artefactData, ActiveArtefactData activeData, Artefact3DItem item3D)
    {
        currentArtefactData = artefactData;
        activeArtefactData = activeData;
        current3DItem = item3D;
        isCompleted = activeArtefactData.IsArtefactCompleted(currentArtefactData.BaseData.Id);
        
        wallTextGroup.alpha = 0;
        wallTextGroup.gameObject.SetActive(false);

        root.SetActive(true);
        canvasGroupRoot.alpha = 1;

        if (isCompleted)
        {
            // POST-RESTORATION FLOW
            instructionText.gameObject.SetActive(false);
            isBookOpen = true; // Book opens directly in the center of the screen
            journalController.ShowPostRestoration(currentArtefactData, OnJournalContinueClicked);
        }
        else
        {
            // FLOW PRE-RESTORATION
            bool isStoryRead = activeArtefactData.IsStoryRead(currentArtefactData.BaseData.Id);
            
            if (isStoryRead)
            {
                // Story has been read before, book just peeks from below
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Open the box and start restore...";
                isBookOpen = false; 
                journalController.HideBookToPeek();
            }
            else
            {
                // First time clicking, book is still hidden
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Peel off the sticky note...";
                isBookOpen = false;
                journalController.HideBookCompletely();
            }
        }
    }

    private void HandleNotePeeled(StickyNote3DItem peeledNote)
    {
        if (current3DItem != null) current3DItem.SetDetailMode(true, notesRemaining: false);
        activeArtefactData.MarkStoryRead(currentArtefactData.BaseData.Id);
        instructionText.gameObject.SetActive(false);
        
        isBookOpen = true; // Because peeling note triggers book to open to center
        journalController.ShowPreRestoration(currentArtefactData, OnJournalRestoreClicked);
    }

    private void OnJournalRestoreClicked()
    {
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }

    private void OnJournalContinueClicked()
    {
        isBookOpen = false; // Because book goes down (peeking)
        journalController.HideBookToPeek();
        
        wallTitleText.text = currentArtefactData.BaseData.ItemName;
        wallDescText.text = currentArtefactData.BaseData.ItemDescription;
        
        wallTextGroup.gameObject.SetActive(true);
        wallTextGroup.DOFade(1, 0.5f);
    }

    // --- NEW BACK BUTTON LOGIC ---
    private void OnBackClicked()
    {
        if (isBookOpen)
        {
            // IF BOOK IS OPEN (FORWARD): Back button will collapse book downwards
            isBookOpen = false;
            journalController.HideBookToPeek();

            if (isCompleted)
            {
                // If restoration is done, show text on the wall
                wallTitleText.text = currentArtefactData.BaseData.ItemName;
                wallDescText.text = currentArtefactData.BaseData.ItemDescription;
                wallTextGroup.gameObject.SetActive(true);
                wallTextGroup.DOFade(1, 0.5f);
            }
            else
            {
                // If not done, show instruction text to click box
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Open the box and start restore...";
            }
        }
        else
        {
            // IF BOOK IS PEEKING/HIDDEN: Back button really exits Detail Mode
            if (current3DItem != null) current3DItem.SetDetailMode(false, false);
            CloseDetail();
            MainMenuEvents.TriggerCloseArtefactDetail();
        }
    }

    private void OnReplayClicked()
    {
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }

    private void OnContinueStoryClicked()
    {
        // Exit from detail mode
        if (current3DItem != null) current3DItem.SetDetailMode(false, false);
        CloseDetail();
        MainMenuEvents.TriggerCloseArtefactDetail();
    }

    public void CloseDetail()
    {
        canvasGroupRoot.DOFade(0f, 0.3f).OnComplete(() => root.SetActive(false));
    }
}