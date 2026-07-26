using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

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
    [SerializeField] private LocalizedString localizedInstructOpenBox;
    [SerializeField] private LocalizedString localizedInstructPeelNote;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player wallTextRevealFeedback;

    [Header("Animation Timings")]
    [SerializeField] private float backButtonUnlockDelay = 1.5f;
    [SerializeField] private float wallTextFadeDuration = 0.5f;
    [SerializeField] private float closeMenuDuration = 0.3f;

    private ArtefactData currentArtefactData;
    private Artefact3DItem current3DItem;
    private ActiveArtefactData activeArtefactData;

    private bool isCompleted;
    private bool isClosing = false;
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

    private void OnEnable()
    {
        if (localizedInstructOpenBox != null) localizedInstructOpenBox.StringChanged += UpdateInstructionText;
        if (localizedInstructPeelNote != null) localizedInstructPeelNote.StringChanged += UpdateInstructionText;
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

        if (localizedInstructOpenBox != null) localizedInstructOpenBox.StringChanged -= UpdateInstructionText;
        if (localizedInstructPeelNote != null) localizedInstructPeelNote.StringChanged -= UpdateInstructionText;
        UnsubscribeArtefactData();
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

    private void UnsubscribeArtefactData()
    {
        if (currentArtefactData != null)
        {
            if (currentArtefactData.LocalizedItemName != null) currentArtefactData.LocalizedItemName.StringChanged -= UpdateWallTitleText;
            if (currentArtefactData.LocalizedItemDescription != null) currentArtefactData.LocalizedItemDescription.StringChanged -= UpdateWallDescText;
        }
    }

    private void UpdateWallTitleText(string value) { if (wallTitleText != null) wallTitleText.text = value; }
    private void UpdateWallDescText(string value) { if (wallDescText != null) wallDescText.text = value; }
    private void UpdateInstructionText(string value) { if (instructionText != null) instructionText.text = value; }

    public void OpenDetail(ArtefactData artefactData, ActiveArtefactData activeData, Artefact3DItem item3D, bool forceShowBook = false)
    {
        UnsubscribeArtefactData();
        isClosing = false;
        currentArtefactData = artefactData;
        activeArtefactData = activeData;
        current3DItem = item3D;

        isCompleted = activeArtefactData.IsArtefactCompleted(currentArtefactData.BaseData.Id);
        bool isStoryRead = activeArtefactData.IsStoryRead(currentArtefactData.BaseData.Id);

        if (currentArtefactData.LocalizedItemName != null) currentArtefactData.LocalizedItemName.StringChanged += UpdateWallTitleText;
        if (currentArtefactData.LocalizedItemDescription != null) currentArtefactData.LocalizedItemDescription.StringChanged += UpdateWallDescText;

        bool hasNotesRemaining = !isCompleted && !isStoryRead;
        if (current3DItem != null) current3DItem.SetDetailMode(true, hasNotesRemaining);

        wallTextGroup.alpha = 0;
        wallTextGroup.gameObject.SetActive(false);
        root.SetActive(true);
        canvasGroupRoot.alpha = 1;

        journalController.SetBookHiddenInstant();

        if (isCompleted)
        {
            instructionText.gameObject.SetActive(false);

            if (forceShowBook) // Just finished gameplay!
            {
                if (buttonBack != null) buttonBack.Button.interactable = false;
                DOVirtual.DelayedCall(backButtonUnlockDelay, () => { if (buttonBack != null) buttonBack.Button.interactable = true; });
                
                buttonBack.SetAlternateStyle(false);
                
                // Animate = false
                journalController.SetupPostRestoration(currentArtefactData, OnJournalContinueClicked, false);
                journalController.OpenBookFull();
            }
            else // Reopened from the Main Menu
            {
                if (buttonBack != null) buttonBack.Button.interactable = true;
                
                buttonBack.SetAlternateStyle(true);
                
                // Animate = true (Skip typing!)
                journalController.SetupPostRestoration(currentArtefactData, OnJournalContinueClicked, true);
                journalController.HideBookToPeek();
                
                wallTextGroup.gameObject.SetActive(true);
                if (currentArtefactData.LocalizedItemName != null) currentArtefactData.LocalizedItemName.RefreshString();
                if (currentArtefactData.LocalizedItemDescription != null) currentArtefactData.LocalizedItemDescription.RefreshString();
                
                if (wallTextRevealFeedback != null) wallTextRevealFeedback.PlayFeedbacks();
                else wallTextGroup.DOFade(1, wallTextFadeDuration);
            }
        }
        else
        {
            if (isStoryRead) // Reopened from menu, pre-restoration
            {
                instructionText.gameObject.SetActive(true);
                if (localizedInstructOpenBox != null) localizedInstructOpenBox.RefreshString();
                
                buttonBack.SetAlternateStyle(true);
                
                // Skip typing
                journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked, true);
                journalController.HideBookToPeek();
            }
            else // First time
            {
                instructionText.gameObject.SetActive(true);
                if (localizedInstructPeelNote != null) localizedInstructPeelNote.RefreshString();
                
                buttonBack.SetAlternateStyle(true);
                
                // Animate
                journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked, false);
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
            DOVirtual.DelayedCall(backButtonUnlockDelay, () => { if (buttonBack != null) buttonBack.Button.interactable = true; });
            buttonBack.SetAlternateStyle(false);

            // Just unlocked: Animate
            journalController.SetupPreRestoration(currentArtefactData, OnJournalRestoreClicked, false);
            journalController.OpenBookFull();
        }
    }

    private void HandleJournalOpenedFromClick()
    {
        if (buttonBack != null) buttonBack.SetAlternateStyle(false);
    }

    private void OnJournalRestoreClicked()
    {
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }

    private void OnJournalContinueClicked()
    {
        bool isAllArtefactsCompleted = true;
        
        var allArtefacts = activeArtefactData.GetArtefactDatabase().GetAllItems();
        
        foreach (var item in allArtefacts)
        {
            if (!activeArtefactData.IsArtefactCompleted(item.BaseData.Id))
            {
                isAllArtefactsCompleted = false;
                break;
            }
        }

        if (isAllArtefactsCompleted)
        {
            if (isClosing) return;
            isClosing = true;

            if (current3DItem != null)
            {
                current3DItem.SetDetailMode(false, false);
                current3DItem.Initialize(activeArtefactData);
            }
            CloseDetail();
            MainMenuEvents.TriggerCloseArtefactDetail();
            journalController.HideBookCompletely();
            
            MainMenuEvents.TriggerOpenCredits();
            return;
        }

        journalController.HideBookToPeek();
        if (buttonBack != null) buttonBack.SetAlternateStyle(true);

        wallTextGroup.gameObject.SetActive(true);
        if (currentArtefactData.LocalizedItemName != null) currentArtefactData.LocalizedItemName.RefreshString();
        if (currentArtefactData.LocalizedItemDescription != null) currentArtefactData.LocalizedItemDescription.RefreshString();
        
        if (wallTextRevealFeedback != null) wallTextRevealFeedback.PlayFeedbacks();
        else wallTextGroup.DOFade(1, wallTextFadeDuration);
    }

    private void OnBackClicked()
    {
        if (journalController != null && journalController.IsAnimating) return;

        if (journalController.CurrentState == JournalState.Opened)
        {
            journalController.HideBookToPeek();
            buttonBack.SetAlternateStyle(true);

            if (isCompleted)
            {
                wallTextGroup.gameObject.SetActive(true);
                if (currentArtefactData.LocalizedItemName != null) currentArtefactData.LocalizedItemName.RefreshString();
                if (currentArtefactData.LocalizedItemDescription != null) currentArtefactData.LocalizedItemDescription.RefreshString();
                wallTextGroup.DOFade(1, wallTextFadeDuration);
            }
            else
            {
                instructionText.gameObject.SetActive(true);
                if (localizedInstructOpenBox != null) localizedInstructOpenBox.RefreshString();
            }
        }
        else
        {
            if (isClosing) return;
            isClosing = true;

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
        if (isClosing) return;
        isClosing = true;

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
        canvasGroupRoot.DOFade(0f, closeMenuDuration).OnComplete(() => root.SetActive(false));
    }

    private void OnUIKeycodeEscapePerformed()
    {
        if (!IsOpen) return;
        if (journalController != null && journalController.IsAnimating) return;

        if (buttonBack != null && buttonBack.Button.interactable && buttonBack.gameObject.activeInHierarchy)
        {
            OnBackClicked();
        }
    }

    private void OnUIKeycodeRPerformed()
    {
        if (!IsOpen) return;
        if (journalController != null && journalController.CurrentState == JournalState.Opened) return;

        if (wallTextGroup != null && wallTextGroup.gameObject.activeInHierarchy &&
            buttonReplay != null && buttonReplay.interactable)
        {
            OnReplayClicked();
        }
    }

    private void OnUIKeycodeEnterPerformed()
    {
        if (!IsOpen) return;
        if (journalController != null && journalController.CurrentState == JournalState.Opened) return;

        if (wallTextGroup != null && wallTextGroup.gameObject.activeInHierarchy &&
            buttonContinueStory != null && buttonContinueStory.interactable)
        {
            OnContinueStoryClicked();
        }
    }
}