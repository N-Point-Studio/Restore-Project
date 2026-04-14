using System.Collections;
using TMPro;
using UnityEngine;

public class ProjectLoadingView : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    private string baseLoadingText = "Loading";
    private Coroutine loadingTextAnimation;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        loadingPanel.SetActive(false);
    }

    public void ShowLoading(string text)
    {
        loadingPanel.SetActive(true);
        
        if (text.EndsWith("...")) baseLoadingText = text.Substring(0, text.Length - 3);
        else if (text.EndsWith("..")) baseLoadingText = text.Substring(0, text.Length - 2);
        else if (text.EndsWith(".")) baseLoadingText = text.Substring(0, text.Length - 1);
        else baseLoadingText = text;
        
        if (loadingTextAnimation != null) StopCoroutine(loadingTextAnimation);
        loadingTextAnimation = StartCoroutine(AnimateLoadingText());
        
        LoadingEvents.OnLoadingStarted?.Invoke();
    }

    public void HideLoading()
    {
        loadingPanel.SetActive(false);
        LoadingEvents.OnLoadingFinished?.Invoke();
    }

    private IEnumerator AnimateLoadingText()
    {
        string[] dotStates = { "", ".", "..", "..." };
        int i = 0;
        while (true)
        {
            loadingText.text = baseLoadingText + dotStates[i];
            i = (i + 1) % dotStates.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }
}