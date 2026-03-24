using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactDetailController : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroupRoot;
    [SerializeField] private ButtonInputInstructionUI buttonBack;
    
    [Header("Sub-Controllers")]
    [SerializeField] private JournalController journalController; 
    
    [Header("Text Wall (Post-Restoration)")]
    [SerializeField] private CanvasGroup wallTextGroup; 
    [SerializeField] private TMP_Text wallTitleText;
    [SerializeField] private TMP_Text wallDescText;
    [SerializeField] private ButtonInputInstructionUI buttonReplay;
    [SerializeField] private ButtonInputInstructionUI buttonContinueStory;

    [Header("Text Instructions")]
    [SerializeField] private TMP_Text instructionText; 

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player wallTextRevealFeedback;

    private ArtefactData currentArtefactData;
    private Artefact3DItem current3DItem; 
    private ActiveArtefactData activeArtefactData;
    private bool isCompleted;
    
    private void Awake()
    {
        buttonBack.OnClick += OnBackClicked;
        buttonReplay.OnClick += OnReplayClicked;
        buttonContinueStory.OnClick += OnContinueStoryClicked;
        root.SetActive(false);
        
        StickyNote3DItem.OnNotePeeled += HandleNotePeeled;
    }

    private void OnDestroy()
    {
        buttonBack.OnClick -= OnBackClicked;
        buttonReplay.OnClick -= OnReplayClicked;
        buttonContinueStory.OnClick -= OnContinueStoryClicked;
        StickyNote3DItem.OnNotePeeled -= HandleNotePeeled;
    }

    public void OpenDetail(ArtefactData artefactData, ActiveArtefactData activeData, Artefact3DItem item3D)
    {
        currentArtefactData = artefactData;
        activeArtefactData = activeData;
        current3DItem = item3D;
        isCompleted = activeArtefactData.IsArtefactCompleted(currentArtefactData.BaseData.Id);
        bool isStoryRead = activeArtefactData.IsStoryRead(currentArtefactData.BaseData.Id);

        bool hasNotesRemaining = !isCompleted && !isStoryRead;
        if (current3DItem != null) 
        {
            current3DItem.SetDetailMode(true, hasNotesRemaining); 
        }
        
        wallTextGroup.alpha = 0;
        wallTextGroup.gameObject.SetActive(false);

        root.SetActive(true);
        canvasGroupRoot.alpha = 1;

        journalController.SetBookHiddenInstant();

        if (isCompleted)
        {
            // POST-RESTORATION FLOW
            instructionText.gameObject.SetActive(false);
            
            journalController.SetupPostRestoration(currentArtefactData, OnJournalContinueClicked);
            journalController.OpenBookFull();
        }
        else
        {
            // FLOW PRE-RESTORATION
            if (isStoryRead)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Open the box and start restore...";
                
                journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked);
                journalController.HideBookToPeek();
            }
            else
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Peel off the sticky note...";
                
                journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked);
            }
        }
    }

    private void HandleNotePeeled(StickyNote3DItem peeledNote)
    {
        if (current3DItem == null) return;

        bool hasNotesLeft = current3DItem.CheckUnpeeledNotes();

        if (hasNotesLeft)
        {
            current3DItem.SetDetailMode(true, true);
        }
        else
        {
            current3DItem.SetDetailMode(true, false);
            activeArtefactData.MarkStoryRead(currentArtefactData.BaseData.Id);
            instructionText.gameObject.SetActive(false);
                        
            journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked);
            journalController.OpenBookFull();
        }
    }

    private void OnJournalRestoreClicked()
    {
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }

    private void OnJournalContinueClicked()
    {
        journalController.HideBookToPeek();
        
        wallTitleText.text = currentArtefactData.BaseData.ItemName;
        wallDescText.text = currentArtefactData.BaseData.ItemDescription;
        
        wallTextGroup.gameObject.SetActive(true);

        if (wallTextRevealFeedback != null)
        {
            wallTextRevealFeedback.PlayFeedbacks();
        }
        else
        {
            wallTextGroup.DOFade(1, 0.5f);
        }
    }

    private void OnBackClicked()
    {
        if (journalController.CurrentState == JournalState.Opened)
        {
            journalController.HideBookToPeek();

            if (isCompleted)
            {
                wallTitleText.text = currentArtefactData.BaseData.ItemName;
                wallDescText.text = currentArtefactData.BaseData.ItemDescription;
                wallTextGroup.gameObject.SetActive(true);
                wallTextGroup.DOFade(1, 0.5f);
            }
            else
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Open the box and start restore...";
            }
        }
        else
        {
            if (current3DItem != null) 
            {
                current3DItem.SetDetailMode(false, false);
                current3DItem.Initialize(activeArtefactData); 
            }
            CloseDetail();
            MainMenuEvents.TriggerCloseArtefactDetail();
            journalController.HideBookCompletely();
        }
    }

    private void OnReplayClicked()
    {
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }

    private void OnContinueStoryClicked()
    {
        if (current3DItem != null) 
        {
            current3DItem.SetDetailMode(false, false);            
            current3DItem.Initialize(activeArtefactData); 
        }
        CloseDetail();
        MainMenuEvents.TriggerCloseArtefactDetail();
        journalController.HideBookCompletely();
    }

    public void CloseDetail()
    {
        canvasGroupRoot.DOFade(0f, 0.3f).OnComplete(() => root.SetActive(false));
    }
}