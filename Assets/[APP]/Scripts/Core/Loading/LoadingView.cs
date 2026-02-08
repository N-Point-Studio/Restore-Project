using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Image loadingFill;
    
    [Header("Loading Text Animation")]
    [SerializeField] private float dotAnimationSpeed = 0.5f;
    
    private Coroutine loadingTextAnimation;
    private string baseLoadingText = "Loading";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        loadingPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        StopLoadingTextAnimation();
    }

    public void ShowLoading(string text = "Loading...")
    {
        loadingPanel.SetActive(true);
        
        // Extract base text without dots for animation
        if (text.EndsWith("..."))
        {
            baseLoadingText = text.Substring(0, text.Length - 3);
        }
        else if (text.EndsWith(".."))
        {
            baseLoadingText = text.Substring(0, text.Length - 2);
        }
        else if (text.EndsWith("."))
        {
            baseLoadingText = text.Substring(0, text.Length - 1);
        }
        else
        {
            baseLoadingText = text;
        }
        
        // Start animated dots
        StartLoadingTextAnimation();

        LoadingEvents.OnLoadingStarted?.Invoke();
    }

    public void HideLoading()
    {
        StopLoadingTextAnimation();
        loadingPanel.SetActive(false);

        LoadingEvents.OnLoadingFinished?.Invoke();
    }

    public void SetProgress(float progress)
    {
        loadingFill.fillAmount = progress;
    }
    
    private void StartLoadingTextAnimation()
    {
        StopLoadingTextAnimation();
        loadingTextAnimation = StartCoroutine(AnimateLoadingText());
    }
    
    private void StopLoadingTextAnimation()
    {
        if (loadingTextAnimation != null)
        {
            StopCoroutine(loadingTextAnimation);
            loadingTextAnimation = null;
        }
    }
    
    private IEnumerator AnimateLoadingText()
    {
        string[] dotStates = { "", ".", "..", "..." };
        int currentState = 0;
        
        while (true)
        {
            if (loadingText != null)
            {
                loadingText.text = baseLoadingText + dotStates[currentState];
            }
            
            currentState = (currentState + 1) % dotStates.Length;
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }
}
