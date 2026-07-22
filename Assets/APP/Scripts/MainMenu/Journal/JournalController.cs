using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using UnityEngine.Localization;
using TMPro;

public enum JournalState { Hidden, Peeking, Opened }

public class JournalController : MonoBehaviour
{
    [Header("Overlay Animations")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected float fadeDuration = 0.25f;

    [Header("Feel Feedbacks (Gerakan Utama)")]
    [SerializeField] private MMF_Player feedbackOpen;
    [SerializeField] private MMF_Player feedbackOpenFromHidden;
    [SerializeField] private MMF_Player feedbackPeek;
    [SerializeField] private MMF_Player feedbackHide;

    [Header("Book Transform & Hover (DOTween)")]
    [SerializeField] private RectTransform bookRect;
    [SerializeField] private Vector2 posHidden;
    [SerializeField] private Vector2 posPeek;
    [SerializeField] private Vector2 posPeekHovered;
    [SerializeField] private float hoverAnimDuration = 0.15f;

    [Header("Page Containers")]
    [SerializeField] private CanvasGroup pagesCanvasGroup;
    [SerializeField] private Transform leftPageContainer;
    [SerializeField] private Transform rightPageContainer;

    [Header("Buttons & Interactions")]
    [SerializeField] private ButtonInputInstructionUI buttonAction;
    [SerializeField] private BookPanelInteractable bookInteractable;
    
    [Header("Pagination Controls")]
    [SerializeField] private Button buttonNextPage;
    [SerializeField] private Button buttonPrevPage;
    [SerializeField] private GameObject paginationSeparator;
    [SerializeField] private TMP_Text textPageIndicator; // Optional: To show "1 / 2"

    [Header("Localization")]
    [SerializeField] private LocalizedString localizedRestoreText;
    [SerializeField] private LocalizedString localizedContinueText;

    public JournalState CurrentState { get; private set; } = JournalState.Hidden;
    public bool IsAnimating { get; private set; } = false;
    private ArtefactData currentData;
    private Action currentActionCallback;
    private bool isPostRestoration;
    private bool isContentRevealed = false;

    // Pagination State
    private int currentPageIndex = 0;
    private JournalPageAnimator activeLeftPage; // Pre-restoration
    private List<JournalPageAnimator> postPagesList = new List<JournalPageAnimator>(); // Post-restoration

    public Action OnBookOpened;

    private InputSystemService input;

    private void Awake()
    {
        buttonAction.OnClick += OnButtonActionClicked;

        if (buttonNextPage != null) buttonNextPage.onClick.AddListener(NextPage);
        if (buttonPrevPage != null) buttonPrevPage.onClick.AddListener(PrevPage);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (bookInteractable != null)
        {
            bookInteractable.OnBookHoverEnter += HandleBookHoverEntered;
            bookInteractable.OnBookHoverExit += HandleBookHoverExited;
            bookInteractable.OnBookClicked += HandleBookClicked;
        }
    }

    private void OnDestroy()
    {
        buttonAction.OnClick -= OnButtonActionClicked;
        
        if (buttonNextPage != null) buttonNextPage.onClick.RemoveListener(NextPage);
        if (buttonPrevPage != null) buttonPrevPage.onClick.RemoveListener(PrevPage);

        if (bookInteractable != null)
        {
            bookInteractable.OnBookHoverEnter -= HandleBookHoverEntered;
            bookInteractable.OnBookHoverExit -= HandleBookHoverExited;
            bookInteractable.OnBookClicked -= HandleBookClicked;
        }

        if (this.input != null)
        {
            this.input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
        }
    }

    public void SetInputSystemService(InputSystemService input)
    {
        if (this.input != null)
        {
            this.input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
        }

        this.input = input;

        if (this.input != null)
        {
            this.input.OnUIKeycodeEnterPerformed += OnUIKeycodeEnterPerformed;
        }
    }

    public void SetupPreRestoration(ArtefactData data, Action onRestoreClicked)
    {
        currentData = data;
        currentActionCallback = onRestoreClicked;
        isPostRestoration = false;
        buttonAction.ForceSetLocalizedText(false, localizedRestoreText);
    }

    public void SetupPostRestoration(ArtefactData data, Action onContinueClicked)
    {
        currentData = data;
        currentActionCallback = onContinueClicked;
        isPostRestoration = true;
        buttonAction.ForceSetLocalizedText(false, localizedContinueText);
    }

    public void SetBookHiddenInstant()
    {
        CurrentState = JournalState.Hidden;
        IsAnimating = false;
        isContentRevealed = false;

        if (feedbackOpen != null) feedbackOpen.StopFeedbacks();
        if (feedbackPeek != null) feedbackPeek.StopFeedbacks();
        if (feedbackHide != null) feedbackHide.StopFeedbacks();
        bookRect.DOKill();

        if (bookRect != null) bookRect.anchoredPosition = posHidden;
        HideOverlay();
    }

    private void SpawnAndInjectPage()
    {
        StopAllCoroutines();
        foreach (Transform child in leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in rightPageContainer) Destroy(child.gameObject);

        activeLeftPage = null;
        postPagesList.Clear();
        currentPageIndex = 0;

        if (!isPostRestoration)
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.LocalizedStickyNoteTexts);
            }
        }
        else
        {
            // Spawn Left Page (Sticky Notes)
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.LocalizedStickyNoteTexts);
            }

            // Spawn ALL Post Restoration Pages into the right container
            if (currentData.PostRestorationPagePrefabs != null && currentData.PostRestorationPagePrefabs.Length > 0)
            {
                for (int i = 0; i < currentData.PostRestorationPagePrefabs.Length; i++)
                {
                    JournalPageAnimator page = Instantiate(currentData.PostRestorationPagePrefabs[i], rightPageContainer);
                    page.gameObject.SetActive(i == 0); // Only show the first page initially
                    postPagesList.Add(page);
                }
            }
        }
    }

    private void ShowContent(bool isShowing)
    {
        if (pagesCanvasGroup != null)
        {
            pagesCanvasGroup.alpha = isShowing ? 1f : 0f;
            pagesCanvasGroup.blocksRaycasts = isShowing;
            pagesCanvasGroup.interactable = isShowing;
        }

        if (leftPageContainer != null) leftPageContainer.gameObject.SetActive(true);
        if (rightPageContainer != null) rightPageContainer.gameObject.SetActive(true);
    }

    public void OpenBookFull()
    {
        JournalState prevState = CurrentState;
        CurrentState = JournalState.Opened;
        buttonAction.gameObject.SetActive(false);
        IsAnimating = true;
        ShowOverlay();

        OnBookOpened?.Invoke();

        ShowContent(false);

        if (!isContentRevealed)
        {
            SpawnAndInjectPage();
        }

        MMF_Player targetFeedback = (prevState == JournalState.Hidden && feedbackOpenFromHidden != null)
            ? feedbackOpenFromHidden
            : feedbackOpen;

        if (targetFeedback != null)
        {
            targetFeedback.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);
            targetFeedback.Events.OnComplete.AddListener(OnBookOpenAnimationComplete);
            targetFeedback.PlayFeedbacks();
        }
        else
        {
            OnBookOpenAnimationComplete();
        }
    }

    private void OnBookOpenAnimationComplete()
    {
        if (feedbackOpen != null) feedbackOpen.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);
        if (feedbackOpenFromHidden != null) feedbackOpenFromHidden.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);

        ShowContent(true);

        if (!isContentRevealed)
        {
            if (!isPostRestoration && activeLeftPage != null)
            {
                activeLeftPage.PlayRevealAnimation(() =>
                {
                    UpdatePaginationUI();
                    IsAnimating = false;
                });
            }
            else if (isPostRestoration && postPagesList.Count > 0)
            {
                postPagesList[0].PlayRevealAnimation(() =>
                {
                    UpdatePaginationUI();
                    IsAnimating = false;
                });
            }
            else
            {
                UpdatePaginationUI();
                IsAnimating = false;
            }

            isContentRevealed = true;
        }
        else
        {
            if (activeLeftPage != null) activeLeftPage.ShowInstant();
            if (postPagesList.Count > 0) postPagesList[currentPageIndex].ShowInstant();

            UpdatePaginationUI();
            IsAnimating = false;
        }
    }

    #region Pagination Logic

    public void NextPage()
    {
        if (currentPageIndex < postPagesList.Count - 1)
        {
            ChangePage(currentPageIndex + 1);
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            ChangePage(currentPageIndex - 1);
        }
    }

    private void ChangePage(int newIndex)
    {
        if (postPagesList.Count == 0) return;

        // Hide old page
        postPagesList[currentPageIndex].gameObject.SetActive(false);

        // Show new page
        currentPageIndex = newIndex;
        postPagesList[currentPageIndex].gameObject.SetActive(true);
        postPagesList[currentPageIndex].ShowInstant(); 

        UpdatePaginationUI();
    }

    private void UpdatePaginationUI()
    {
        if (!isPostRestoration || postPagesList.Count <= 1)
        {
            // Single page (e.g., "Before" state)
            if (buttonNextPage != null) buttonNextPage.gameObject.SetActive(false);
            if (buttonPrevPage != null) buttonPrevPage.gameObject.SetActive(false);
            if (paginationSeparator != null) paginationSeparator.SetActive(false); // Hide pipe
            if (textPageIndicator != null) textPageIndicator.gameObject.SetActive(false);

            buttonAction.gameObject.SetActive(true); // Shows "Let's Restore"
            return;
        }

        // Multi-page logic
        bool showPrev = currentPageIndex > 0;
        bool showNext = currentPageIndex < postPagesList.Count - 1;
        bool showAction = currentPageIndex == postPagesList.Count - 1;

        if (buttonPrevPage != null) buttonPrevPage.gameObject.SetActive(showPrev);
        if (buttonNextPage != null) buttonNextPage.gameObject.SetActive(showNext);
        
        // The separator "|" should only show if "Previous" is visible
        if (paginationSeparator != null) paginationSeparator.SetActive(showPrev); 

        // This will swap seamlessly from "Next" to "Completed"
        buttonAction.gameObject.SetActive(showAction);

        if (textPageIndicator != null)
        {
            textPageIndicator.gameObject.SetActive(true);
            textPageIndicator.text = $"{currentPageIndex + 1} / {postPagesList.Count}";
        }
    }

    #endregion

    public void HideBookToPeek()
    {
        CurrentState = JournalState.Peeking;
        IsAnimating = false;
        HideOverlay();
        ShowContent(false);
        buttonAction.gameObject.SetActive(false);

        if (feedbackPeek != null) feedbackPeek.PlayFeedbacks();
    }

    public void HideBookCompletely()
    {
        CurrentState = JournalState.Hidden;
        IsAnimating = false;
        HideOverlay();
        ShowContent(false);

        if (feedbackHide != null) feedbackHide.PlayFeedbacks();
    }

    private void AnimateHoverPeek(bool isHovering)
    {
        Vector2 targetPos = isHovering ? posPeekHovered : posPeek;
        if (bookRect != null) bookRect.DOAnchorPos(targetPos, hoverAnimDuration).SetEase(Ease.OutQuad);
    }

    private void OpenBookFromPeek()
    {
        OpenBookFull();
    }

    private void ShowOverlay()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, fadeDuration);
        }
    }

    private void HideOverlay()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, fadeDuration);
        }
    }

    private void OnButtonActionClicked()
    {
        currentActionCallback?.Invoke();
    }

    private void HandleBookHoverEntered()
    {
        if (CurrentState == JournalState.Peeking) AnimateHoverPeek(true);
    }

    private void HandleBookHoverExited()
    {
        if (CurrentState == JournalState.Peeking) AnimateHoverPeek(false);
    }

    private void HandleBookClicked()
    {
        if (CurrentState == JournalState.Peeking) OpenBookFromPeek();
    }

    private void OnUIKeycodeEnterPerformed()
    {
        if (CurrentState == JournalState.Opened && buttonAction != null &&
            buttonAction.gameObject.activeInHierarchy && buttonAction.Button.interactable)
        {
            OnButtonActionClicked();
        }
    }
}