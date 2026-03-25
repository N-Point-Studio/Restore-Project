using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] protected GameObject root;
    [SerializeField] protected CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] protected float fadeDuration = 0.25f;

    [Header("Feedbacks")]
    [SerializeField] protected MMF_Player showMenuFeedback;
    [SerializeField] protected MMF_Player hideMenuFeedback;

    private bool isActive;
    public bool IsActive => isActive;

    protected virtual void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0;
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
        if (showMenuFeedback != null) showMenuFeedback.Events.OnComplete.RemoveListener(OnShowComplete);
        if (hideMenuFeedback != null) hideMenuFeedback.Events.OnComplete.RemoveListener(OnHideComplete);
    }

    public virtual void SetActive(bool isActive)
    {
        if (root == null) return;

        if (canvasGroup == null)
        {
            root.SetActive(isActive);
            return;
        }

        canvasGroup.DOKill();
        if (showMenuFeedback != null) showMenuFeedback.StopFeedbacks();
        if (hideMenuFeedback != null) hideMenuFeedback.StopFeedbacks();

        this.isActive = isActive;

        if (isActive)
        {
            root.SetActive(true);

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (showMenuFeedback != null)
            {
                showMenuFeedback.Events.OnComplete.RemoveListener(OnShowComplete);
                showMenuFeedback.Events.OnComplete.AddListener(OnShowComplete);
                showMenuFeedback.PlayFeedbacks();
            }
            else
            {
                canvasGroup.DOFade(1, fadeDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(OnShowComplete);
            }
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (hideMenuFeedback != null)
            {
                hideMenuFeedback.Events.OnComplete.RemoveListener(OnHideComplete);
                hideMenuFeedback.Events.OnComplete.AddListener(OnHideComplete);
                hideMenuFeedback.PlayFeedbacks();
            }
            else
            {
                canvasGroup.DOFade(0, fadeDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(OnHideComplete);
            }
        }
    }

    private void OnShowComplete()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void OnHideComplete()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }
}

