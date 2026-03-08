using DG.Tweening;
using UnityEngine;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("UI Container")]
    [SerializeField] protected GameObject root;
    [SerializeField] protected CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] protected float fadeDuration = 0.25f;

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

        this.isActive = isActive;

        if (isActive)
        {
            root.SetActive(true);

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOFade(1, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOFade(0, fadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    root.SetActive(false);
                });
        }
    }
}

