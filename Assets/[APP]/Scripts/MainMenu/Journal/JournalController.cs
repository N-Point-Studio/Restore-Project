using DG.Tweening;
using System;
using UnityEngine;
using MoreMountains.Feedbacks;

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
    [SerializeField] private Transform leftPageContainer;
    [SerializeField] private Transform rightPageContainer;

    [Header("Buttons & Interactions")]
    [SerializeField] private ButtonInputInstructionUI buttonAction;
    [SerializeField] private BookPanelInteractable bookInteractable;

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
        
        buttonAction.ForceSetText(false, "Let's Restore");
    }

    public void SetupPostRestoration(ArtefactData data, Action onContinueClicked)
    {
        currentData = data;
        currentActionCallback = onContinueClicked;
        isPostRestoration = true;

        buttonAction.ForceSetText(false, "Continue");
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

    private void SpawnAndInjectHalaman()
    {
        StopAllCoroutines();
        foreach (Transform child in leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in rightPageContainer) Destroy(child.gameObject);

        activeLeftPage = null;
        activeRightPage = null;

        if (!isPostRestoration) // FLOW PRE-RESTORATION
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.StickyNoteTexts);
            }
        }
        else // FLOW POST-RESTORATION
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.StickyNoteTexts);
            }

            if (currentData.PostRestorationPagePrefab != null)
            {
                activeRightPage = Instantiate(currentData.PostRestorationPagePrefab, rightPageContainer);
            }
        }
    }

    public void OpenBookFull()
    {
        JournalState prevState = CurrentState;

        CurrentState = JournalState.Opened;
        buttonAction.gameObject.SetActive(false);
        IsAnimating = true;
        ShowOverlay();

        OnBookOpened?.Invoke();

        if (!isContentRevealed)
        {
            SpawnAndInjectHalaman();
        }
        else
        {
            if (activeLeftPage != null) activeLeftPage.ShowInstant();
            if (activeRightPage != null) activeRightPage.ShowInstant();
        }

        if (prevState == JournalState.Hidden && feedbackOpenFromHidden != null)
        {
            feedbackOpenFromHidden.PlayFeedbacks();
        }
        else if (feedbackOpen != null)
        {
            feedbackOpen.PlayFeedbacks();
        }

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
            buttonAction.gameObject.SetActive(true);
            IsAnimating = false;
        }
    }

    public void HideBookToPeek()
    {
        CurrentState = JournalState.Peeking;
        IsAnimating = false;
        HideOverlay();
        buttonAction.gameObject.SetActive(false);
        
        if (feedbackPeek != null) feedbackPeek.PlayFeedbacks();
    }

    public void HideBookCompletely()
    {
        CurrentState = JournalState.Hidden;
        IsAnimating = false;
        HideOverlay();
        
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