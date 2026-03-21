using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public enum RevealStyle 
{ 
    FadeAndPop, 
    Typewriter, 
    Scribble 
}

[System.Serializable]
public class PageElement
{
    public CanvasGroup canvasGroup;
    public RevealStyle revealStyle = RevealStyle.FadeAndPop;
    public TMP_Text textComponent;
    
    [Tooltip("Set Image to Filed")]
    public Image imageComponent;
}

public class JournalPageAnimator : MonoBehaviour
{
    [Header("Animation Sequence")]
    [SerializeField] private PageElement[] sequenceElements;
    [SerializeField] private float delayBetweenElements = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private TMP_Text[] uiNoteTexts;

    public void PlayRevealAnimation(System.Action onComplete = null)
    {
        StartCoroutine(RevealSequence(onComplete));
    }

    private IEnumerator RevealSequence(System.Action onComplete)
    {
        if (sequenceElements != null)
        {
            for (int i = 0; i < sequenceElements.Length; i++)
            {
                PageElement element = sequenceElements[i];
                if (element == null || element.canvasGroup == null) continue;

                element.canvasGroup.DOFade(1f, fadeDuration);

                switch (element.revealStyle)
                {
                    case RevealStyle.FadeAndPop:
                        element.canvasGroup.transform.localScale = Vector3.one * 0.9f;
                        element.canvasGroup.transform.DOScale(1f, fadeDuration).SetEase(Ease.OutBack);
                        break;

                    case RevealStyle.Typewriter:
                        if (element.textComponent != null)
                        {
                            element.textComponent.maxVisibleCharacters = 0;
                            int totalChars = element.textComponent.text.Length;
                            
                            DOTween.To(() => element.textComponent.maxVisibleCharacters, 
                                       x => element.textComponent.maxVisibleCharacters = x, 
                                       totalChars, 
                                       fadeDuration * 2f)
                                   .SetEase(Ease.Linear);
                        }
                        break;

                    case RevealStyle.Scribble:
                        if (element.imageComponent != null)
                        {
                            element.imageComponent.fillAmount = 0f;
                            element.imageComponent.DOFillAmount(1f, fadeDuration * 1.5f).SetEase(Ease.InOutQuad);
                        }
                        break;
                }

                yield return new WaitForSeconds(delayBetweenElements);
            }
        }
        
        onComplete?.Invoke();
    }

    public void ShowInstant()
    {
        if (sequenceElements != null)
        {
            for (int i = 0; i < sequenceElements.Length; i++)
            {
                PageElement element = sequenceElements[i];
                if (element != null && element.canvasGroup != null)
                {
                    element.canvasGroup.alpha = 1f;
                    element.canvasGroup.transform.localScale = Vector3.one;

                    if (element.revealStyle == RevealStyle.Scribble && element.imageComponent != null)
                    {
                        element.imageComponent.fillAmount = 1f;
                    }

                    if (element.revealStyle == RevealStyle.Typewriter && element.textComponent != null)
                    {
                        element.textComponent.maxVisibleCharacters = 99999; 
                    }
                }
            }
        }
    }

    public void InjectTexts(string[] textsFromSSoT)
    {
        if (uiNoteTexts == null || textsFromSSoT == null) return;

        for (int i = 0; i < uiNoteTexts.Length; i++)
        {
            if (i < textsFromSSoT.Length && uiNoteTexts[i] != null)
            {
                uiNoteTexts[i].text = textsFromSSoT[i];
            }
        }
    }
}