using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public enum JournalState { Hidden, Peeking, Opened }

public class JournalController : MonoBehaviour
{
    [Header("Overlay Animations")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected float fadeDuration = 0.25f;

    [Header("Feel Feedbacks (Gerakan Utama)")]
    [SerializeField] private MMF_Player feedbackOpen; 
    [SerializeField] private MMF_Player feedbackPeek;
    [SerializeField] private MMF_Player feedbackHide;

    [Header("Book Transform & Hover (DOTween)")]
    [SerializeField] private RectTransform bookRect;
    [SerializeField] private Vector2 posHidden;
    [SerializeField] private Vector2 posPeek;
    [SerializeField] private Vector2 posPeekHovered;

    [Header("Page Containers")]
    [SerializeField] private Transform leftPageContainer;
    [SerializeField] private Transform rightPageContainer;

    [Header("Buttons & Interactions")]
    [SerializeField] private Button buttonAction;
    [SerializeField] private TMP_Text buttonActionText;
    [SerializeField] private BookPanelInteractable bookInteractable;

    public JournalState CurrentState { get; private set; } = JournalState.Hidden;
    private ArtefactData currentData;
    private Action currentActionCallback;
    private bool isPostRestoration;
    private bool isContentRevealed = false; 

    private JournalPageAnimator activeLeftPage;
    private JournalPageAnimator activeRightPage;

    public Action OnBookOpened;

    private void Awake()
    {
        buttonAction.onClick.AddListener(OnButtonActionClicked);

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
        buttonAction.onClick.RemoveListener(OnButtonActionClicked);

        if (bookInteractable != null)
        {
            bookInteractable.OnBookHoverEnter -= HandleBookHoverEntered;
            bookInteractable.OnBookHoverExit -= HandleBookHoverExited;
            bookInteractable.OnBookClicked -= HandleBookClicked;
        }
    }

    public void SetupPreRestoration(ArtefactData data, Action onRestoreClicked)
    {
        currentData = data;
        currentActionCallback = onRestoreClicked;
        isPostRestoration = false;
        
        buttonActionText.text = "Let's Restore";
    }

    public void SetupPostRestoration(ArtefactData data, Action onContinueClicked)
    {
        currentData = data;
        currentActionCallback = onContinueClicked;
        isPostRestoration = true;

        buttonActionText.text = "Continue";
    }

    public void SetBookHiddenInstant()
    {
        CurrentState = JournalState.Hidden;
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
        CurrentState = JournalState.Opened;
        buttonAction.gameObject.SetActive(false);
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

        if (feedbackOpen != null) feedbackOpen.PlayFeedbacks();

        if (!isContentRevealed)
        {
            if (!isPostRestoration && activeLeftPage != null)
            {
                activeLeftPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
            }
            else if (isPostRestoration && activeRightPage != null)
            {
                activeRightPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
            }
            else
            {
                buttonAction.gameObject.SetActive(true);
            }
            
            isContentRevealed = true;
        }
        else
        {
            buttonAction.gameObject.SetActive(true);
        }
    }

    public void HideBookToPeek()
    {
        CurrentState = JournalState.Peeking;
        HideOverlay();
        buttonAction.gameObject.SetActive(false);
        
        if (feedbackPeek != null) feedbackPeek.PlayFeedbacks();
    }

    public void HideBookCompletely()
    {
        CurrentState = JournalState.Hidden;
        HideOverlay();
        
        if (feedbackHide != null) feedbackHide.PlayFeedbacks();
    }

    private void AnimateHoverPeek(bool isHovering)
    {
        Vector2 targetPos = isHovering ? posPeekHovered : posPeek;
        if (bookRect != null) bookRect.DOAnchorPos(targetPos, 0.15f).SetEase(Ease.OutQuad);
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
}