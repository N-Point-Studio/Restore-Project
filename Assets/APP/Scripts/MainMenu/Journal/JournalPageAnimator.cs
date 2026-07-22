using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.Localization;
using System;

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
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private PageElement[] sequenceElements;
    [SerializeField] private float delayBetweenElements = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    
    [Tooltip("Make sure to assign your TMP_Text components here in the Inspector!")]
    [SerializeField] private TMP_Text[] uiNoteTexts;

    private LocalizedString[] currentLocalizedTexts;
    private LocalizedString.ChangeHandler[] textUpdateActions;

    private void OnDestroy()
    {
        if (currentLocalizedTexts != null && textUpdateActions != null)
        {
            for (int i = 0; i < currentLocalizedTexts.Length; i++)
            {
                if (i < textUpdateActions.Length && currentLocalizedTexts[i] != null && textUpdateActions[i] != null)
                {
                    currentLocalizedTexts[i].StringChanged -= textUpdateActions[i];
                }
            }
        }
    }

    public void PrepareForReveal()
    {
        if (sequenceElements != null)
        {
            foreach (var element in sequenceElements)
            {
                if (element == null || element.canvasGroup == null) continue;
                element.canvasGroup.alpha = 0f;
                
                if (element.revealStyle == RevealStyle.FadeAndPop)
                    element.canvasGroup.transform.localScale = Vector3.one * 0.9f;
                else if (element.revealStyle == RevealStyle.Typewriter && element.textComponent != null)
                    element.textComponent.maxVisibleCharacters = 0;
                else if (element.revealStyle == RevealStyle.Scribble && element.imageComponent != null)
                    element.imageComponent.fillAmount = 0f;
            }
        }
    }
    public void InjectTexts(LocalizedString[] textsFromSSoT)
    {
        if (uiNoteTexts == null || textsFromSSoT == null) return;
        
        currentLocalizedTexts = textsFromSSoT;
        textUpdateActions = new LocalizedString.ChangeHandler[uiNoteTexts.Length];

        for (int i = 0; i < uiNoteTexts.Length; i++)
        {
            if (i < textsFromSSoT.Length && uiNoteTexts[i] != null && textsFromSSoT[i] != null)
            {
                int index = i; 
                textUpdateActions[index] = (val) => 
                { 
                    if (uiNoteTexts[index] != null) 
                    {
                        uiNoteTexts[index].text = val; 
                        
                        if (uiNoteTexts[index].maxVisibleCharacters > 0)
                        {
                            uiNoteTexts[index].maxVisibleCharacters = 99999;
                        }
                    }
                };
                
                textsFromSSoT[index].StringChanged += textUpdateActions[index];
                textsFromSSoT[index].RefreshString();
            }
        }
    }

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
                if (element == null || element.canvasGroup == null) continue;

                element.canvasGroup.alpha = 0f;

                if (element.revealStyle == RevealStyle.FadeAndPop)
                    element.canvasGroup.transform.localScale = Vector3.one * 0.9f;
                else if (element.revealStyle == RevealStyle.Typewriter && element.textComponent != null)
                    element.textComponent.maxVisibleCharacters = 0;
                else if (element.revealStyle == RevealStyle.Scribble && element.imageComponent != null)
                    element.imageComponent.fillAmount = 0f;
            }
        }

        if (initialDelay > 0)
        {
            yield return new WaitForSeconds(initialDelay); 
        }

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
                        element.canvasGroup.transform.DOScale(1f, fadeDuration).SetEase(Ease.OutBack);
                        break;

                    case RevealStyle.Typewriter:
                        if (element.textComponent != null)
                        {
                            float progress = 0f;
                            DOTween.To(() => progress, 
                                       x => {
                                           progress = x;
                                           if (element.textComponent != null)
                                           {
                                               element.textComponent.maxVisibleCharacters = Mathf.CeilToInt(element.textComponent.text.Length * progress);
                                           }
                                       }, 
                                       1f, 
                                       fadeDuration * 2f)
                                   .SetEase(Ease.Linear);
                        }
                        break;

                    case RevealStyle.Scribble:
                        if (element.imageComponent != null)
                        {
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
}