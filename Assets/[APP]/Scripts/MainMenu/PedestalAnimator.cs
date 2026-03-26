using UnityEngine;
using DG.Tweening;

public class PedestalAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float slideDownDistance = 2f;
    [SerializeField] private float defaultDuration = 0.5f;

    private Vector3 originalPosition;
    private bool isInitialized = false;

    private void InitializeIfNeeded()
    {
        if (!isInitialized)
        {
            originalPosition = transform.localPosition;
            isInitialized = true;
        }
    }

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void OnDestroy()
    {
        transform.DOKill();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r != null && r.material != null)
            {
                r.material.DOKill();
            }
        }
    }

    /// <summary>
    /// show = true (Appears & Moves Up), show = false (Fades & Moves Down)
    /// </summary>
    public void AnimatePedestal(bool show, float duration = -1f)
    {
        InitializeIfNeeded();
        float animDuration = duration > 0 ? duration : defaultDuration;
        
        transform.DOKill();

        Vector3 targetPos = show ? originalPosition : originalPosition + (Vector3.down * slideDownDistance);
        transform.DOLocalMove(targetPos, animDuration).SetEase(show ? Ease.OutBack : Ease.InBack);

        float targetAlpha = show ? 1f : 0f;
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r.material == null) continue;

            r.material.DOKill();

            if (r.material.HasProperty("_BaseColor"))
            {
                r.material.DOFade(targetAlpha, "_BaseColor", animDuration);
            }
            else if (r.material.HasProperty("_Color"))
            {
                r.material.DOFade(targetAlpha, "_Color", animDuration);
            }
        }
    }
}