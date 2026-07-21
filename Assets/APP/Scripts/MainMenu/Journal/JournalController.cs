using DG.Tweening;
using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Localization;

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

    [Header("Localization")]
    [SerializeField] private LocalizedString localizedRestoreText;
    [SerializeField] private LocalizedString localizedContinueText;

    public JournalState CurrentState { get; private set; } = JournalState.Hidden;
    public bool IsAnimating { get; private set; } = false;
    private ArtefactData currentData;
    private Action currentActionCallback;
    private bool isPostRestoration;
    private bool isContentRevealed = false;

    private JournalPageAnimator activeLeftPage;
    private JournalPageAnimator activeRightPage;

    public Action OnBookOpened;

    private InputSystemService input;

    private void Awake()
    {
        buttonAction.OnClick += OnButtonActionClicked;

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
        activeRightPage = null;

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
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.LocalizedStickyNoteTexts);
            }

            if (currentData.PostRestorationPagePrefab != null)
            {
                activeRightPage = Instantiate(currentData.PostRestorationPagePrefab, rightPageContainer);
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

        // Keep the GameObjects active so Coroutines never crash
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

        // 1. Hide visuals using the CanvasGroup, but ensure GameObjects are active
        ShowContent(false);

        if (!isContentRevealed)
        {
            SpawnAndInjectPage();
        }

        // 2. Play the Feel Feedback and listen for when it finishes
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
        // Clean up listeners
        if (feedbackOpen != null) feedbackOpen.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);
        if (feedbackOpenFromHidden != null) feedbackOpenFromHidden.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);

        // 3. Now that the book is physically open, make the container visible
        ShowContent(true);

        // 4. Trigger the text reveal Coroutines
        if (!isContentRevealed)
        {
            if (!isPostRestoration && activeLeftPage != null)
            {
                activeLeftPage.PlayRevealAnimation(() =>
                {
                    buttonAction.gameObject.SetActive(true);
                    IsAnimating = false;
                });
            }
            else if (isPostRestoration && activeRightPage != null)
            {
                activeRightPage.PlayRevealAnimation(() =>
                {
                    buttonAction.gameObject.SetActive(true);
                    IsAnimating = false;
                });
            }
            else
            {
                buttonAction.gameObject.SetActive(true);
                IsAnimating = false;
            }
            isContentRevealed = true;
        }
        else
        {
            if (activeLeftPage != null) activeLeftPage.ShowInstant();
            if (activeRightPage != null) activeRightPage.ShowInstant();

            buttonAction.gameObject.SetActive(true);
            IsAnimating = false;
        }
    }

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