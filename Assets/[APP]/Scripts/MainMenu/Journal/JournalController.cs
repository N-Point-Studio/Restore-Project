using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum JournalState { Hidden, Peeking, Opened }

public class JournalController : MonoBehaviour
{
    [Header("Overlay Animations")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected float fadeDuration = 0.25f;

    [Header("Book Animations")]
    [SerializeField] private RectTransform bookRect;
    [SerializeField] private Vector2 posHidden;
    [SerializeField] private Vector2 posPeek;
    [SerializeField] private Vector2 posPeekHovered; // Posisi saat di-hover
    [SerializeField] private Vector2 posOpened;
    [SerializeField] private float animDuration = 0.5f;

    [Header("Page Containers")]
    [SerializeField] private Transform leftPageContainer;
    [SerializeField] private Transform rightPageContainer;

    [Header("Buttons & Interactions")]
    [SerializeField] private Button buttonAction;
    [SerializeField] private TMP_Text buttonActionText;
    [SerializeField] private BookPanelInteractable bookInteractable;

    // --- STATE & DATA MEMORY ---
    public JournalState CurrentState { get; private set; } = JournalState.Hidden;
    private ArtefactData currentData;
    private Action currentActionCallback;
    private bool isPostRestoration;

    private JournalPageAnimator activeLeftPage;
    private JournalPageAnimator activeRightPage;

    private void Awake()
    {
        buttonAction.onClick.AddListener(OnButtonActionClicked);

        // Matikan overlay di awal
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        // Daftarkan event dari Interactable
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

    // --- 1. SETUP DATA ---
    public void SetupPreRestoration(ArtefactData data, Action onRestoreClicked)
    {
        currentData = data;
        currentActionCallback = onRestoreClicked;
        isPostRestoration = false;
        
        buttonActionText.text = "Let's Restore";
        OpenBookFull();
    }

    public void SetupPostRestoration(ArtefactData data, Action onContinueClicked)
    {
        currentData = data;
        currentActionCallback = onContinueClicked;
        isPostRestoration = true;

        buttonActionText.text = "Continue";
        OpenBookFull();
    }

    // --- 2. LOGIKA BUKA BUKU ---
    public void OpenBookFull()
    {
        CurrentState = JournalState.Opened;
        ResetBookPages();
        buttonAction.gameObject.SetActive(false);
        ShowOverlay();

        if (!isPostRestoration) // FLOW PRE-RESTORATION
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.StickyNoteTexts);
            }

            bookRect.DOAnchorPos(posOpened, animDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (activeLeftPage != null) activeLeftPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
                else buttonAction.gameObject.SetActive(true);
            });
        }
        else // FLOW POST-RESTORATION
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                activeLeftPage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                activeLeftPage.InjectTexts(currentData.StickyNoteTexts);
                activeLeftPage.ShowInstant();
            }

            if (currentData.PostRestorationPagePrefab != null)
            {
                activeRightPage = Instantiate(currentData.PostRestorationPagePrefab, rightPageContainer);
            }

            bookRect.DOAnchorPos(posOpened, animDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (activeRightPage != null) activeRightPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
                else buttonAction.gameObject.SetActive(true);
            });
        }
    }

    // --- 3. KONTROL POSISI BUKU ---
    public void HideBookToPeek()
    {
        CurrentState = JournalState.Peeking;
        HideOverlay();
        bookRect.DOAnchorPos(posPeek, animDuration).SetEase(Ease.InOutQuad);
    }

    public void HideBookCompletely()
    {
        CurrentState = JournalState.Hidden;
        HideOverlay();
        bookRect.DOAnchorPos(posHidden, animDuration).SetEase(Ease.InBack);
    }

    private void AnimateHoverPeek(bool isHovering)
    {
        Vector2 targetPos = isHovering ? posPeekHovered : posPeek;
        bookRect.DOAnchorPos(targetPos, 0.15f).SetEase(Ease.OutQuad);
    }

    private void OpenBookFromPeek()
    {
        OpenBookFull(); // Langsung buka pakai data yang tersimpan
    }

    // --- 4. OVERLAY & RESET ---
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

    private void ResetBookPages()
    {
        bookRect.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
        StopAllCoroutines();

        foreach (Transform child in leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in rightPageContainer) Destroy(child.gameObject);

        activeLeftPage = null;
        activeRightPage = null;
    }

    private void OnButtonActionClicked()
    {
        currentActionCallback?.Invoke();
    }

    // --- 5. EVENT HANDLERS DARI BOOK INTERACTABLE ---
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