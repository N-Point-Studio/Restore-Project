using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalController : MonoBehaviour
{
    [Header("Book Animations")]
    [SerializeField] private RectTransform bookRect;
    [SerializeField] private Vector2 posHidden;
    [SerializeField] private Vector2 posPeek;
    [SerializeField] private Vector2 posOpened;
    [SerializeField] private float animDuration = 0.5f;

    [Header("Page Containers")]
    [SerializeField] private Transform leftPageContainer;  // Empty it, just a container for left prefab
    [SerializeField] private Transform rightPageContainer; // Empty it, just a container for right prefab

    [Header("Buttons")]
    [SerializeField] private Button buttonAction; 
    [SerializeField] private TMP_Text buttonActionText;

    private Action onActionCallback;
    private JournalPageAnimator activeLeftPage;
    private JournalPageAnimator activeRightPage;

    private void Awake()
    {
        buttonAction.onClick.AddListener(() => onActionCallback?.Invoke());
    }

    // --- PRE-RESTORATION (Not Played Yet) ---
    public void ShowPreRestoration(ArtefactData data, Action onRestoreClicked)
    {
        ResetBook();
        
        buttonActionText.text = "Let's Restore";
        onActionCallback = onRestoreClicked;
        buttonAction.gameObject.SetActive(false);

        // Spawn Left Page (Sticky notes & initial scribbles)
        if (data.PreRestorationPagePrefab != null)
        {
            activeLeftPage = Instantiate(data.PreRestorationPagePrefab, leftPageContainer);
        }

        bookRect.DOAnchorPos(posOpened, animDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (activeLeftPage != null)
            {
                // Play left page animation, when finished then show the Let's Restore button
                activeLeftPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
            }
            else
            {
                buttonAction.gameObject.SetActive(true);
            }
        });
    }

    // --- POST-RESTORATION (Already Played) ---
    public void ShowPostRestoration(ArtefactData data, Action onContinueClicked)
    {
        ResetBook();
        
        buttonActionText.text = "Continue";
        onActionCallback = onContinueClicked;
        buttonAction.gameObject.SetActive(false);

        // Spawn Left Page (Instant) & Right Page (Empty/Alpha 0)
        if (data.PreRestorationPagePrefab != null)
        {
            activeLeftPage = Instantiate(data.PreRestorationPagePrefab, leftPageContainer);
            activeLeftPage.ShowInstant(); // Directly visible because it's been seen before
        }

        if (data.PostRestorationPagePrefab != null)
        {
            activeRightPage = Instantiate(data.PostRestorationPagePrefab, rightPageContainer);
        }

        bookRect.DOAnchorPos(posOpened, animDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (activeRightPage != null)
            {
                // Play right page animation, when finished then show the Continue button
                activeRightPage.PlayRevealAnimation(() => buttonAction.gameObject.SetActive(true));
            }
            else
            {
                buttonAction.gameObject.SetActive(true);
            }
        });
    }

    public void HideBookToPeek()
    {
        bookRect.DOAnchorPos(posPeek, animDuration).SetEase(Ease.InOutQuad);
    }

    public void HideBookCompletely()
    {
        bookRect.DOAnchorPos(posHidden, animDuration).SetEase(Ease.InBack);
    }

    private void ResetBook()
    {
        bookRect.DOKill();
        StopAllCoroutines();
        
        // Delete old page contents
        foreach (Transform child in leftPageContainer) Destroy(child.gameObject);
        foreach (Transform child in rightPageContainer) Destroy(child.gameObject);
        
        activeLeftPage = null;
        activeRightPage = null;
        bookRect.anchoredPosition = posHidden;
    }
}