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
    private class JournalSpread
    {
        public JournalPageAnimator leftPage;
        public JournalPageAnimator rightPage;
        public bool hasBeenRevealed = false;

        public void SetActive(bool active)
        {
            if (leftPage != null) leftPage.gameObject.SetActive(active);
            if (rightPage != null) rightPage.gameObject.SetActive(active);
        }

        public void PrepareForReveal()
        {
            if (leftPage != null) leftPage.PrepareForReveal();
            if (rightPage != null) rightPage.PrepareForReveal();
        }

        public void ShowInstant()
        {
            if (leftPage != null) leftPage.ShowInstant();
            if (rightPage != null) rightPage.ShowInstant();
        }
    }

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
    [SerializeField] private float pageFadeDuration = 0.3f; 
    [SerializeField] private float pageHideFadeDuration = 0.1f; 

    [Header("Buttons & Interactions")]
    [SerializeField] private ButtonInputInstructionUI buttonAction;
    [SerializeField] private BookPanelInteractable bookInteractable;
    
    [Header("Pagination Controls")]
    [SerializeField] private Button buttonNextPage;
    [SerializeField] private Button buttonPrevPage;
    [SerializeField] private GameObject paginationSeparator;
    [SerializeField] private TMP_Text textPageIndicator;

    [Header("Localization")]
    [SerializeField] private LocalizedString localizedRestoreText;
    [SerializeField] private LocalizedString localizedContinueText;

    public JournalState CurrentState { get; private set; } = JournalState.Hidden;
    public bool IsAnimating { get; private set; } = false;
    private ArtefactData currentData;
    private Action currentActionCallback;
    private bool isPostRestoration;
    private bool isContentRevealed = false;
    
    private bool forceInstantReveal = false; 

    private int currentPageIndex = 0;
    private List<JournalSpread> spreads = new List<JournalSpread>();

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

        if (input != null)
        {
            input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
            input.OnUIKeycodeArrowRightPerformed -= OnUIKeycodeArrowRightPerformed;
            input.OnUIKeycodeArrowLeftPerformed -= OnUIKeycodeArrowleftPerformed;
        }
    }

    public void SetInputSystemService(InputSystemService input)
    {
        if (this.input != null) 
        {
            this.input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
            this.input.OnUIKeycodeArrowRightPerformed -= OnUIKeycodeArrowRightPerformed;
            this.input.OnUIKeycodeArrowLeftPerformed -= OnUIKeycodeArrowleftPerformed;
        }

        this.input = input;
        
        if (this.input != null)
        {
            this.input.OnUIKeycodeEnterPerformed += OnUIKeycodeEnterPerformed;
            input.OnUIKeycodeArrowRightPerformed += OnUIKeycodeArrowRightPerformed;
            input.OnUIKeycodeArrowLeftPerformed += OnUIKeycodeArrowleftPerformed;
        }
    }

    public void SetupPreRestoration(ArtefactData data, Action onRestoreClicked, bool instantReveal = false)
    {
        currentData = data;
        currentActionCallback = onRestoreClicked;
        isPostRestoration = false;
        forceInstantReveal = instantReveal;
        buttonAction.ForceSetLocalizedText(false, localizedRestoreText);
    }

    public void SetupPostRestoration(ArtefactData data, Action onContinueClicked, bool instantReveal = false)
    {
        currentData = data;
        currentActionCallback = onContinueClicked;
        isPostRestoration = true;
        forceInstantReveal = instantReveal;
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
        ShowContentInstant(false);
    }

    private void SpawnAndInjectPage()
    {
        StopAllCoroutines();
        foreach (Transform child in leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in rightPageContainer) Destroy(child.gameObject);

        spreads.Clear();
        currentPageIndex = 0;

        if (!isPostRestoration)
        {
            if (currentData.PreRestorationPagePrefab != null)
            {
                JournalPageAnimator prePage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                prePage.InjectTexts(currentData.LocalizedStickyNoteTexts);
                spreads.Add(new JournalSpread { leftPage = prePage });
            }
        }
        else
        {
            JournalSpread spread0 = new JournalSpread();
            
            if (currentData.PreRestorationPagePrefab != null)
            {
                JournalPageAnimator prePage = Instantiate(currentData.PreRestorationPagePrefab, leftPageContainer);
                prePage.InjectTexts(currentData.LocalizedStickyNoteTexts);
                spread0.leftPage = prePage;
            }

            spreads.Add(spread0);

            if (currentData.PostRestorationPagePrefabs != null && currentData.PostRestorationPagePrefabs.Length > 0)
            {
                for (int i = 0; i < currentData.PostRestorationPagePrefabs.Length; i++)
                {
                    JournalPageAnimator prefab = currentData.PostRestorationPagePrefabs[i];
                    
                    if (i == 0) 
                    {
                        JournalPageAnimator page = Instantiate(prefab, rightPageContainer);
                        spread0.rightPage = page;
                    }
                    else
                    {
                        int spreadIndex = (i + 1) / 2; 

                        if (spreads.Count <= spreadIndex) spreads.Add(new JournalSpread());

                        if (i % 2 != 0) 
                        {
                            JournalPageAnimator page = Instantiate(prefab, leftPageContainer);
                            spreads[spreadIndex].leftPage = page;
                        }
                        else 
                        {
                            JournalPageAnimator page = Instantiate(prefab, rightPageContainer);
                            spreads[spreadIndex].rightPage = page;
                        }
                    }
                }
            }
        }

        if (forceInstantReveal)
        {
            foreach (var spread in spreads)
            {
                spread.hasBeenRevealed = true;
            }
        }

        foreach (var spread in spreads) spread.SetActive(false);
    }

    private void ShowContentInstant(bool isShowing)
    {
        if (pagesCanvasGroup != null)
        {
            pagesCanvasGroup.DOKill();
            pagesCanvasGroup.alpha = isShowing ? 1f : 0f;
            pagesCanvasGroup.blocksRaycasts = isShowing;
            pagesCanvasGroup.interactable = isShowing;
        }

        if (leftPageContainer != null) leftPageContainer.gameObject.SetActive(true);
        if (rightPageContainer != null) rightPageContainer.gameObject.SetActive(true);
    }

    private void FadePagesContent(bool isShowing, float duration, Action onComplete = null)
    {
        if (leftPageContainer != null) leftPageContainer.gameObject.SetActive(true);
        if (rightPageContainer != null) rightPageContainer.gameObject.SetActive(true);

        if (pagesCanvasGroup != null)
        {
            pagesCanvasGroup.DOKill();
            pagesCanvasGroup.blocksRaycasts = isShowing;
            pagesCanvasGroup.interactable = isShowing;
            float targetAlpha = isShowing ? 1f : 0f;

            if (duration > 0f) pagesCanvasGroup.DOFade(targetAlpha, duration).OnComplete(() => onComplete?.Invoke());
            else
            {
                pagesCanvasGroup.alpha = targetAlpha;
                onComplete?.Invoke();
            }
        }
        else onComplete?.Invoke();
    }

    public void OpenBookFull()
    {
        JournalState prevState = CurrentState;
        CurrentState = JournalState.Opened;
        buttonAction.gameObject.SetActive(false);
        IsAnimating = true;
        ShowOverlay();

        OnBookOpened?.Invoke();
        ShowContentInstant(false);

        if (!isContentRevealed) SpawnAndInjectPage();

        MMF_Player targetFeedback = (prevState == JournalState.Hidden && feedbackOpenFromHidden != null)
            ? feedbackOpenFromHidden : feedbackOpen;

        if (targetFeedback != null)
        {
            targetFeedback.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);
            targetFeedback.Events.OnComplete.AddListener(OnBookOpenAnimationComplete);
            targetFeedback.PlayFeedbacks();
        }
        else OnBookOpenAnimationComplete();
    }

    private void OnBookOpenAnimationComplete()
    {
        if (feedbackOpen != null) feedbackOpen.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);
        if (feedbackOpenFromHidden != null) feedbackOpenFromHidden.Events.OnComplete.RemoveListener(OnBookOpenAnimationComplete);

        if (!isContentRevealed)
        {
            foreach (var spread in spreads) 
            {
                // Only hide elements if we actually want to animate them
                if (!spread.hasBeenRevealed) spread.PrepareForReveal();
            }
        }

        UpdatePaginationUI();
        if (!isContentRevealed) buttonAction.gameObject.SetActive(false);

        FadePagesContent(true, pageFadeDuration, () =>
        {
            RevealCurrentSpread(() => 
            {
                isContentRevealed = true;
                UpdatePaginationUI();
                IsAnimating = false;
            });
        });
    }

    private void RevealCurrentSpread(Action onComplete)
    {
        if (spreads.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        JournalSpread currentSpread = spreads[currentPageIndex];
        currentSpread.SetActive(true);

        if (!currentSpread.hasBeenRevealed)
        {
            currentSpread.PrepareForReveal();

            // 1. First Page Edge Case (Pre-restoration left side is already revealed)
            if (isPostRestoration && currentPageIndex == 0)
            {
                if (currentSpread.leftPage != null) currentSpread.leftPage.ShowInstant();
                if (currentSpread.rightPage != null)
                {
                    currentSpread.rightPage.PlayRevealAnimation(() => 
                    {
                        currentSpread.hasBeenRevealed = true;
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    currentSpread.hasBeenRevealed = true;
                    onComplete?.Invoke();
                }
            }
            // 2. Sequential Reveal (Left page first, THEN Right page)
            else
            {
                if (currentSpread.leftPage != null && currentSpread.rightPage != null)
                {
                    // Animate left page...
                    currentSpread.leftPage.PlayRevealAnimation(() => 
                    {
                        // ...then animate right page when left is done
                        currentSpread.rightPage.PlayRevealAnimation(() => 
                        {
                            currentSpread.hasBeenRevealed = true;
                            onComplete?.Invoke();
                        });
                    });
                }
                else if (currentSpread.leftPage != null)
                {
                    currentSpread.leftPage.PlayRevealAnimation(() => 
                    {
                        currentSpread.hasBeenRevealed = true;
                        onComplete?.Invoke();
                    });
                }
                else if (currentSpread.rightPage != null)
                {
                    currentSpread.rightPage.PlayRevealAnimation(() => 
                    {
                        currentSpread.hasBeenRevealed = true;
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    currentSpread.hasBeenRevealed = true;
                    onComplete?.Invoke();
                }
            }
        }
        else
        {
            currentSpread.ShowInstant();
            onComplete?.Invoke();
        }
    }

    #region Pagination Logic

    public void NextPage()
    {
        if (currentPageIndex < spreads.Count - 1) ChangePage(currentPageIndex + 1);
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0) ChangePage(currentPageIndex - 1);
    }

    private void ChangePage(int newIndex)
    {
        if (spreads.Count == 0) return;

        spreads[currentPageIndex].SetActive(false);
        currentPageIndex = newIndex;

        buttonAction.gameObject.SetActive(false);
        if (buttonNextPage != null) buttonNextPage.interactable = false;
        if (buttonPrevPage != null) buttonPrevPage.interactable = false;
        IsAnimating = true;

        RevealCurrentSpread(() => 
        {
            IsAnimating = false;
            
            UpdatePaginationUI(); 
        });
    }

    private void UpdatePaginationUI()
    {
        // 1. Single Page Edge Case (Pre-Restoration)
        if (spreads.Count <= 1)
        {
            if (buttonNextPage != null) buttonNextPage.gameObject.SetActive(false);
            if (buttonPrevPage != null) buttonPrevPage.gameObject.SetActive(false); // Completely hidden!
            if (paginationSeparator != null) paginationSeparator.SetActive(false);  // Completely hidden!
            if (textPageIndicator != null) textPageIndicator.gameObject.SetActive(false);

            buttonAction.gameObject.SetActive(true);
            return;
        }

        // 2. Multi-Page Journal Logic (Post-Restoration)
        bool hasPrev = currentPageIndex > 0;
        bool hasNext = currentPageIndex < spreads.Count - 1;
        bool showAction = currentPageIndex == spreads.Count - 1;

        if (buttonPrevPage != null) 
        {
            buttonPrevPage.gameObject.SetActive(true); // Always visible in multi-page mode
            buttonPrevPage.interactable = hasPrev;     // Greyed out on page 1, active on others
        }
        
        if (buttonNextPage != null) 
        {
            buttonNextPage.gameObject.SetActive(hasNext); // Replaced by the Action button on the last page
            buttonNextPage.interactable = true;
        }

        if (paginationSeparator != null) paginationSeparator.SetActive(true); // Always visible in multi-page mode

        buttonAction.gameObject.SetActive(showAction);

        if (textPageIndicator != null)
        {
            textPageIndicator.gameObject.SetActive(true);
            textPageIndicator.text = $"{currentPageIndex + 1}/{spreads.Count}";
        }
    }

    #endregion

    public void HideBookToPeek()
    {
        CurrentState = JournalState.Peeking;
        IsAnimating = false;
        HideOverlay();
        FadePagesContent(false, pageHideFadeDuration);
        buttonAction.gameObject.SetActive(false);

        if (feedbackPeek != null) feedbackPeek.PlayFeedbacks();
    }

    public void HideBookCompletely()
    {
        CurrentState = JournalState.Hidden;
        IsAnimating = false;
        HideOverlay();
        FadePagesContent(false, pageHideFadeDuration);

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

    private void OnUIKeycodeArrowRightPerformed()
    {
        if (CurrentState == JournalState.Opened && buttonNextPage != null &&
            buttonNextPage.gameObject.activeInHierarchy && buttonNextPage.interactable)
        {
            NextPage();
        }
    }

    private void OnUIKeycodeArrowleftPerformed()
    {
        if (CurrentState == JournalState.Opened && buttonPrevPage != null &&
            buttonPrevPage.gameObject.activeInHierarchy && buttonPrevPage.interactable)
        {
            PrevPage();
        }
    }
}