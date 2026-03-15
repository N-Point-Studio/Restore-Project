using UnityEngine;
using System.Collections;
using DG.Tweening;

public class JournalPageAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] sequenceElements;
    [SerializeField] private float delayBetweenElements = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;

    public void PlayRevealAnimation(System.Action onComplete = null)
    {
        StartCoroutine(RevealSequence(onComplete));
    }

    private IEnumerator RevealSequence(System.Action onComplete)
    {
        if (sequenceElements != null)
        {
            foreach (var element in sequenceElements)
            {
                if (element != null)
                {
                    element.transform.localScale = Vector3.one * 0.9f;
                    element.transform.DOScale(1f, fadeDuration).SetEase(Ease.OutBack);
                    element.DOFade(1f, fadeDuration);
                    
                    yield return new WaitForSeconds(delayBetweenElements);
                }
            }
        }
        
        onComplete?.Invoke();
    }

    public void ShowInstant()
    {
        if (sequenceElements != null)
        {
            foreach (var element in sequenceElements)
            {
                if (element != null)
                {
                    element.alpha = 1f;
                    element.transform.localScale = Vector3.one;
                }
            }
        }
    }
}