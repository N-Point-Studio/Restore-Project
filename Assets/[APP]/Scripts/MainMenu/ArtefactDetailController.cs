using System;
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
    [SerializeField] private Button buttonReplay;
    [SerializeField] private Button buttonContinueStory;

    [Header("Text Instructions")]
    [SerializeField] private TMP_Text instructionText; 

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player wallTextRevealFeedback;

    private ArtefactData currentArtefactData;
    private Artefact3DItem current3DItem; 
    private ActiveArtefactData activeArtefactData;
    private bool isCompleted;

    private InputSystemService input;

    public bool IsOpen => root != null && root.activeSelf;
    
    private void Awake()
    {
        buttonBack.OnClick += OnBackClicked;
        buttonReplay.onClick.AddListener(OnReplayClicked);
        buttonContinueStory.onClick.AddListener(OnContinueStoryClicked);
        root.SetActive(false);
        
        StickyNote3DItem.OnNotePeeled += HandleNotePeeled;

        if (journalController != null)
        {
            journalController.OnBookOpened += HandleJournalOpenedFromClick;
        }
    }

    private void OnDestroy()
    {
        buttonBack.OnClick -= OnBackClicked;
        buttonReplay.onClick.RemoveListener(OnReplayClicked);
        buttonContinueStory.onClick.RemoveListener(OnContinueStoryClicked);
        StickyNote3DItem.OnNotePeeled -= HandleNotePeeled;

        if (journalController != null)
        {
            journalController.OnBookOpened -= HandleJournalOpenedFromClick;
        }

        if (this.input != null)
        {
            this.input.OnUIKeycodeEscapePerformed -= OnUIKeycodeEscapePerformed;
            this.input.OnUIKeycodeRPerformed -= OnUIKeycodeRPerformed;
            this.input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
        }
    }

    public void SetInputSystemService(InputSystemService input)
    {
        if (this.input != null)
        {
            this.input.OnUIKeycodeEscapePerformed -= OnUIKeycodeEscapePerformed;
            this.input.OnUIKeycodeRPerformed -= OnUIKeycodeRPerformed;
            this.input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
        }

        this.input = input;
        journalController.SetInputSystemService(this.input);

        if (this.input != null)
        {
            this.input.OnUIKeycodeEscapePerformed += OnUIKeycodeEscapePerformed;
            this.input.OnUIKeycodeRPerformed += OnUIKeycodeRPerformed;
            this.input.OnUIKeycodeEnterPerformed += OnUIKeycodeEnterPerformed;
        }
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
            instructionText.gameObject.SetActive(false);
            
            buttonBack.Button.interactable = false; 
            DOVirtual.DelayedCall(1.5f, () => { if (buttonBack != null) buttonBack.Button.interactable = true; });

            buttonBack.SetAlternateStyle(true);

            journalController.SetupPostRestoration(currentArtefactData, OnJournalContinueClicked);
            journalController.OpenBookFull();
        }
        else
        {
            if (isStoryRead)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Open the box and start restore...";
                
                buttonBack.SetAlternateStyle(false);

                journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked);
                journalController.HideBookToPeek();
            }
            else
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = "Peel off the sticky note...";
                
                buttonBack.SetAlternateStyle(false);

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
                        
            buttonBack.Button.interactable = false; 
            DOVirtual.DelayedCall(1.5f, () => { if (buttonBack != null) buttonBack.Button.interactable = true; });

            buttonBack.SetAlternateStyle(true);

            journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked);
            journalController.OpenBookFull();
        }
    } 

    private void HandleJournalOpenedFromClick()
    {
        if (buttonBack != null)
        {
            buttonBack.SetAlternateStyle(true);
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
            
            buttonBack.SetAlternateStyle(false);

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

    private void OnUIKeycodeEscapePerformed()
    {
        if (!IsOpen) return;
        if (buttonBack != null && buttonBack.Button.interactable && buttonBack.gameObject.activeInHierarchy)
        {
            OnBackClicked();
        }
    }

    private void OnUIKeycodeRPerformed()
    {
        if (!IsOpen) return;
        if (wallTextGroup != null && wallTextGroup.gameObject.activeInHierarchy && 
            buttonReplay != null && buttonReplay.interactable)
        {
            OnReplayClicked();
        }
    }

    private void OnUIKeycodeEnterPerformed()
    {
        if (!IsOpen) return;
        if (wallTextGroup != null && wallTextGroup.gameObject.activeInHierarchy && 
            buttonContinueStory != null && buttonContinueStory.interactable)
        {
            OnContinueStoryClicked();
        }
    }
}